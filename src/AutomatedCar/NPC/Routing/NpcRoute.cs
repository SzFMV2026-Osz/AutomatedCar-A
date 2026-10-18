using System;
using System.Collections.Generic;

namespace AutomatedCar.Npc.Routing
{
    public sealed record NpcRoute(
        string NpcId,
        NpcKind Kind,
        string Sprite,
        bool Loop,
        IReadOnlyList<Waypoint> Waypoints);
}
