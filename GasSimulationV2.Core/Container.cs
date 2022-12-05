using GasSimulationV2.Core.Collisions;
using GasSimulationV2.Core.Interface;
using GasSimulationV2.Core.Objects;
using GasSimulationV2.Utils;
using GasSimulationV2.Utils.Math;

namespace GasSimulationV2.Core
{
	internal class Container
	{
		private readonly double _width;
		private readonly double _height;

		private readonly List<Surface> _surfaces;
		private readonly List<Particle> _particles;

		public Collidables Collidables { get; private init; }

		public Container(double width, double height)
		{
			_width = width;
			_height = height;

			_surfaces = new List<Surface>()
			{
				Surface.GetLeftVerticalEnclosureAt(0),
				Surface.GetUpperHorizontalEnclosureAt(0),
				Surface.GetRightVerticalEnclosureAt(_width),
				Surface.GetBottomHorizontalEnclosureAt(_height)
			};

			_particles = new List<Particle>();
			Collidables = new Collidables(_surfaces, _particles);
		}

		private (double x, double y) _getRandomPos(double r, int? seed = null)
		{
			var rand = new CustomRandom(seed);

		getNewCenter:
			var center = new Vector
			(
				rand.NextDouble(r, _width - r),
				rand.NextDouble(r, _height - r)
			);

			foreach (var p in _particles)
			{
				// if overlaps with any other particle
				if (MathUtils.Distance(center, p.Center) <= r + p.R)
					goto getNewCenter;
			}

			return (center.Vx, center.Vy);
		}

		public void Clear()
		{
			_particles.Clear();
		}

		public void Spawn(SpawnParameters spawnParams, int? seed = null)
		{
			var rand = new CustomRandom(seed);
			Particle _getParticle(bool isBigParticle)
			{
				var sp = spawnParams;
				var vSpan = sp.SpeedSpan;

				double r = isBigParticle
					? sp.BaseRadius * sp.SizeMultiplyer

					// [baseRadius - sp.RadiusSpan; baseRadius + sp.RadiusSpan]
					: sp.BaseRadius + rand.NextDouble(-sp.RadiusSpan, sp.RadiusSpan);

				var (x, y) = _getRandomPos(r, seed);
				var vx = rand.NextDouble(-vSpan, vSpan);
				var vy = rand.NextDouble(-vSpan, vSpan);

				return new(new(x, y), new(vx, vy), r);
			}

			if (spawnParams.BaseRadius < spawnParams.RadiusSpan + 1)
				throw new ArgumentException(
					"'RadiusSpan' must be at least 1 less than 'BaseRadius'"
				);

			if (spawnParams.Amount < 1)
				throw new ArgumentException("'Amount' must be at least 1");

			_particles.Add(_getParticle(spawnParams.BigParticle));
			for (int amount = spawnParams.Amount; amount > 1; --amount)
				_particles.Add(_getParticle(false));
		}
	}
}
