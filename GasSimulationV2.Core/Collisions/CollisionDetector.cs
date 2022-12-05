using GasSimulationV2.Core.Objects;
using GasSimulationV2.Utils.Math;

using static GasSimulationV2.Utils.Math.MathUtils;

namespace GasSimulationV2.Core.Collisions
{
	internal class CollisionDetector
    {
		private static readonly CollisionInfo 
			_farthest = new(double.PositiveInfinity, null!, null!);

		private CollisionInfo _nearest(Particle p, Surface target)
		{
			if (IsCollisionDetected(p, target, out double time))
				return new(time, p, target);
			return _farthest;
		}

		private CollisionInfo _nearest(Particle p, Particle target)
		{
			if (IsCollisionDetected(p, target, out double time))
				return new(time, p, target);
			return _farthest;
		}

		// Too much struct creation, but the struct is sufficienly small,
		// so it's as good as keeping one struct and passing by reference
		// in terms of performance (was benchmarked)
		public CollisionInfo NearestCollision(Collidables cs, CancellationToken token)
		{
			// Each particle is checked against all other particles
			var nearest = _farthest;
			for (int i = 0; i < cs.Particles.Count; ++i)
			{
				// Iteration starts with the given particle, instead of the beginning 
				// of the list, because the collisions with previous particles in the
				// list were already checked at this point
				// (if p1 collides with p2, then p2 collides with p1)

				// i + 1 because a particle cannot collide with itself
				CollisionInfo nearestFound;
				for (int j = i + 1; j < cs.Particles.Count; ++j)
				{
					if (token.IsCancellationRequested)
						return _farthest;

					nearestFound = _nearest(cs.Particles[i], cs.Particles[j]);
					if (nearestFound.Time < nearest.Time)
						nearest = nearestFound;
				}

				// Each particle is also checked against all surfaces
				for (int j = 0; j < cs.Surfaces.Count; ++j)
				{
					if (token.IsCancellationRequested)
						return _farthest;

					nearestFound = _nearest(cs.Particles[i], cs.Surfaces[j]);
					if (nearestFound.Time < nearest.Time)
						nearest = nearestFound;
				}
			}

			return nearest;
        }

		public bool IsCollisionDetected(Particle p, Surface target, out double time)
		{
			time = double.NaN;

			// Closest point is never null, because surface is always a line
			Vector closest = (Vector)target.LineEquation.ClosestPoint(p.Center)!;
			Vector dVector = closest - p.Center;

			// Same vector direction check as in particle detection
			if (p.Velocity * dVector <= 0)
				return false;

			double sc = target.SignedDistance(p.Center);
			double se = target.SignedDistance(p.Center + p.Velocity);
			if (sc * se > 0 && Math.Abs(sc) > p.R && Math.Abs(se) > p.R)
				return false;

			time = (sc - p.R) / (sc - se);
			return true;
		}

		public bool IsCollisionDetected(Particle p, Particle target, out double time)
		{
			/* Collision of two particles can be thought of a collision of
             * a point and a stationary particle of radius p1.R + p.R. 
             * 
             * To tell whether a point and a stationary particle collide, 
             * we do 3 steps:
             * 
             *  1. Check if a particle lies on a trajectory line of a point.
             *  2. Check if the velocity vector of a point is big enough
             * to reach the particle.
             *  3. Check if the point moves towards the particle along its
             * trajectory. 
             *
             * If one of those conditinons fails, a collision cannot occur. */

			time = double.NaN;

			// Point's velocity 
			Vector Vab = p.Velocity - target.Velocity;

			// Check if velocity vector of a stationary point
			// is less than the distance between the two particles.
			// If the distance is bigger, then a collision cannot occur
			if (Distance(p.Center, target.Center) - (p.R + target.R) > Vab.Length)
				return false;

			// If trajectory line doesn't cross or touch the
			// stationary particle, then a collision cannot happen.
			if (StraightLine
					.GetPointVectorLine(p.Center, Vab)
					.Distance(target.Center) > p.R + target.R)
				return false;

			/* We know that the point is on trajectory to the stationary particle
			 * and that the velocity vector is long enough to reach the particle,
			 * now we need to know whether it goes in the correct direction, for
			 * which we take a dot product of distance vector between a particle
			 * and a point (order is important) and particle's velocity vector */

			// Reversing the vector, because we need a vector from point to
			// stationary particle, but the distance vector used in later
			// calculations is the reverse of it
			Vector dVector = p.Center - target.Center;
			if (Vab * (-dVector) <= 0)
				return false;

			// All checks passed, so the collision will happen.
			// Now we need to find when
			double A = Vab * Vab;
			double B = dVector * Vab * 2;
			double C = dVector * dVector - (p.R + target.R) * (p.R + target.R);
			double Q = -(B + Sign(B) * Math.Sqrt(B * B - 4 * A * C)) / 2;

			// An extra root comes from the fact that the equation counts
			// "entering" and "leaving" the object as two separate collisions
			double t0 = Q / A;
			double t1 = C / Q;

			// The nearest of the two collisions is "enterting" the object
			time = Math.Min(t0, t1);
			return true;
		}
	}
}
