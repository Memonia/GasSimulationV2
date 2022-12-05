using System.Runtime.InteropServices;

namespace GasSimulationV2.Core.Native.StructureWrappers
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeParticle
    {
        public NativeVector Velocity;
        public NativeVector Center;
        public double R;
        public double M;
    }
}
