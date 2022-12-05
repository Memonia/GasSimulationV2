using GasSimulationV2.Core.Native.StructureWrappers;
using GasSimulationV2.Core.Objects;
using GasSimulationV2.Utils.Math;

namespace GasSimulationV2.Core.Extensions
{
    internal static class NativeConverterExtensions
    {
        public static NativeParticle ToNative(this Particle particle)
        {
            return new()
            {
                Velocity = particle.Velocity.ToNative(),
                Center = particle.Center.ToNative(),
                R = particle.R,
                M = particle.M
            };
        }

        public static NativeSurface ToNative(this Surface surface)
        {
            return new()
            {
                InBoundsNormal = surface.InBoundsNormal.ToNative(),
                LineEquation = surface.LineEquation.ToNative()
            };
        }

        public static NativeVector ToNative(this Vector vector)
        {
            return new()
            {
                Vx = vector.Vx,
                Vy = vector.Vy,
                Length = vector.Length
            };
        }

        public static NativeStraightLine ToNative(this StraightLine line)
        {
            return new()
            {
                NotLine = line.Type == StraightLineType.NotLine,
				ParallelVector = line.ParallelVector.ToNative(),
				A = line.A,
                B = line.B,
                C = line.C,
            };
        }        
        
        public static Vector FromNative(this NativeVector vector)
        {
            return new(vector.Vx, vector.Vy);
        }
    }
}
