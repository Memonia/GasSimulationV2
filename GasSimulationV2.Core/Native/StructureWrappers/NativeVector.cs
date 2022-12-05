using System.Runtime.InteropServices;

namespace GasSimulationV2.Core.Native.StructureWrappers
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeVector
    {
        public double Vx;
        public double Vy;
        public double Length;
    }
}
