using System;
using System.Collections.Generic;
using AutomatedCar.Npc;
using AutomatedCar.Npc.Routing;

namespace AutomatedCar.Npc.Routing
{
    public sealed record NpcRoute(
        string NpcId,
        NpcKind Kind,
        string Sprite,
        bool Loop,
        IReadOnlyList<Waypoint> Waypoints);
}
