#include <math.h>
#include <cstring>

#include "structures.cuh"

/* -------------------------- ------ -------------------------- */
/* -------------------------- Vector -------------------------- */
/* -------------------------- ------ -------------------------- */

Vector::Vector(double vx, double vy) :
	Vx{ vx },
	Vy{ vy },
	Length{ sqrt(vx * vx + vy * vy) }
{ }

Vector::Vector()
{
	std::memset(this, 0, sizeof(*this));
}

Vector Vector::Normal() const
{
	return Vector(Vy, -Vx);
}

Vector Vector::Normalised() const
{
	return Vector(Vx / Length, Vy / Length);
}

Vector Vector::Reflected(const Vector& n) const
{
	auto ned = n.Normalised();
	return *this - ned * (*this * ned) * 2;
}

Vector Vector::operator-() const
{
	return Vector(-Vx, -Vy);
}

Vector Vector::operator-(const Vector v) const
{
	return Vector(Vx - v.Vx, Vy - v.Vy);
}

Vector Vector::operator+(const Vector v) const
{
	return Vector(Vx + v.Vx, Vy + v.Vy);
}

Vector Vector::operator*(double n) const
{
	return Vector(Vx * n, Vy * n);
}

double Vector::operator*(const Vector v) const
{
	return Vx * v.Vx + Vy * v.Vy;
}

/* -------------------------- ------------ -------------------------- */
/* -------------------------- StraightLine -------------------------- */
/* -------------------------- ------------ -------------------------- */

StraightLine::StraightLine(double a, double b, double c) :
	A{ a }, B{ b }, C{ c },
	ParallelVector{ Vector(-b, a) },
	_notLine{ (a == 0) && (b == 0) }
{ }

StraightLine::StraightLine(Vector p, Vector v) :
	StraightLine
	(
		v.Vy,
		-v.Vx,
		v.Vx * p.Vy - v.Vy * p.Vx
	)
{ }

double StraightLine::Distance(Vector p) const
{
	if (!_notLine)
		return fabs(A * p.Vx + B * p.Vy + C) / sqrt(A * A + B * B);
	return NAN;
}

Vector StraightLine::ClosestPoint(Vector p) const
{
	if (_notLine)
		return Vector(0, 0);

	const double AApBB = A * A + B * B;
	double x = ((B * (B * p.Vx - A * p.Vy)) - A * C) / AApBB;
	double y = ((A * ((-B * p.Vx) + (A * p.Vy))) - B * C) / AApBB;

	return Vector(x, y);
}

Vector StraightLine::DistanceVectorFrom(Vector p) const
{
	if (!_notLine)
		return p - ClosestPoint(p);
	return Vector(0, 0);
}

/* -------------------------- ------- -------------------------- */
/* -------------------------- Surface -------------------------- */
/* -------------------------- ------- -------------------------- */

Surface::Surface(StraightLine lineEquation, Vector inBoundsNormal) :
	LineEquation{ lineEquation },
	InBoundsNormal{ inBoundsNormal }
{ }

double Surface::SignedDistance(const Vector p) const
{
	double d = LineEquation.Distance(p);
	Vector dVector = LineEquation.DistanceVectorFrom(p);

	if (InBoundsNormal * dVector > 0)
		return d;
	return -d;
}

/* -------------------------- -------- -------------------------- */
/* -------------------------- Particle -------------------------- */
/* -------------------------- -------- -------------------------- */

Particle::Particle(Vector center, Vector v, double r, double m) :
	Velocity{ v },
	Center{ center },
	R{ r },
	M{ m }
{ }

Particle::Particle()
{
	std::memset(this, 0, sizeof(*this));
}
