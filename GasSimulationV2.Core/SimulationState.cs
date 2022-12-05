using GasSimulationV2.Core.Objects;

namespace GasSimulationV2.Core
{
	internal struct SimulationState
    {
		public int Collisions;
		public IReadOnlyList<Particle> Particles;
	}
}
