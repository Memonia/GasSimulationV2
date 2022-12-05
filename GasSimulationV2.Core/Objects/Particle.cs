using GasSimulationV2.Core.Interface;
using GasSimulationV2.Utils.Math;

namespace GasSimulationV2.Core.Objects
{
	internal partial class Particle : IParticle, ICollidable
	{
		public StraightLine TrajectoryLine => 
			StraightLine.GetPointVectorLine(Center, Velocity);

		public Vector Velocity { get; set; }
		public Vector Center { get; set; }
		public double M { get; }
		public double R { get; }
		
		public Particle(Vector center, Vector v, double r)
		{
			M = Math.PI * r * r;
			R = r;
			Velocity = v;
			Center = center;
		}

		public Particle(Vector center, double r) : 
			this(center, new Vector(0, 0), r) 
			{ }

		public override string ToString() =>
			$"Point(" +
			$"\nMass: {M:.000}, " +
			$"\nRadius: {R:.000}, " +
			$"\nCenter: {Center}, " +
			$"\nVelocity: {Velocity}, " +
			$"\nTrajectory: {TrajectoryLine})";
	}
}
