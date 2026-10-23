namespace AutomatedCar.Npc.Routing
{
    using AutomatedCar.NPC;
    using AutomatedCar.NPC.Routing;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// Loads NPC routes from the embedded <c>Assets/NpcRoutes/{mapName}.json</c> resources.
    public sealed class JsonNpcRouteRepository : INpcRouteRepository
    {
        /// <summary>Manifest resource name prefix of the route files.</summary>
        public const string ResourcePrefix = "AutomatedCar.Assets.NpcRoutes.";

        private readonly Assembly assembly;

        /// <summary>Initializes a new instance of the <see cref="JsonNpcRouteRepository"/> class using the application assembly.</summary>
        public JsonNpcRouteRepository()
            : this(typeof(JsonNpcRouteRepository).Assembly)
        {
        }

        /// Initializes a new instance of the "JsonNpcRouteRepository"
        /// Assembly that contains the route resources.
        public JsonNpcRouteRepository(Assembly assembly)
        {
            this.assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        /// <inheritdoc/>
        public IReadOnlyList<NpcRoute> Load(string mapName)
        {
            if (string.IsNullOrWhiteSpace(mapName))
            {
                throw new ArgumentException("Map name must not be empty.", nameof(mapName));
            }

            string resourceName = ResourcePrefix + mapName + ".json";
            using Stream stream = this.assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"NPC route file not found: {resourceName}", resourceName);
            using var reader = new StreamReader(stream);
            return Parse(reader.ReadToEnd());
        }

        /// Parses the content of a route JSON file.
        /// returns the routes
        public static IReadOnlyList<NpcRoute> Parse(string json)
        {
            RawRouteFile file;
            try
            {
                file = JsonConvert.DeserializeObject<RawRouteFile>(json ?? string.Empty);
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("Invalid NPC route JSON: " + ex.Message, ex);
            }

            if (file?.Npcs == null)
            {
                throw new InvalidDataException("NPC route file has no 'npcs' array.");
            }

            return file.Npcs.Select(ToRoute).ToList();
        }

        private static NpcRoute ToRoute(RawNpc raw)
        {
            if (raw == null || string.IsNullOrWhiteSpace(raw.Id))
            {
                throw new InvalidDataException("Every NPC needs an 'id'.");
            }

            if (!Enum.TryParse(raw.Kind, ignoreCase: true, out NpcKind kind) || !Enum.IsDefined(kind))
            {
                throw new InvalidDataException($"NPC '{raw.Id}': unknown kind '{raw.Kind}'.");
            }

            if (string.IsNullOrWhiteSpace(raw.Sprite))
            {
                throw new InvalidDataException($"NPC '{raw.Id}': missing 'sprite'.");
            }

            if (raw.Waypoints == null || raw.Waypoints.Count < 2)
            {
                throw new InvalidDataException($"NPC '{raw.Id}': at least 2 waypoints are required.");
            }

            var waypoints = raw.Waypoints.Select((w, i) => ToWaypoint(raw.Id, i, w)).ToList();
            return new NpcRoute(raw.Id, kind, raw.Sprite, raw.Loop ?? true, waypoints);
        }

        private static Waypoint ToWaypoint(string npcId, int index, RawWaypoint raw)
        {
            if (raw == null || raw.X == null || raw.Y == null || raw.Speed == null)
            {
                throw new InvalidDataException($"NPC '{npcId}', waypoint #{index}: 'x', 'y' and 'speed' are required.");
            }

            if (raw.Speed < 0 || (raw.Accel != null && raw.Accel <= 0) || (raw.Wait != null && raw.Wait < 0))
            {
                throw new InvalidDataException($"NPC '{npcId}', waypoint #{index}: speed and wait must be >= 0, accel must be > 0.");
            }

            return new Waypoint(raw.X.Value, raw.Y.Value, raw.Speed.Value, raw.Accel, raw.Wait ?? 0);
        }

        internal sealed class RawRouteFile
        {
            [JsonProperty("map")]
            public string Map { get; set; }

            [JsonProperty("npcs")]
            public List<RawNpc> Npcs { get; set; }
        }

        internal sealed class RawNpc
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("kind")]
            public string Kind { get; set; }

            [JsonProperty("sprite")]
            public string Sprite { get; set; }

            [JsonProperty("loop")]
            public bool? Loop { get; set; }

            [JsonProperty("waypoints")]
            public List<RawWaypoint> Waypoints { get; set; }
        }

        internal sealed class RawWaypoint
        {
            [JsonProperty("x")]
            public double? X { get; set; }

            [JsonProperty("y")]
            public double? Y { get; set; }

            [JsonProperty("speed")]
            public double? Speed { get; set; }

            [JsonProperty("accel")]
            public double? Accel { get; set; }

            [JsonProperty("wait")]
            public double? Wait { get; set; }
        }
    }
}