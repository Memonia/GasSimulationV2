using static System.Math;
using static GasSimulationV2.Utils.Math.StraightLineType;

namespace GasSimulationV2.Utils.Math
{
	public readonly struct StraightLine
	{
		public static StraightLine GetVerticalLine(double x) =>
			new(a: -1, b: 0, c: x);

		public static StraightLine GetHorizontalLine(double y) =>
			new(a: 0, b: -1, c: y);

		public static StraightLine GetPointVectorLine(Vector p, Vector v) =>
			new(a: v.Vy, b: -v.Vx, c: v.Vx * p.Vy - v.Vy * p.Vx);

		public static StraightLine GetPointPointLine(Vector p1, Vector p2) =>
			new(a: p1.Vy - p2.Vy, b: p2.Vx - p1.Vx, c: p1.Vx * p2.Vy - p2.Vx * p1.Vy);

		public double A { get; }
		public double B { get; }
		public double C { get; }

		public Vector ParallelVector { get; init; }
		public StraightLineType Type { get; init; }

		private readonly StraightLineType[,] _typeLookup = 
		{
			{ NotLine, Horizontal }, { Vertical, Arbitrary }
		};

		public StraightLine(double a, double b, double c)
		{
			A = a;
			B = b;
			C = c;

			ParallelVector = new(-b, a);
			Type = _typeLookup[a == 0 ? 0 : 1, b == 0 ? 0 : 1];
		}

		public double GetX(double y)
		{
			if (Type != Horizontal)
				return -(B * y + C) / A;
			return double.NaN;
		}

		public double GetY(double x)
		{
			if (Type != Vertical)
				return -(A * x + C) / B;
			return double.NaN;
		}

		public double Distance(Vector p)
		{
			if (Type != NotLine)
				return Abs(A * p.Vx + B * p.Vy + C) / Sqrt(A * A + B * B);
			return double.NaN;
		}

		public Vector? DistanceVectorTo(Vector p)
		{
			if (Type != NotLine)
				return ClosestPoint(p)! - p;
			return null;
		}

		public Vector? DistanceVectorFrom(Vector p)
		{
			if (Type != NotLine)
				return p - ClosestPoint(p)!;
			return null;
		}

		public Vector? ClosestPoint(Vector p)
		{
			if (Type == NotLine)
				return null;

			double AApBB = A * A + B * B;
			double x = ((B * (B * p.Vx - A * p.Vy)) - A * C) / AApBB;
			double y = ((A * ((-B * p.Vx) + (A * p.Vy))) - B * C) / AApBB;

			return new(x, y);
		}

		public override string ToString() => $"StraightLine\\{Type}" +
			$"(A: {A:0.000}, B: {B:0.000}, C: {C:0.000})";
	}
}