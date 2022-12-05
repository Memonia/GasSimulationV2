namespace GasSimulationV2.Utils
{
	public class CustomRandom
	{
		private readonly Random _rand;

		public CustomRandom(int? seed = null)
		{
			if (seed.HasValue) 
				_rand = new(seed.Value);
			else 
				_rand = new();
		}

		// min and max inclusive
		public double NextDouble(double min, double max)
		{
			return (max - min) * _rand.NextDouble() + min;
		}		
	}
}
