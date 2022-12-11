using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using GasSimulationV2.Core.Interface;
using GasSimulationV2.Core.Native;

namespace GasSimulationV2.Core.Benchmarks
{
    public class Program
	{
		[SimpleJob]
		public class CollisionDetectionBenchmarks
		{
			private SpawnParameters _getSpawnParams(int amount) =>
				new(BaseRadius: 5, RadiusSpan: 0, SpeedSpan: 100,
						BigParticle: false, SizeMultiplyer: 0,
							Amount: amount
				);

			private readonly int _seed;
			private readonly int _iterations;
			private readonly Container _container;
			private readonly Simulator _cpuSimulator;
			private readonly NativeSimulator _gpuSimulator;

			public CollisionDetectionBenchmarks()
			{
				_seed = 123;
				_iterations = 1;
				_container = new(1000, 1000);

				_cpuSimulator = new(_container);
				_gpuSimulator = new(_container);
			}

			[Params(16, 64, 128, 512, 1024, Priority = 1)]
			public int ParticleAmount;

			[IterationSetup(Target = nameof(GPUSimulation))]
			public void ResetGPUSimulator()
			{
				_gpuSimulator.Reset();
				_gpuSimulator.SpawnParticles(
					_getSpawnParams(ParticleAmount), _seed
				);
			}

			[IterationSetup(Target = nameof(CPUSimulation))]
			public void ResetCPUSimulator()
			{
				_cpuSimulator.Reset();
				_cpuSimulator.SpawnParticles(
					_getSpawnParams(ParticleAmount), _seed
				);
			}

			[Benchmark]
			public void GPUSimulation()
			{
				for (int i = 0; i < _iterations; ++i)
					_gpuSimulator.Step();
			}

			[Benchmark(Baseline = true)]
			public void CPUSimulation()
			{
				for (int i = 0; i < _iterations; ++i)
					_cpuSimulator.Step();
			}
		}

		static void Main(string[] args)
		{
			var res = BenchmarkRunner.Run<CollisionDetectionBenchmarks>();
		}
	}
}