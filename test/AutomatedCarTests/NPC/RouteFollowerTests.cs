namespace AutomatedCarTests.Npc
{
    using AutomatedCar.Npc;
    using AutomatedCar.Npc.Routing;
    using AutomatedCar.NPC;
    using AutomatedCar.NPC.Routing;
    using System;
    using System.Linq;
    using Xunit;
    using static AutomatedCarTests.Npc.RouteTestHelper;

    public class RouteFollowerTests
    {
        // ---------- start state ----------
        [Fact]
        public void Constructor_StartsAtFirstWaypoint()
        {
            var follower = new RouteFollower(Route(false, new Waypoint(100, 200, 50), new Waypoint(100, 0, 50)));

            Assert.Equal(100, follower.PosX);
            Assert.Equal(200, follower.PosY);
            Assert.Equal(0, follower.SpeedPxPerSec);
            Assert.Equal(0, follower.HeadingDeg, 6);
            Assert.Equal(1, follower.TargetIndex);
            Assert.False(follower.IsFinished);
            Assert.Equal(NpcState.Idle, follower.State);
        }

        [Fact]
        public void Constructor_LessThanTwoWaypoints_Throws()
        {
            Assert.Throws<ArgumentException>(() => new RouteFollower(Route(false, new Waypoint(0, 0, 10))));
        }

        [Fact]
        public void Constructor_NullRoute_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new RouteFollower(null));
        }

        // ---------- position ----------
        [Fact]
        public void Step_MovesTowardNextWaypoint()
        {
            var follower = new RouteFollower(Route(false, new Waypoint(0, 1000, 50), new Waypoint(0, 0, 50)), Instant);

            Run(follower, 1);

            Assert.Equal(0, follower.PosX, 9);
            Assert.InRange(follower.PosY, 305, 306.5); // 1000 - 694.4
        }

        [Fact]
        public void Step_DiagonalSegment_StaysOnLine()
        {
            var follower = new RouteFollower(Route(false, new Waypoint(0, 0, 50), new Waypoint(1000, 1000, 50)), Instant);

            for (int i = 0; i < 60; i++)
            {
                follower.Step(Dt);
                Assert.True(Math.Abs(follower.PosX - follower.PosY) < 1e-6);
            }
        }

        [Fact]
        public void Step_DoesNotOvershootLastWaypoint()
        {
            var follower = new RouteFollower(Route(false, new Waypoint(0, 0, 50), new Waypoint(0, -100, 50)), Instant);

            Run(follower, 2);

            Assert.Equal(0, follower.PosX);
            Assert.Equal(-100, follower.PosY);
            Assert.True(follower.IsFinished);
            Assert.Equal(NpcState.Finished, follower.State);
            Assert.Equal(0, follower.SpeedPxPerSec);
        }

        [Fact]
        public void Step_DenseWaypoints_CarriesRemainderToNextSegment()
        {
            // a waypoint every 5 px, but the car moves 11.6 px per tick
            var waypoints = Enumerable.Range(0, 201).Select(i => new Waypoint(0, -5 * i, 50)).ToArray();
            var follower = new RouteFollower(Route(false, waypoints), Instant);

            Run(follower, 1);

            Assert.InRange(follower.PosY, -695.5, -693.5);
        }

        [Fact]
        public void Step_ZeroDeltaTime_NoChange()
        {
            var follower = new RouteFollower(Route(false, new Waypoint(0, 0, 50), new Waypoint(0, -100, 50)), Instant);

            follower.Step(0);

            Assert.Equal(0, follower.PosY);
            Assert.Equal(0, follower.SpeedPxPerSec);
        }

        // ---------- speed ----------
        [Fact]
        public void Step_Accelerates_WithDefaultAccel()
        {
            var follower = new RouteFollower(Route(false, new Waypoint(0, 10000, 50), new Waypoint(0, 0, 50)), new MotionLimits());

            Run(follower, 1);

            Assert.Equal(125, follower.SpeedPxPerSec, 6); // 2.5 m/s after 1 s
        }

        [Fact]
        public void Step_UsesWaypointAccel()
        {
            var follower = new RouteFollower(Route(false, new Waypoint(0, 10000, 50, AccelMps2: 5), new Waypoint(0, 0, 50)), new MotionLimits());

            Run(follower, 1);

            Assert.Equal(250, follower.SpeedPxPerSec, 6);
        }

        [Fact]
        public void Step_NeverExceedsTargetSpeed()
        {
            var follower = new RouteFollower(Route(false, new Waypoint(0, 100000, 50), new Waypoint(0, 0, 50)), new MotionLimits());

            for (int i = 0; i < 600; i++)
            {
                follower.Step(Dt);
                Assert.True(follower.SpeedPxPerSec <= Units.KmhToPxPerSec(50) + 1e-9);
            }
        }

        [Fact]
        public void Step_NewTargetSpeedAppliesAfterPassingWaypoint()
        {
            var follower = new RouteFollower(
                Route(false, new Waypoint(0, 2000, 50), new Waypoint(0, 1000, 20), new Waypoint(0, 0, 20)),
                Instant);

            while (follower.PosY > 1020)
            {
                follower.Step(Dt);
            }

            Assert.Equal(Units.KmhToPxPerSec(50), follower.SpeedPxPerSec, 6);

            Run(follower, 0.5);

            Assert.Equal(Units.KmhToPxPerSec(20), follower.SpeedPxPerSec, 6);
        }

        [Fact]
        public void Step_BrakingFrom50At9Mps2_StopsWithin536Px()
        {
            var follower = new RouteFollower(
                Route(false, new Waypoint(0, 5000, 50), new Waypoint(0, 3000, 0, AccelMps2: 9), new Waypoint(0, 0, 0)),
                new MotionLimits(DefaultAccelMps2: 100));

            while (follower.PosY > 3000)
            {
                follower.Step(Dt);
            }

            double brakeStartY = follower.PosY;
            Run(follower, 3);

            Assert.Equal(0, follower.SpeedPxPerSec, 6);
            Assert.InRange(brakeStartY - follower.PosY, 525, 547); // v²/2a = 10.7 m = 536 px
        }

        [Fact]
        public void Step_Braking_IsGradualAndReportsBraking()
        {
            var follower = new RouteFollower(
                Route(false, new Waypoint(0, 5000, 50), new Waypoint(0, 3000, 0, AccelMps2: 9), new Waypoint(0, 0, 0)),
                new MotionLimits(DefaultAccelMps2: 100));

            while (follower.PosY > 3000)
            {
                follower.Step(Dt);
            }

            double before = follower.SpeedPxPerSec;
            follower.Step(Dt);

            Assert.Equal(7.5, before - follower.SpeedPxPerSec, 6); // 450 px/s² * 1/60 s
            Assert.Equal(NpcState.Braking, follower.State);
        }

        // ---------- heading ----------
        [Theory]
        [InlineData(0, -1000, 0)]
        [InlineData(1000, 0, 90)]
        [InlineData(0, 1000, 180)]
        [InlineData(-1000, 0, 270)]
        public void Heading_PointsTowardsMovement(double dx, double dy, double expected)
        {
            var follower = new RouteFollower(Route(false, new Waypoint(0, 0, 50), new Waypoint(dx, dy, 50)), Instant);

            Assert.Equal(expected, follower.HeadingDeg, 6);
            follower.Step(Dt);
            Assert.Equal(expected, follower.HeadingDeg, 6);
        }

        [Fact]
        public void Heading_TurnRateIsLimited()
        {
            var follower = new RouteFollower(
                Route(false, new Waypoint(0, 0, 50), new Waypoint(0, -100, 50), new Waypoint(1000, -100, 50)),
                Instant); // 90°/s → max 1.5° per tick
            double previous = follower.HeadingDeg;

            for (int i = 0; i < 120; i++)
            {
                follower.Step(Dt);
                Assert.True(Math.Abs(SignedDelta(previous, follower.HeadingDeg)) <= 1.5 + 1e-9);
                previous = follower.HeadingDeg;
            }

            Assert.Equal(90, follower.HeadingDeg, 6);
        }

        [Fact]
        public void Heading_TakesShortestWay()
        {
            // first segment heads 350°, second heads 10°: the car must turn +20°, not -340°
            double x1 = 1000 * Math.Sin(350 * Math.PI / 180);
            double y1 = -1000 * Math.Cos(350 * Math.PI / 180);
            double x2 = x1 + (1000 * Math.Sin(10 * Math.PI / 180));
            double y2 = y1 - (1000 * Math.Cos(10 * Math.PI / 180));
            var follower = new RouteFollower(
                Route(false, new Waypoint(0, 0, 50), new Waypoint(x1, y1, 50), new Waypoint(x2, y2, 50)),
                Instant);
            Assert.Equal(350, follower.HeadingDeg, 6);

            while (follower.TargetIndex == 1)
            {
                follower.Step(Dt);
            }

            double previous = follower.HeadingDeg;
            for (int i = 0; i < 30; i++)
            {
                follower.Step(Dt);
                Assert.True(SignedDelta(previous, follower.HeadingDeg) >= -1e-9);
                previous = follower.HeadingDeg;
            }

            Assert.Equal(10, follower.HeadingDeg, 3);
        }

        [Fact]
        public void Heading_StaysIn0To360()
        {
            var follower = new RouteFollower(
                Route(true, new Waypoint(0, 0, 50), new Waypoint(400, 0, 50), new Waypoint(400, 400, 50), new Waypoint(0, 400, 50)),
                Instant);

            for (int i = 0; i < 60 * 20; i++)
            {
                follower.Step(Dt);
                Assert.InRange(follower.HeadingDeg, 0, 359.999999);
            }
        }

        // ---------- loop / finish / reset ----------
        [Fact]
        public void Loop_True_RestartsFromFirstWaypoint()
        {
            var follower = new RouteFollower(
                Route(true, new Waypoint(0, 0, 50), new Waypoint(400, 0, 50), new Waypoint(400, 400, 50), new Waypoint(0, 400, 50)),
                Instant);
            bool headedBackToStart = false;

            for (int i = 0; i < 60 * 3; i++)
            {
                follower.Step(Dt);
                headedBackToStart |= follower.TargetIndex == 0;
            }

            Assert.True(headedBackToStart);
            Assert.False(follower.IsFinished);
        }

        [Fact]
        public void Loop_False_StaysFinished()
        {
            var follower = new RouteFollower(Route(false, new Waypoint(0, 0, 50), new Waypoint(0, -100, 50)), Instant);
            Run(follower, 2);

            Run(follower, 1);

            Assert.Equal(-100, follower.PosY);
            Assert.True(follower.IsFinished);
        }

        [Fact]
        public void Reset_RestoresInitialState()
        {
            var route = Route(true, new Waypoint(0, 0, 50), new Waypoint(400, 0, 50), new Waypoint(400, 400, 50));
            var fresh = new RouteFollower(route, Instant);
            var used = new RouteFollower(route, Instant);
            Run(used, 2.5);

            used.Reset();

            Assert.Equal(fresh.PosX, used.PosX);
            Assert.Equal(fresh.PosY, used.PosY);
            Assert.Equal(fresh.HeadingDeg, used.HeadingDeg);
            Assert.Equal(fresh.SpeedPxPerSec, used.SpeedPxPerSec);
            Assert.Equal(fresh.TargetIndex, used.TargetIndex);
            Assert.Equal(fresh.State, used.State);
        }

        // ---------- waiting ----------
        [Fact]
        public void Wait_StandsStillForGivenSeconds_ThenContinues()
        {
            var follower = new RouteFollower(
                Route(false, new Waypoint(0, 0, 5), new Waypoint(0, -10, 5, WaitSeconds: 2), new Waypoint(0, -1000, 5)),
                Instant);

            while (follower.TargetIndex == 1)
            {
                follower.Step(Dt);
            }

            Assert.Equal(-10, follower.PosY);
            Assert.Equal(NpcState.Waiting, follower.State);

            Run(follower, 1.9);
            Assert.Equal(-10, follower.PosY);
            Assert.True(follower.IsWaiting);

            Run(follower, 0.5);
            Assert.True(follower.PosY < -10);
            Assert.Equal(NpcState.Moving, follower.State);
        }

        [Fact]
        public void Wait_Zero_NoPause()
        {
            var follower = new RouteFollower(
                Route(false, new Waypoint(0, 0, 50), new Waypoint(0, -10, 50), new Waypoint(0, -1000, 50)),
                Instant);

            for (int i = 0; i < 30; i++)
            {
                follower.Step(Dt);
                Assert.False(follower.IsWaiting);
            }
        }

        [Fact]
        public void Wait_TurnsTowardsNextWaypointWhileWaiting()
        {
            // pedestrian walks up, waits, then walks back down: it turns around while standing
            var follower = new RouteFollower(
                PedestrianRoute(false, new Waypoint(0, 0, 5), new Waypoint(0, -10, 5, WaitSeconds: 2), new Waypoint(0, 1000, 5)),
                MotionLimits.ForPedestrian);

            while (follower.TargetIndex == 1)
            {
                follower.Step(Dt);
            }

            Run(follower, 1.5);

            Assert.Equal(-10, follower.PosY);
            Assert.Equal(180, follower.HeadingDeg, 3);
        }
    }
}