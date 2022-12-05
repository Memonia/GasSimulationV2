using GasSimulationV2.Core.Interface;

namespace GasSimulationV2.GUI
{
	internal static class SpawnSettings
	{
		public const int MaxSpeedSpan = 10;
		public const int MaxRadiusSpan = (int)(_baseRadius - 1);
		public const int MinParticleAmount = 1;
		public const int MaxParticleAmount = 1024;
		public const int MaxSizeMultiplyer = 25;

		public static int? Seed { get; set; } = null;

		public static int SpeedSpan { get; set; } = 3;
		public static int RadiusSpan { get; set; } = 2;
		public static int ParticleAmount { get; set; } = 256;
		public static bool BigParticle { get; set; } = false;
		public static double SizeMultiplyer { get; set; } = 15;

		// _baseRadius - RadiusSpan >= 1
		private const double _baseRadius = 5;

		public static SpawnParameters SpawnParameters =>
			new
			(
				BaseRadius : _baseRadius,
				Amount:	ParticleAmount,
				SpeedSpan: SpeedSpan,
				RadiusSpan: RadiusSpan,
				BigParticle: BigParticle,
				SizeMultiplyer: SizeMultiplyer
			);
	}
}
