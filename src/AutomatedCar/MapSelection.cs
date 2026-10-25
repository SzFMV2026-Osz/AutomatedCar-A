using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar
{
    public static class MapSelection
    {
        public const string DefaultMap = "test_world";

        //Gets the maps that have a world file and an NPC route file.
        public static IReadOnlyList<string> KnownMaps { get; } = new[] { "test_world", "oval" };

        //Resolves the map name from the command line arguments (without the program path).
        //Command line arguments.
        //Returns The map name in lower case.
        public static string Resolve(IReadOnlyList<string> args)
        {
            if (args == null)
            {
                return DefaultMap;
            }

            for (int i = 0; i < args.Count; i++)
            {
                string arg = args[i] ?? string.Empty;
                string value = null;

                if (arg.StartsWith("--map=", StringComparison.OrdinalIgnoreCase))
                {
                    value = arg.Substring("--map=".Length);
                }
                else if (arg.Equals("--map", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Count)
                    {
                        throw new ArgumentException("Missing value after --map. Known maps: " + string.Join(", ", KnownMaps));
                    }

                    value = args[i + 1];
                }

                if (value != null)
                {
                    string map = value.Trim().ToLowerInvariant();
                    if (!KnownMaps.Contains(map))
                    {
                        throw new ArgumentException($"Unknown map '{value}'. Known maps: " + string.Join(", ", KnownMaps));
                    }

                    return map;
                }
            }

            return DefaultMap;
        }
    }
}
