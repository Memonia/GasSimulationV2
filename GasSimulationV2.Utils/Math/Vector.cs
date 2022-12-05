namespace GasSimulationV2.Utils.Math
{
    public readonly struct Vector
    {
        public double Vx { get; init; }
        public double Vy { get; init; }
        public double Length { get; private init; }

        public Vector(double vx, double vy)
        {
            Vx = vx;
            Vy = vy;
            Length = System.Math.Sqrt(vx * vx + vy * vy);
        }

        public Vector Normal()
        {
			// new(Vy/Vx, -1)
			// new(1, -Vx/Vy) 
			// new(1/-Vx, 1/Vy)
			return new(Vy, -Vx);
        }

        public Vector Normalised()
        {
            return new(Vx / Length, Vy / Length);
        }

        public Vector Reflected(Vector n)
        {
            // Just the formula of reflected ray of light
            n = n.Normalised();
            return this - n * (this * n) * 2;
        }

        static public Vector operator +(Vector v1, Vector v2) =>
            new(v1.Vx + v2.Vx, v1.Vy + v2.Vy);

        static public Vector operator -(Vector v1, Vector v2) =>
            new(v1.Vx - v2.Vx, v1.Vy - v2.Vy);

        static public Vector operator -(Vector v) =>
            new(-v.Vx, -v.Vy);

        static public Vector operator *(Vector v, int n) =>
            new(v.Vx * n, v.Vy * n);

        static public Vector operator /(Vector v, int n) =>
            new(v.Vx / n, v.Vy / n);

        static public Vector operator *(Vector v, double n) =>
            new(v.Vx * n, v.Vy * n);

        static public Vector operator /(Vector v, double n) =>
            new(v.Vx / n, v.Vy / n);

        // Unusual operations, but needed for convinience
        static public Vector operator -(Vector v, double n) =>
            new(v.Vx - n, v.Vy - n);

        static public Vector operator +(Vector v, double n) =>
            new(v.Vx + n, v.Vy + n);

        // Dot product
        static public double operator *(Vector v1, Vector v2) =>
            v1.Vx * v2.Vx + v1.Vy * v2.Vy;

        public override string ToString() => $"Vector{{{Vx:0.000}; {Vy:0.000}}}";
    }
}
