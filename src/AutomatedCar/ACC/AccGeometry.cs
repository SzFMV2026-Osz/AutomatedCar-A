namespace AutomatedCar.ACC
{
    using System;

    public static class AccGeometry
    {
        // 0 deg = up, clockwise, y grows downwards
        public static CarFramePoint ToCarFrame(EgoPose ego, Vec2 worldPoint)
        {
            double heading = ego.HeadingDeg * Math.PI / 180.0;
            var forward = new Vec2(Math.Sin(heading), -Math.Cos(heading));
            var right = new Vec2(Math.Cos(heading), Math.Sin(heading));
            var d = worldPoint - new Vec2(ego.X, ego.Y);
            return new CarFramePoint(d.Dot(forward), d.Dot(right));
        }

        // same formula as the drawing: world = position + R(rotation) * (local - rotationPoint)
        public static Vec2 ToWorld(Vec2 local, Vec2 rotationPoint, Vec2 position, double rotationDeg)
        {
            double angle = rotationDeg * Math.PI / 180.0;
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            var p = local - rotationPoint;
            return new Vec2(
                position.X + (p.X * cos) - (p.Y * sin),
                position.Y + (p.X * sin) + (p.Y * cos));
        }
    }
}
