using static System.Math;

namespace GasSimulationV2.Utils.Math
{
    public static class MathUtils
    {
        public static int Sign(int n) => n >= 0 ? 1 : -1;
        public static int Sign(double n) => n >= 0 ? 1 : -1;
        public static int IsZero(int n) => n == 0 ? 0 : 1;
        public static int IsZero(double n) => n == 0 ? 0 : 1;

        public static double Distance(Vector p1, Vector p2)
        {
            return Sqrt(
                (p1.Vx - p2.Vx) * (p1.Vx - p2.Vx) + (p1.Vy - p2.Vy) * (p1.Vy - p2.Vy)
            );
        }
    }
}
