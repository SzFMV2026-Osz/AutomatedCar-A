using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace AutomatedCar.NPC.Routing
{

    /// Loads NPC routes from the embedded Assets/NpcRoutes/{mapName}.json resource.
    public sealed class JsonNpcRouteRepository : INpcRouteRepository
    {
        private const int MinimumWaypointCount = 2;

        private readonly Assembly assembly;

        public JsonNpcRouteRepository()
            : this(typeof(JsonNpcRouteRepository).Assembly)
        {
        }

        public JsonNpcRouteRepository(Assembly assembly)
        {
            this.assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        /// <summary>
        /// Loads the routes of a map.
        /// </summary>
        /// FileNotFoundException -> There is no embedded route file for the map.</exception>
        /// InvalidDataException ->The file is not valid JSON or the routes are invalid.</exception>
        public IReadOnlyList<NpcRoute> Load(string mapName)
        {
            if (string.IsNullOrWhiteSpace(mapName))
            {
                throw new ArgumentException("Map name must not be empty.", nameof(mapName));
            }

            string json = this.ReadResource(mapName);
            List<NpcRoute> routes = Parse(json, mapName);

            foreach (NpcRoute route in routes)
            {
                Validate(route, mapName);
            }

            return routes;
        }

        private static List<NpcRoute> Parse(string json, string mapName)
        {
            try
            {
                var serializer = JsonSerializer.CreateDefault();
                serializer.Converters.Add(new StringEnumConverter());

                JToken root = JToken.Parse(json);

                // Accepts both a plain array and an object like { "routes": [ ... ] }.
                JToken routesToken = root is JObject rootObject
                    ? rootObject.GetValue("routes", StringComparison.OrdinalIgnoreCase)
                    : root;

                if (!(routesToken is JArray))
                {
                    throw new InvalidDataException($"Route file of map '{mapName}' must contain a list of routes.");
                }

                return routesToken.ToObject<List<NpcRoute>>(serializer) ?? new List<NpcRoute>();
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException($"Route file of map '{mapName}' is not valid: {exception.Message}", exception);
            }
        }

        private static void Validate(NpcRoute route, string mapName)
        {
            if (route == null)
            {
                throw new InvalidDataException($"Route file of map '{mapName}' contains an empty route.");
            }

            if (string.IsNullOrWhiteSpace(route.NpcId))
            {
                throw new InvalidDataException($"Route file of map '{mapName}' contains a route without NpcId.");
            }

            if (route.Waypoints == null || route.Waypoints.Count < MinimumWaypointCount || route.Waypoints.Any(waypoint => waypoint == null))
            {
                throw new InvalidDataException($"Route '{route.NpcId}' of map '{mapName}' needs at least {MinimumWaypointCount} waypoints.");
            }
        }

        private string ReadResource(string mapName)
        {
            string suffix = ".NpcRoutes." + mapName + ".json";
            string resourceName = this.assembly
                .GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
            {
                throw new FileNotFoundException($"No embedded NPC route file found for map '{mapName}'.", mapName + ".json");
            }

            using (Stream stream = this.assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}