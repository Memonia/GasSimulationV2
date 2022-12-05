using System.Runtime.InteropServices;

namespace GasSimulationV2.Core.Native.StructureWrappers
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeStraightLine
    {
        public bool NotLine;
        public double A;
        public double B;
        public double C;
        public NativeVector ParallelVector;
    }
}
