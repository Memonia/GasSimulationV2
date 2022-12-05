#ifndef STRUCTURES_CUH
#define STRUCTURES_CUH

#include "cuda_macros.cuh"

struct Vector
{
public:
	double Vx;
	double Vy;
	double Length;

	CUDA_CALLABLE_MEMBER Vector();
	CUDA_CALLABLE_MEMBER Vector(double vx, double vy);

	CUDA_CALLABLE_MEMBER Vector Normal() const;
	CUDA_CALLABLE_MEMBER Vector Normalised() const;
	CUDA_CALLABLE_MEMBER Vector Reflected(const Vector& n) const;

	CUDA_CALLABLE_MEMBER Vector operator-() const;
	CUDA_CALLABLE_MEMBER Vector operator-(const Vector v) const;
	CUDA_CALLABLE_MEMBER Vector operator+(const Vector v) const;
	CUDA_CALLABLE_MEMBER Vector operator*(double n) const;
	CUDA_CALLABLE_MEMBER double operator*(const Vector v) const;
};

struct StraightLine
{
private:
	const bool _notLine = false;

public:
	const double A;
	const double B;
	const double C;
	const Vector ParallelVector;

	CUDA_CALLABLE_MEMBER StraightLine(double a, double b, double c);
	CUDA_CALLABLE_MEMBER StraightLine(Vector p, Vector v);
	CUDA_CALLABLE_MEMBER double Distance(Vector p) const;
	CUDA_CALLABLE_MEMBER Vector ClosestPoint(Vector p) const;
	CUDA_CALLABLE_MEMBER Vector DistanceVectorFrom(Vector p) const;
};

class Surface
{
public:
	const Vector InBoundsNormal;
	const StraightLine LineEquation;

	CUDA_CALLABLE_MEMBER Surface(StraightLine lineEquation, Vector inBoundsNormal);
	CUDA_CALLABLE_MEMBER double SignedDistance(const Vector p) const;
};

class Particle
{
public:
	Vector Velocity;
	Vector Center;
	double R;
	double M;

	CUDA_CALLABLE_MEMBER Particle();
	CUDA_CALLABLE_MEMBER Particle(Vector center, Vector v, double r, double m);
};

struct Collidables
{
public:
	Surface* Surfaces;
	int SurfaceCount;
	Particle* Particles;
	int ParticleCount;
};

struct CollisionInfo
{
public:
	// inf if no collision happened
	double Time;

	// [0, particles.Count - 1]
	int ParticleIndex;

	// [0, particles.Count - 1] or
	// [particles.Count, particles.Count + surfaces.Count - 1]
	int CollidableIndex;
};

#endif // !STRUCTURES_CU
