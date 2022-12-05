using GasSimulationV2.Core.Objects;

namespace GasSimulationV2.Core.Collisions
{
    internal readonly record struct Collidables
    (
        IReadOnlyList<Surface> Surfaces,
        IReadOnlyList<Particle> Particles
    );
}
