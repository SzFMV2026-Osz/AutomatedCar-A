using AutomatedCar.Helpers;
using AutomatedCar.NPC.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar.NPC
{

    //Creates every NPC defined for the map.
    //The NPCs; each one is also a WorldObject.
    public static class NpcFactory
    {
        public static IReadOnlyList<INpc> CreateAll(string mapName, INpcRouteRepository repository)
        {
            ArgumentNullException.ThrowIfNull(repository);
            return repository.Load(mapName).Select(Create).ToList();
        }

        // <summary>Creates one NPC from its route.</summary>
        //The route.
        // returns npcCar or npcPedestrian
        public static INpc Create(NpcRoute route)
        {
            ArgumentNullException.ThrowIfNull(route);
            var polygons = PolygonRepository.GetPolygons(route.Sprite);
            PolygonRepository.TryGetReferencePoint(route.Sprite, out var rotationPoint);

            return route.Kind switch
            {
                NpcKind.Car => new NpcCar(route, polygons, rotationPoint, MotionLimits.ForCar),
                NpcKind.Pedestrian => new NpcPedestrian(route, polygons, rotationPoint, MotionLimits.ForPedestrian),
                _ => throw new ArgumentOutOfRangeException(nameof(route), route.Kind, "Unknown NPC kind."),
            };
        }
    }
}
