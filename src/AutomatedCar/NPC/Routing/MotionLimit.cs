using System;
using AutomatedCar.Npc;
using AutomatedCar.Npc.Routing;

namespace AutomatedCar.Npc.Routing
{
    public sealed record MotionLimits(
        double DefaultAccelMps2 = 2.5,
        double MaxTurnRateDegPerSec = 90,
        double ArrivalTolerancePx = 5)
    {
        public static MotionLimits ForCar { get; } = new MotionLimits();

        public static MotionLimits ForPedestrian { get; } = new MotionLimits(DefaultAccelMps2: 1.0, MaxTurnRateDegPerSec: 180, ArrivalTolerancePx: 2);

        public static MotionLimits For(NpcKind kind) => kind == NpcKind.Pedestrian ? ForPedestrian : ForCar;
    }
}