using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar.ACC
{
    public readonly record struct Vec2(double X, double Y)
    {
        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);

        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);

        public double Dot(Vec2 other) => (this.X * other.X) + (this.Y * other.Y);
    }
}
