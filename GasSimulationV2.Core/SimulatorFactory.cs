using GasSimulationV2.Core.Native;
using GasSimulationV2.Core.Interface;

using Microsoft.Extensions.Configuration;

namespace GasSimulationV2.Core
{
	public static class SimulatorFactory
	{
		private static readonly bool _enableGPU;

		static SimulatorFactory()
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile("core_config.json")
				.Build();

			_enableGPU = Boolean.Parse(config["enable_gpu"]);
		}

		public static ISimulator GetSimulator(double containerWidth, double containerHeight)
		{
			var container = new Container(containerWidth, containerHeight);

			if (_enableGPU)
				return new NativeSimulator(container);
			return new Simulator(container);
		}
	}
}
