namespace AutomatedCarTests.Npc
{
    using AutomatedCar.Npc;
    using AutomatedCar.Npc.Routing;
    using AutomatedCar.NPC;
    using AutomatedCar.NPC.Routing;
    using System;

    /// <summary>Helpers for building routes and running followers in tests.</summary>
    public static class RouteTestHelper
    {
        public const double Dt = 1.0 / 60;

        /// <summary>Gets limits that reach any target speed in one tick (to test geometry without acceleration).</summary>
        public static MotionLimits Instant { get; } = new MotionLimits(DefaultAccelMps2: 1000);

        public static NpcRoute Route(bool loop, params Waypoint[] waypoints) =>
            new NpcRoute("test", NpcKind.Car, "car_3_black.png", loop, waypoints);

        public static NpcRoute PedestrianRoute(bool loop, params Waypoint[] waypoints) =>
            new NpcRoute("test_ped", NpcKind.Pedestrian, "man.png", loop, waypoints);

        public static void Run(RouteFollower follower, double seconds)
        {
            int ticks = (int)Math.Round(seconds / Dt);
            for (int i = 0; i < ticks; i++)
            {
                follower.Step(Dt);
            }
        }

        /// <summary>Signed heading change in (-180, 180].</summary>
        public static double SignedDelta(double from, double to) => RouteFollower.NormalizeDeg(to - from + 180) - 180;
    }
}