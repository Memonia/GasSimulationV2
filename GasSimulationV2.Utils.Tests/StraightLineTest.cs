using GasSimulationV2.Utils.Math;

using Xunit.Sdk;

namespace GasSimulationV2.Utils.Tests
{
	public class StraightLineTest
	{
		public class Constructor
		{
			private void _assertSameLines(
				Vector p1,
				Vector p2, 
				StraightLine l)
			{
				// If a given line contain two given points, then the
				// equation represents the same line as those two points

				if (l.Type == StraightLineType.Horizontal)
				{
					Assert.Equal(l.GetY(p1.Vx), p1.Vy);
					Assert.Equal(l.GetY(p2.Vx), p2.Vy);
				}

				else
				if (l.Type == StraightLineType.Vertical)
				{
					Assert.Equal(l.GetX(p1.Vy), p1.Vx);
					Assert.Equal(l.GetX(p2.Vy), p2.Vx);
				}

				else
				if (l.Type == StraightLineType.Arbitrary)
				{
					Assert.Equal(l.GetX(p1.Vy), p1.Vx);
					Assert.Equal(l.GetY(p1.Vx), p1.Vy);
					Assert.Equal(l.GetX(p2.Vy), p2.Vx);
					Assert.Equal(l.GetY(p2.Vx), p2.Vy);
				}

				else
					throw new EqualException(true, false);
			}

			[Fact]
			public void ArbitraryLine_SameObjects()
			{
				// Equation: -4x - 2y + 4 = 0
				// Parallel vector: {-4, 8}
				// x-axis intersection: (1, 0)
				// y-axis intersection: (0, 2)

				double A = -4;
				double B = -2;
				double C = +4;

				Vector xIntersection = new(1, 0);
				Vector yIntersection = new(0, 2);
				Vector pVector = new(-4, 8);

				StraightLine l1 = new(A, B, C);
				StraightLine l2 = StraightLine.GetPointVectorLine(xIntersection, pVector);
				StraightLine l3 = StraightLine.GetPointPointLine(xIntersection, yIntersection);

				Assert.All(new StraightLine[] { l1, l2, l3 },
					e => _assertSameLines(xIntersection, yIntersection, e)
				);
			}

			[Fact]
			public void VerticalLine_SameObjects()
			{
				// Equation: -4x + 4 = 0
				// Parallel vector: {0, 2}
				// x-axis intersection: (1, 0)
				// arbitrary point: (1, 4)

				double A = -4;
				double B = +0;
				double C = +4;

				Vector xIntersection = new(1, 0);
				Vector arbitraryPoint = new(1, 4);
				Vector pVector = new(0, 2);

				StraightLine l1 = new(A, B, C);
				StraightLine l2 = StraightLine.GetPointVectorLine(xIntersection, pVector);
				StraightLine l3 = StraightLine.GetPointPointLine(xIntersection, arbitraryPoint);

				Assert.All(new StraightLine[] { l1, l2, l3 },
					e => _assertSameLines(xIntersection, arbitraryPoint, e)
				);
			}

			[Fact]
			public void HorizontalLine_SameObjects()
			{
				// Equation: -4y + 4 = 0
				// Parallel vector: {-2, 0}
				// y-axis intersection: (0, 1)
				// arbitrary point: (4, 1)

				double A = +0;
				double B = -4;
				double C = +4;
				
				Vector yIntersection = new(0, 1);
				Vector arbitraryPoint = new(4, 1);
				Vector pVector = new(-2, 0);

				StraightLine l1 = new(A, B, C);
				StraightLine l2 = StraightLine.GetPointVectorLine(yIntersection, pVector);
				StraightLine l3 = StraightLine.GetPointPointLine(yIntersection, arbitraryPoint);

				Assert.All(new StraightLine[] { l1, l2, l3 },
					e => _assertSameLines(yIntersection, arbitraryPoint, e)
				);
			}
		}
	}
}