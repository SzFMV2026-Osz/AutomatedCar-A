using System;
using System.Collections.Generic;
using AutomatedCar.NPC;

namespace AutomatedCar.NPC.Routing
{
    public sealed record NpcRoute(
        string NpcId,
        NpcKind Kind,
        string Sprite,
        bool Loop,
        IReadOnlyList<Waypoint> Waypoints);
}
