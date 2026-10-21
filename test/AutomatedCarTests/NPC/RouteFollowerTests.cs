using System.IO;
using AutomatedCar.NPC;
using AutomatedCar.NPC.Routing;
using Xunit;

namespace AutomatedCarTests.NPC
{
    public class RouteFollowerTests
    {
        private static RouteFollower Follower(params Waypoint[] waypoints) =>
            new RouteFollower(new NpcRoute("test", NpcKind.Car, "car", false, waypoints));

        [Fact]
        public void Step_ReachesLastWaypoint_Finishes()
        {
            var follower = Follower(new Waypoint(0, 0, 36), new Waypoint(0, -100, 36));

            for (int i = 0; i < 300; i++)
            {
                follower.Step(1.0 / 60);
            }

            Assert.Equal(NpcState.Finished, follower.State);
        }

        [Fact]
        public void Step_WhileWaiting_TurnsTheShorterWay()
        {
            var follower = Follower(new Waypoint(0, 0, 36), new Waypoint(0, -100, 36, null, 10), new Waypoint(-100, -100, 36));

            for (int i = 0; i < 300 && follower.State != NpcState.Waiting; i++)
            {
                follower.Step(1.0 / 60);
            }

            follower.Step(0.1);

            Assert.True(follower.HeadingDeg > 270);
        }

        [Fact]
        public void Load_MissingMap_Throws()
        {
            Assert.Throws<FileNotFoundException>(() => new JsonNpcRouteRepository().Load("no_such_map"));
        }
    }
}