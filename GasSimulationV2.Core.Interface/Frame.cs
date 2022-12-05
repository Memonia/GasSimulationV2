namespace GasSimulationV2.Core.Interface
{
	public readonly record struct Frame
	(
		int Collisions,
		IReadOnlyCollection<IParticle> Particles
	);
}
