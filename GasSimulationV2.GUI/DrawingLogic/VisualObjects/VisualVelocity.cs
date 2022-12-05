using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GasSimulationV2.GUI.DrawingLogic.VisualObjects
{
    internal class VisualVelocity : Shape
    {
        private readonly List<Point> _points;

		private readonly PathFigure _pathFigure;

		private readonly PathGeometry _pathGeometry;

		public VisualVelocity()
        {
            var initP = new Point();
			_points = new(6) { initP, initP, initP, initP, initP , initP };

            _pathFigure = new()
            {
                Segments = new()
                {
                    new LineSegment() { IsStroked = true },
                    new LineSegment() { IsStroked = true },
                    new LineSegment() { IsStroked = true },
                    new LineSegment() { IsStroked = false },
                    new LineSegment() { IsStroked = true },
                    new LineSegment() { IsStroked = false },
                }
            };

            _pathGeometry = new PathGeometry { Figures = { _pathFigure } };
		}

        public void SetPoints(IList<Point> points)
        {
            if (points.Count != 6)
                throw new ArgumentException(
                    "The list must contain exactly 6 elements");

			/* 'DefiningGeometry' getter constructs the geometry on the fly,
			 * so the list has to be updated in a way that doesn't break when
			 * multiple threads access the object (one updates the points and
			 * the other gets the geometry).
			 * 
			 * Can also lock while updating the points, but this is nah */

			_points[0] = points[0];
            _points[1] = points[1];
            _points[2] = points[2];
            _points[3] = points[3];
            _points[4] = points[4];
            _points[5] = points[5];
		}

        protected override Geometry DefiningGeometry
        {
            get
            {
                _pathFigure.StartPoint = _points[0];
                ((LineSegment)_pathFigure.Segments[0]).Point = _points[0];
                ((LineSegment)_pathFigure.Segments[1]).Point = _points[1];
                ((LineSegment)_pathFigure.Segments[2]).Point = _points[2];
                ((LineSegment)_pathFigure.Segments[3]).Point = _points[3];
                ((LineSegment)_pathFigure.Segments[4]).Point = _points[4];
                ((LineSegment)_pathFigure.Segments[5]).Point = _points[5];

                return _pathGeometry;
            }
        }
    }
}
