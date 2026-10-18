using System;

namespace AutomatedCar.Npc.Routing
{
    public sealed record Waypoint(
        double X,
        double Y,
        double SpeedKmh,
        double? AccelMps2 = null,
        double WaitSeconds = 0);
}