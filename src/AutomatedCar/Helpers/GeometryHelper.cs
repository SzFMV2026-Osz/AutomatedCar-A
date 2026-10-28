namespace AutomatedCar.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutomatedCar.Models;
    using Avalonia;

    public static class GeometryHelper
    {
        public static List<List<Point>> GetWorldPolygons(WorldObject worldObject)
        {
            double rotation = worldObject.Rotation * Math.PI / 180.0;
            double sin = Math.Sin(rotation);
            double cos = Math.Cos(rotation);

            return worldObject.RawGeometries
                .Select(geometry => geometry.Points
                    .Select(p => new Point(
                        worldObject.X + ((p.X - worldObject.RotationPoint.X) * cos) - ((p.Y - worldObject.RotationPoint.Y) * sin),
                        worldObject.Y + ((p.X - worldObject.RotationPoint.X) * sin) + ((p.Y - worldObject.RotationPoint.Y) * cos)))
                    .ToList())
                .ToList();
        }

        public static bool IsInTriangle(WorldObject worldObject, Point a, Point b, Point c)
        {
            var triangle = new List<Point> { a, b, c, a };
            var polygons = GetWorldPolygons(worldObject);

            if (polygons.Count == 0)
            {
                return IsPointInPolygon(new Point(worldObject.X, worldObject.Y), triangle);
            }

            return polygons.Any(polygon => Touches(polygon, triangle));
        }

         public static bool Touches(WorldObject first, WorldObject second)
        {
            var secondPolygons = GetWorldPolygons(second);
            return GetWorldPolygons(first).Any(a => secondPolygons.Any(b => Touches(a, b)));
        }

        public static bool Touches(List<Point> a, List<Point> b)
        {
            for (int i = 0; i < a.Count - 1; i++)
            {
                for (int j = 0; j < b.Count - 1; j++)
                {
                    if (SegmentsIntersect(a[i], a[i + 1], b[j], b[j + 1]))
                    {
                        return true;
                    }
                }
            }

            return a.Count > 0 && b.Count > 0 && (IsPointInPolygon(a[0], b) || IsPointInPolygon(b[0], a));
        }

        public static bool IsPointInPolygon(Point point, List<Point> polygon)
        {
            if (polygon.Count < 3)
            {
                return false;
            }

            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if ((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y) &&
                    point.X <= ((polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y)) + polygon[i].X)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private static bool SegmentsIntersect(Point p1, Point p2, Point q1, Point q2)
        {
            bool boundsOverlap = Math.Min(p1.X, p2.X) <= Math.Max(q1.X, q2.X) && Math.Min(q1.X, q2.X) <= Math.Max(p1.X, p2.X) &&
                Math.Min(p1.Y, p2.Y) <= Math.Max(q1.Y, q2.Y) && Math.Min(q1.Y, q2.Y) <= Math.Max(p1.Y, p2.Y);

            return boundsOverlap &&
                Cross(q1, q2, p1) * Cross(q1, q2, p2) <= 0 &&
                Cross(p1, p2, q1) * Cross(p1, p2, q2) <= 0;
        }

        private static double Cross(Point a, Point b, Point c)
        {
            return ((b.X - a.X) * (c.Y - a.Y)) - ((b.Y - a.Y) * (c.X - a.X));
        }
    }
}