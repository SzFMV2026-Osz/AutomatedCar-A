using System;
using AutomatedCar.Npc;
using AutomatedCar.Npc.Routing;
using Xunit;


namespace AutomatedCarTests.NPC
{


    public class RouteModelTests
    {
        [Fact]
        public void Waypoint_Defaults()
        {
            var waypoint = new Waypoint(1, 2, 30);

            Assert.Null(waypoint.AccelMps2);
            Assert.Equal(0, waypoint.WaitSeconds);
        }

        [Fact]
        public void Waypoint_ValueEquality()
        {
            Assert.Equal(new Waypoint(1, 2, 30, 9, 1), new Waypoint(1, 2, 30, 9, 1));
            Assert.NotEqual(new Waypoint(1, 2, 30), new Waypoint(1, 2, 40));
        }

        [Fact]
        public void MotionLimits_Defaults()
        {
            var limits = new MotionLimits();

            Assert.Equal(2.5, limits.DefaultAccelMps2);
            Assert.Equal(90, limits.MaxTurnRateDegPerSec);
            Assert.Equal(5, limits.ArrivalTolerancePx);
        }

        [Fact]
        public void MotionLimits_ForKind()
        {
            Assert.Same(MotionLimits.ForCar, MotionLimits.For(NpcKind.Car));
            Assert.Same(MotionLimits.ForPedestrian, MotionLimits.For(NpcKind.Pedestrian));
            Assert.True(MotionLimits.ForPedestrian.MaxTurnRateDegPerSec > MotionLimits.ForCar.MaxTurnRateDegPerSec);
        }
    }
}
