namespace AutomatedCar.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Avalonia.Media;
    using Newtonsoft.Json;

    public static class PolygonRepository
    {
        private const string PolygonResource = "AutomatedCar.Assets.worldobject_polygons.json";
        private const string ReferencePointResource = "AutomatedCar.Assets.reference_points.json";

        private static readonly Lazy<Dictionary<string, List<List<Avalonia.Point>>>> Polygons =
            new Lazy<Dictionary<string, List<List<Avalonia.Point>>>>(() => ParsePolygons(ReadResource(PolygonResource)));

        private static readonly Lazy<Dictionary<string, System.Drawing.Point>> ReferencePoints =
            new Lazy<Dictionary<string, System.Drawing.Point>>(() => ParseReferencePoints(ReadResource(ReferencePointResource)));

        public static List<List<Avalonia.Point>> GetRawPoints(string type)
        {
            if (Polygons.Value.TryGetValue(NormalizeType(type), out var polygons))
            {
                return polygons.Select(points => new List<Avalonia.Point>(points)).ToList();
            }

            return new List<List<Avalonia.Point>>();
        }

        public static List<PolylineGeometry> GetPolygons(string type)
        {
            return GetRawPoints(type).Select(points => new PolylineGeometry(points, false)).ToList();
        }

        public static bool TryGetReferencePoint(string type, out System.Drawing.Point point)
        {
            return ReferencePoints.Value.TryGetValue(NormalizeType(type), out point);
        }

        public static Dictionary<string, List<List<Avalonia.Point>>> ParsePolygons(string json)
        {
            var objects = JsonConvert.DeserializeObject<Dictionary<string, List<RawWorldObjectPolygon>>>(json)["objects"];
            var result = new Dictionary<string, List<List<Avalonia.Point>>>();
            foreach (RawWorldObjectPolygon rwop in objects)
            {
                result[rwop.Type] = rwop.Polys
                    .Select(poly => poly.Points.Select(p => new Avalonia.Point(p[0], p[1])).ToList())
                    .ToList();
            }

            return result;
        }

        public static Dictionary<string, System.Drawing.Point> ParseReferencePoints(string json)
        {
            return JsonConvert.DeserializeObject<List<RotationPoint>>(json)
                .ToDictionary(rp => rp.Type, rp => new System.Drawing.Point(rp.X, rp.Y));
        }

        private static string NormalizeType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return string.Empty;
            }

            return type.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? type[..^4] : type;
        }

        private static string ReadResource(string name)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
                ?? throw new FileNotFoundException($"Embedded resource not found: {name}", name);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}