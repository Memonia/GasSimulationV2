#include <stdio.h>
#include <math.h>
#include <algorithm> 

#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include "collisions.cuh"
#include "structures.cuh"

// There can be no more than 1024 particles
const dim3 g_block(32, 32);

__global__
void moveParticlesKernel(
    Particle* particles, int particleCount, double time)
{
    int i = threadIdx.x;
    if (i >= particleCount)
        return;

    auto& p{ particles[i] };
    p.Center = p.Center + p.Velocity * time;
}

__global__
void nearestParticleCollisionKernel(
    Particle* particles, int particleCount, CollisionInfo* out)
{
    int j = blockIdx.x;
    int i = threadIdx.x;
    if (j >= i || i >= particleCount || j >= particleCount)
        return;

    double time;
    if (isCollisionDetected(particles[i], particles[j], time))
    {
        if (time < out[i].Time)  
        {
            out[i].Time = time;
            out[i].ParticleIndex = i;
            out[i].CollidableIndex = j;
        }
    }
}

__global__
void nearestSurfaceCollisionKernel(
    Surface* surfaces, int surfaceCount,
    Particle* particles, int particleCount, CollisionInfo* out)
{
    int j = blockIdx.x;
    int i = threadIdx.x;
    if (i >= particleCount || j >= surfaceCount)
        return;

    double time;
    if (isCollisionDetected(particles[i], surfaces[j], time))
    {
        if (time < out[i].Time)
        {
            out[i].Time = time;
            out[i].ParticleIndex = i;

            // (see CollisionInfo)
            out[i].CollidableIndex = particleCount + j;
        }
    }
}

template<typename T>
T* cudaMallocHelper(size_t allocSize)
{
    static T* ptr = 0;
    static size_t prevSize = -1;
    if (prevSize != allocSize)
    {
        prevSize = allocSize;

        cudaError_t cudaStatus;
        cudaStatus = cudaFree(ptr);
        if (cudaStatus != cudaSuccess)
        {
            fprintf(stderr, "cudaFree failed!");
            return 0;
        }

        cudaStatus = cudaMalloc((void**)&ptr, allocSize);
        if (cudaStatus != cudaSuccess)
        {
            fprintf(stderr, "cudaMalloc failed!");
            return 0;
        }
    }

    return ptr;
}

cudaError_t detectionKernelsHelper(const Collidables& collidables, CollisionInfo* out)
{
    size_t surfacesSize = collidables.SurfaceCount * sizeof(Surface);
    size_t particlesSize = collidables.ParticleCount * sizeof(Particle);
    size_t resultsSize = collidables.ParticleCount * sizeof(CollisionInfo);

    auto devSurfaces = cudaMallocHelper<Surface>(surfacesSize);
    auto devParticles = cudaMallocHelper<Particle>(particlesSize);
    auto devResults = cudaMallocHelper<CollisionInfo>(resultsSize);

    cudaError_t cudaStatus;

    // Copy data to GPU
    cudaStatus = cudaMemcpy(devResults, out, resultsSize, cudaMemcpyHostToDevice);
    if (cudaStatus != cudaSuccess)
        goto Error;

    cudaStatus = cudaMemcpy(devSurfaces, collidables.Surfaces, surfacesSize, cudaMemcpyHostToDevice);
    if (cudaStatus != cudaSuccess)
        goto Error;

    cudaStatus = cudaMemcpy(devParticles, collidables.Particles, particlesSize, cudaMemcpyHostToDevice);
    if (cudaStatus != cudaSuccess)
        goto Error;

    // A block per surface with N threads for N particles
    nearestSurfaceCollisionKernel<<<collidables.SurfaceCount, collidables.ParticleCount>>>(
        devSurfaces, collidables.SurfaceCount, devParticles, collidables.ParticleCount, devResults
    );

    cudaStatus = cudaGetLastError();
    if (cudaStatus != cudaSuccess)
        goto Error;

    cudaStatus = cudaDeviceSynchronize();
    if (cudaStatus != cudaSuccess)
        goto Error;

    // A block per particle to be compared against
    nearestParticleCollisionKernel<<<collidables.ParticleCount, collidables.ParticleCount>>>(
        devParticles, collidables.ParticleCount, devResults
    );

    cudaStatus = cudaGetLastError();
    if (cudaStatus != cudaSuccess)
        goto Error;

    cudaStatus = cudaDeviceSynchronize();
    if (cudaStatus != cudaSuccess)
        goto Error;

    // Save results
    cudaStatus = cudaMemcpy(out, devResults, resultsSize, cudaMemcpyDeviceToHost);
    if (cudaStatus != cudaSuccess)
        goto Error;

Return:
    return cudaStatus;

Error:
    cudaFree(devResults);
    cudaFree(devSurfaces);
    cudaFree(devParticles);
    goto Return;
}

CollisionInfo detectNearestAndAdvanceCuda(const Collidables& collidables, double frameTime)
{
    const CollisionInfo farthest{ INFINITY, -1, -1 };
    if (collidables.ParticleCount == 0)
        return farthest;

    CollisionInfo* found;
    cudaError_t cudaStatus;

    cudaStatus = cudaSetDevice(0);
    if (cudaStatus != cudaSuccess)
        goto Error;
    
    // Reuse the same memory buffer if the size didn't change
    static size_t prevCount = -1;
    static CollisionInfo* results = 0;
    static CollisionInfo* resultsEnd = 0;
    if (prevCount != collidables.ParticleCount)
    {
        prevCount = collidables.ParticleCount;

        delete[] results;
        results = new CollisionInfo[collidables.ParticleCount];
        resultsEnd = results + collidables.ParticleCount;

        // Set results to default upon creation
        std::fill(results, resultsEnd, farthest);
    }

    // Clear up from the previous call
    std::fill(results, resultsEnd, farthest);

    // Detect collisions on GPU
    cudaStatus = detectionKernelsHelper(collidables, results);
    if (cudaStatus != cudaSuccess)
        goto Error;

    // Find nearest
    found = std::min_element(results, resultsEnd,
        [](auto& e1, auto& e2) { return e1.Time < e2.Time; }
    );

    if (found->Time < frameTime)
        frameTime = found->Time;

    // Move particles on GPU
    {
        auto size = collidables.ParticleCount * sizeof(Particle);

        // Particle array is still in GPU and was not overriden
        auto devParticles = cudaMallocHelper<Particle>(size);

        // A block of N threads for N particles
        moveParticlesKernel<<<1, collidables.ParticleCount>>>(
            devParticles, collidables.ParticleCount, frameTime
        );

        cudaStatus = cudaGetLastError();
        if (cudaStatus != cudaSuccess)
            goto Error;

        cudaStatus = cudaDeviceSynchronize();
        if (cudaStatus != cudaSuccess)
            goto Error;

        cudaStatus = cudaMemcpy(collidables.Particles, devParticles, size, cudaMemcpyDeviceToHost);
        if (cudaStatus != cudaSuccess)
            goto Error;
    }

Return:
    return *found;

Error:
    fprintf(stderr, "Error in CUDA: %s\n", cudaGetErrorString(cudaStatus));
    goto Return;
}