using System.Runtime.InteropServices;

namespace GasSimulationV2.Core.Native.StructureWrappers
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeSimulationStateOut
    {
        public IntPtr Collisions;
        public IntPtr Particles;
    }
}
