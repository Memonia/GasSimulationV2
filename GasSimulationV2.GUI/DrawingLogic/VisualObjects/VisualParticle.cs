using System.Windows.Media;
using System.Windows.Shapes;

using GasSimulationV2.Core.Interface;

namespace GasSimulationV2.GUI.DrawingLogic.VisualObjects
{
    internal class VisualParticle : Shape
    {
        public IParticle Particle { get; init; }

        public bool IsSlowest { get; set; }
        public bool IsFastest { get; set; }
        public bool IsTracked { get; private set; }
        public VisualVelocity Velocity { get; init; }
        public VisualTrajectory Trajectory { get; init; }

        public VisualParticle(IParticle particle)
        {
            Particle = particle;
            Velocity = new VisualVelocity();
            Trajectory = new VisualTrajectory();

            _ellipseGeometry =
                new() { RadiusX = particle.R, RadiusY = particle.R, };

            MouseDown += (_, _) => IsTracked = !IsTracked;
        }

        private readonly EllipseGeometry _ellipseGeometry;

        protected override Geometry DefiningGeometry => _ellipseGeometry;
    }
}
