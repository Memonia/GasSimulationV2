#ifndef CUDA_MACROS_CUH
#define CUDA_MACROS_CUH

#ifdef __CUDACC__
#define CUDA_CALLABLE_MEMBER __host__ __device__
#else
#define CUDA_CALLABLE_MEMBER
#endif

#endif // !CUDA_MACROS_CUH
