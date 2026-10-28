namespace AutomatedCarTests.Npc
{
    using AutomatedCar.Models;
    using AutomatedCar.Npc.Routing;
    using AutomatedCar.NPC;
    using AutomatedCar.NPC.Routing;
    using Avalonia.Media;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Xunit;

    public class NpcPedestrianTests
    {
        private const double Dt = 1.0 / 60;

        [Fact]
        public void Constructor_NullRoute_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new NpcPedestrian(null, null, new Point(0, 0)));
        }

        [Fact]
        public void Constructor_RouteWithoutWaypoints_ThrowsArgumentExceptionWithRouteId()
        {
            var route = new NpcRoute("empty_ped", NpcKind.Pedestrian, "man.png", true, Array.Empty<Waypoint>());

            var exception = Assert.Throws<ArgumentException>(() => new NpcPedestrian(route, null, new Point(0, 0)));

            Assert.Contains("empty_ped", exception.Message);
        }

        [Fact]
        public void Constructor_StartsAtFirstWaypointRoundedAwayFromZero()
        {
            var pedestrian = Create(PedestrianRoute(new Waypoint(10.5, -20.5, 5), new Waypoint(100, -20.5, 5)));

            Assert.Equal(11, pedestrian.X);
            Assert.Equal(-21, pedestrian.Y);
        }

        [Fact]
        public void Constructor_SetsWorldObjectProperties()
        {
            var rotationPoint = new Point(19, 36);
            var route = PedestrianRoute(new Waypoint(100, 200, 5), new Waypoint(100, 100, 5));

            var pedestrian = new NpcPedestrian(route, null, rotationPoint);

            Assert.Equal("man.png", pedestrian.Filename);
            Assert.Equal(10, pedestrian.ZIndex);
            Assert.True(pedestrian.Collideable);
            Assert.Equal(WorldObjectType.Pedestrian, pedestrian.WorldObjectType);
            Assert.Equal(rotationPoint, pedestrian.RotationPoint);
        }

        [Fact]
        public void Constructor_CopiesPolygonsToRawAndTransformedGeometries()
        {
            var polygon = CreatePolygon();
            var pedestrian = Create(PedestrianRoute(new Waypoint(0, 0, 5), new Waypoint(100, 0, 5)), polygon);

            Assert.Single(pedestrian.RawGeometries);
            Assert.Single(pedestrian.Geometries);
            Assert.Same(polygon, pedestrian.RawGeometries[0]);
            Assert.Same(polygon, pedestrian.Geometries[0]);
        }

        [Fact]
        public void Constructor_NullPolygons_DoesNotThrow()
        {
            var route = PedestrianRoute(new Waypoint(0, 0, 5), new Waypoint(100, 0, 5));

            var pedestrian = new NpcPedestrian(route, null, new Point(19, 36));

            Assert.Empty(pedestrian.RawGeometries);
            Assert.Empty(pedestrian.Geometries);
        }

        [Fact]
        public void Constructor_ExposesRouteIdentityAsPedestrian()
        {
            var route = PedestrianRoute(new Waypoint(0, 0, 5), new Waypoint(100, 0, 5));

            var pedestrian = Create(route);

            Assert.Equal(route.NpcId, pedestrian.Id);
            Assert.Equal(NpcKind.Pedestrian, pedestrian.Kind);
        }

        [Fact]
        public void Constructor_WithoutLimits_UsesPedestrianAcceleration()
        {
            var pedestrian = Create(PedestrianRoute(new Waypoint(0, 0, 50), new Waypoint(0, -1000, 50)));

            Run(pedestrian, 1);

            Assert.InRange(pedestrian.SpeedMps, 0.98, 1.02);
            Assert.IsType<RouteFollower>(pedestrian.Follower);
        }

        [Fact]
        public void Update_CopiesFollowerPositionAndHeading()
        {
            var pedestrian = Create(PedestrianRoute(new Waypoint(0, 0, 5), new Waypoint(0, -500, 5)));

            Run(pedestrian, 3);

            Assert.True(pedestrian.Y < 0);
            Assert.Equal(Round(pedestrian.Follower.PosX), pedestrian.X);
            Assert.Equal(Round(pedestrian.Follower.PosY), pedestrian.Y);
            Assert.Equal(pedestrian.Follower.HeadingDeg, pedestrian.Rotation, 9);
            Assert.Equal(pedestrian.Follower.State, pedestrian.State);
            Assert.Equal(pedestrian.Follower.HeadingDeg, pedestrian.HeadingDeg);
        }

        [Fact]
        public void Update_WhileWaiting_DoesNotRaiseRedundantPositionEvents()
        {
            var pedestrian = Create(PedestrianRoute(
                new Waypoint(0, 0, 5, WaitSeconds: 1),
                new Waypoint(0, -100, 5),
                new Waypoint(0, 0, 5)));
            var changedProperties = new List<string>();
            pedestrian.PropertyChangedEvent += (_, e) => changedProperties.Add(e.PropertyName);

            pedestrian.Update(Dt);

            Assert.Equal(NpcState.Waiting, pedestrian.State);
            Assert.DoesNotContain(nameof(WorldObject.X), changedProperties);
            Assert.DoesNotContain(nameof(WorldObject.Y), changedProperties);
        }

        [Fact]
        public void Update_AtCrosswalkEnd_TurnsAroundWhileWaiting()
        {
            var pedestrian = Create(PedestrianRoute(
                new Waypoint(0, 0, 5),
                new Waypoint(0, -20, 5, WaitSeconds: 2),
                new Waypoint(0, 0, 5)));

            while (pedestrian.Follower.TargetIndex == 1)
            {
                pedestrian.Update(Dt);
            }

            Run(pedestrian, 1.5);

            Assert.Equal(NpcState.Waiting, pedestrian.State);
            Assert.Equal(-20, pedestrian.Y);
            Assert.Equal(180, pedestrian.Rotation, 3);
        }

        [Fact]
        public void Reset_RestoresStartPositionAndHeading()
        {
            var pedestrian = Create(PedestrianRoute(new Waypoint(0, 0, 5), new Waypoint(200, 0, 5)));

            Run(pedestrian, 2);
            Assert.True(pedestrian.X > 0);

            pedestrian.Reset();

            Assert.Equal(0, pedestrian.X);
            Assert.Equal(0, pedestrian.Y);
            Assert.Equal(90, pedestrian.Rotation, 9);
            Assert.Equal(0, pedestrian.SpeedMps);
            Assert.Equal(NpcState.Idle, pedestrian.State);
        }

        [Fact]
        public void Update_SlowPedestrian_KeepsFractionalMovementInFollower()
        {
            var pedestrian = Create(PedestrianRoute(new Waypoint(100.4, 500.6, 5), new Waypoint(100.4, -500.6, 5)));

            Run(pedestrian, 10);

            Assert.Equal(100, pedestrian.X);
            Assert.True(pedestrian.Y < 500);
            Assert.Equal(Round(pedestrian.Follower.PosY), pedestrian.Y);
            Assert.InRange(Math.Abs(pedestrian.Follower.PosY - pedestrian.Y), 0, 0.5);
        }

        private static NpcRoute PedestrianRoute(params Waypoint[] waypoints) =>
            new NpcRoute("ped_test", NpcKind.Pedestrian, "man.png", true, waypoints);

        private static NpcPedestrian Create(NpcRoute route, params PolylineGeometry[] polygons) =>
            new NpcPedestrian(route, polygons, new Point(19, 36));

        private static PolylineGeometry CreatePolygon() =>
            new PolylineGeometry(
                new List<Avalonia.Point>
                {
                    new Avalonia.Point(0, 0),
                    new Avalonia.Point(10, 0),
                    new Avalonia.Point(10, 10),
                },
                false);

        private static int Round(double value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

        private static void Run(NpcPedestrian pedestrian, double seconds)
        {
            int ticks = (int)Math.Round(seconds / Dt);
            for (int i = 0; i < ticks; i++)
            {
                pedestrian.Update(Dt);
            }
        }
    }
}
