using AutomatedCar.Models;
using AutomatedCar.NPC.Routing;
using AutomatedCar.NPC;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static AutomatedCarTests.Npc.RouteTestHelper;

namespace Tests.NPC
{
    public class NpcCarTests
    {
        private static NpcCar CreateCar(params Waypoint[] waypoints) =>
            new NpcCar(Route(true, waypoints), Array.Empty<PolylineGeometry>(), new System.Drawing.Point(60, 145), Instant);

        [Fact]
        public void Constructor_IsCollideableCar()
        {
            var car = CreateCar(new Waypoint(100, 200, 50), new Waypoint(100, 0, 50));

            Assert.Equal(WorldObjectType.Car, car.WorldObjectType);
            Assert.True(car.Collideable);
            Assert.Equal(NpcKind.Car, car.Kind);
            Assert.Equal("test", car.Id);
        }

        [Fact]
        public void Update_SpeedIsInPxPerSec()
        {
            // 50 km/h = 13.89 m/s = 694 px/s
            var car = CreateCar(new Waypoint(0, 1000, 50), new Waypoint(0, 0, 50));

            car.Update(1.0);

            Assert.Equal(694, car.Speed);
        }

        [Fact]
        public void SensorsSeeItAsCar()
        {
            WorldObject car = CreateCar(new Waypoint(0, 0, 50), new Waypoint(0, -100, 50));

            Assert.IsAssignableFrom<Car>(car);
        }
    }
}
