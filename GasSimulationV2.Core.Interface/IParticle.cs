using GasSimulationV2.Utils.Math;

namespace GasSimulationV2.Core.Interface
{
    public interface IParticle
    {
        double R { get; }

        Vector Center { get; }

        Vector Velocity { get; }

        StraightLine TrajectoryLine { get; }
    }
}
