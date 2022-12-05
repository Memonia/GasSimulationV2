using GasSimulationV2.Core.Objects;

namespace GasSimulationV2.Core.Extensions
{
	internal static class ParticleExtensions
	{
		public static Particle DeepCopy(this Particle particle)
		{
			return new(particle.Center, particle.Velocity, particle.R);
		}

		public static IReadOnlyList<Particle> DeepCopy(this IReadOnlyList<Particle> list)
		{
			return list.Select(e => e.DeepCopy()).ToList();
		}
	}
}
