using System.Runtime.InteropServices;

namespace GasSimulationV2.Core.Native.StructureWrappers
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeSurface
    {
        public NativeVector InBoundsNormal;
        public NativeStraightLine LineEquation;
    }
}
