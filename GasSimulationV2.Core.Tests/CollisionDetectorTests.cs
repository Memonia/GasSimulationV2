using GasSimulationV2.Core.Collisions;
using GasSimulationV2.Core.Objects;

namespace GasSimulationV2.Core.Tests
{
	public class CollisionDetectorTests
	{
		public class IsSurfaceCollisionDetected
		{
			private readonly CollisionDetector _detector = new();

			[Fact]
			public void LeftWall_Away_StraightPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetLeftVerticalEnclosureAt(0);
				var p = new Particle(new(10, 0), new(-5, 0), 5);

				bool expected_res = true;
				double expected_time = 1;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void UpperWall_Away_StraightPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetUpperHorizontalEnclosureAt(0);
				var p = new Particle(new(0, 10), new(0, -5), 5);

				bool expected_res = true;
				double expected_time = 1;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void RightWall_Away_StraightPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetRightVerticalEnclosureAt(10);
				var p = new Particle(new(0, 0), new(5, 0), 5);

				bool expected_res = true;
				double expected_time = 1;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void BottomWall_Away_StraightPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetBottomHorizontalEnclosureAt(10);
				var p = new Particle(new(0, 0), new(0, 5), 5);

				bool expected_res = true;
				double expected_time = 1;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void LeftWall_Away_DiagonalPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetLeftVerticalEnclosureAt(0);
				var p = new Particle(new(10, 0), new(-5, -5), 5);

				bool expected_res = true;
				double expected_time = 1;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void UpperWall_Away_DiagonalPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetUpperHorizontalEnclosureAt(0);
				var p = new Particle(new(0, 10), new(-5, -5), 5);

				bool expected_res = true;
				double expected_time = 1;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void RightWall_Away_DiagonalPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetRightVerticalEnclosureAt(10);
				var p = new Particle(new(0, 0), new(5, 5), 5);

				bool expected_res = true;
				double expected_time = 1;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void BottomWall_Away_DiagonalPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetBottomHorizontalEnclosureAt(10);
				var p = new Particle(new(0, 0), new(5, 5), 5);

				bool expected_res = true;
				double expected_time = 1;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void LeftWall_Touch_StraightPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetLeftVerticalEnclosureAt(0);
				var p = new Particle(new(5, 0), new(-1, 0), 5);

				bool expected_res = true;
				double expected_time = 0;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);
				
				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void UpperWall_Touch_StraightPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetUpperHorizontalEnclosureAt(0);
				var p = new Particle(new(0, 5), new(0, -1), 5);

				bool expected_res = true;
				double expected_time = 0;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void RightWall_Touch_StraightPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetRightVerticalEnclosureAt(5);
				var p = new Particle(new(0, 0), new(1, 0), 5);

				bool expected_res = true;
				double expected_time = 0;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void BottomWall_Touch_StraightPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetBottomHorizontalEnclosureAt(5);
				var p = new Particle(new(0, 0), new(0, 1), 5);

				bool expected_res = true;
				double expected_time = 0;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void LeftWall_Touch_DiagonalPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetLeftVerticalEnclosureAt(0);
				var p = new Particle(new(5, 0), new(-1, 1), 5);

				bool expected_res = true;
				double expected_time = 0;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void UpperWall_Touch_DiagonalPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetUpperHorizontalEnclosureAt(0);
				var p = new Particle(new(0, 5), new(1, -1), 5);

				bool expected_res = true;
				double expected_time = 0;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void RightWall_Touch_DiagonalPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetRightVerticalEnclosureAt(5);
				var p = new Particle(new(0, 0), new(1, -1), 5);

				bool expected_res = true;
				double expected_time = 0;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}

			[Fact]
			public void BottomWall_Touch_DiagonalPath_DetectedAndCorrectTime()
			{
				var wall = Surface.GetBottomHorizontalEnclosureAt(5);
				var p = new Particle(new(0, 0), new(-1, 1), 5);

				bool expected_res = true;
				double expected_time = 0;

				bool res = _detector.IsCollisionDetected(p, wall, out double time);

				Assert.Equal(expected_res, res);
				Assert.Equal(expected_time, time);
			}
		}
	}
}