namespace AutomatedCar.NPC
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using AutomatedCar.Models;
    using AutomatedCar.Npc.Routing;
    using AutomatedCar.NPC.Routing;
    using Avalonia.Media;

    public class NpcPedestrian : WorldObject, INpc
    {
        private readonly NpcRoute route;
        private readonly RouteFollower follower;

        public NpcPedestrian(
            NpcRoute route,
            IEnumerable<PolylineGeometry> polygons,
            Point rotationPoint,
            MotionLimits limits = null)
            : base(
                StartX(route),
                StartY(route),
                SpriteOf(route),
                zindex: 10,
                collideable: true,
                worldObjectType: WorldObjectType.Pedestrian)
        {
            this.route = route;
            this.follower = new RouteFollower(route, limits ?? MotionLimits.ForPedestrian);
            this.RotationPoint = rotationPoint;

            if (polygons != null)
            {
                foreach (PolylineGeometry polygon in polygons)
                {
                    if (polygon == null)
                    {
                        continue;
                    }

                    this.RawGeometries.Add(polygon);
                    this.Geometries.Add(polygon);
                }
            }

            this.SyncFromFollower();
        }

        public string Id => this.route.NpcId;

        public NpcKind Kind => NpcKind.Pedestrian;

        public NpcState State => this.follower.State;

        public double SpeedMps => Units.PxPerSecToMps(this.follower.SpeedPxPerSec);

        public double HeadingDeg => this.follower.HeadingDeg;

        public RouteFollower Follower => this.follower;

        public void Update(double deltaSeconds)
        {
            this.follower.Step(deltaSeconds);
            this.SyncFromFollower();
        }

        public void Reset()
        {
            this.follower.Reset();
            this.SyncFromFollower();
        }

        private static int StartX(NpcRoute route) => RoundToWorld(ValidateRoute(route).Waypoints[0].X);

        private static int StartY(NpcRoute route) => RoundToWorld(ValidateRoute(route).Waypoints[0].Y);

        private static string SpriteOf(NpcRoute route) => ValidateRoute(route).Sprite;

        private static NpcRoute ValidateRoute(NpcRoute route)
        {
            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            if (route.Waypoints == null || route.Waypoints.Count == 0)
            {
                throw new ArgumentException($"Route '{route.NpcId}' has no waypoints.", nameof(route));
            }

            return route;
        }

        private static int RoundToWorld(double value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

        private void SyncFromFollower()
        {
            int x = RoundToWorld(this.follower.PosX);
            int y = RoundToWorld(this.follower.PosY);

            if (this.X != x)
            {
                this.X = x;
            }

            if (this.Y != y)
            {
                this.Y = y;
            }

            if (this.Rotation != this.follower.HeadingDeg)
            {
                this.Rotation = this.follower.HeadingDeg;
            }
        }
    }
}
