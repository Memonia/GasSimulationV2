﻿using GasSimulationV2.Core.Objects;

namespace GasSimulationV2.Core.Collisions
{
	internal readonly record struct CollisionInfo
	(
		double Time,
		Particle Particle,
		ICollidable Collidable
	);
}
