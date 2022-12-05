namespace GasSimulationV2.Core.Interface
{
	public interface ISimulator : IDisposable
	{
		// Next() merely updates the values in particles,
		// so at any given time particle references are the same
		Frame CurrentFrame { get; }

		int QueueLength { get; }

		bool Next();

		void Reset();

		void SpinJoin();

		void Step();

		void StartSimulation(CancellationToken token);

		void SpawnParticles(SpawnParameters spawnParams, int? seed = null);
	}
}
