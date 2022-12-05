using GasSimulationV2.Utils.Math;

namespace GasSimulationV2.Core.Objects
{
	internal class Surface : ICollidable
	{
		public static Surface GetLeftVerticalEnclosureAt(double x)
			=> new(StraightLine.GetVerticalLine(x), new Vector(1, 0));

		public static Surface GetRightVerticalEnclosureAt(double x)
			=> new(StraightLine.GetVerticalLine(x), new Vector(-1, 0));

		public static Surface GetUpperHorizontalEnclosureAt(double y)
			=> new(StraightLine.GetHorizontalLine(y), new Vector(0, 1));

		public static Surface GetBottomHorizontalEnclosureAt(double y)
			=> new(StraightLine.GetHorizontalLine(y), new Vector(0, -1));

		// Each surface knows which direction is inside the container
		public Vector InBoundsNormal { get; }

		public StraightLine LineEquation { get; }

		public Surface(StraightLine lineEquation, Vector inBoundsNormal)
		{
			LineEquation = lineEquation;
			InBoundsNormal = inBoundsNormal;
		}

		public double SignedDistance(Vector p)
		{
			double d = LineEquation.Distance(p);
			Vector? dVector = LineEquation.DistanceVectorFrom(p);

			// Determines whether the point is on the inside of the container
			if (InBoundsNormal * dVector! > 0)
				return d;
			return -d;
		}
	}
}
