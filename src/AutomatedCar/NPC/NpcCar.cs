using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomatedCar.Models;
using AutomatedCar.Npc.Routing;
using AutomatedCar.NPC.Routing;
using Avalonia.Media;

namespace AutomatedCar.NPC
{
    public class NpcCar: Car, INpc
    {
        private readonly RouteFollower follower;

        //Initializes a new instance of the NpcCar
        //The route; its sprite is used as the image.
        //Polygons in image coordinates (see worldobject_polygons.json).
        //Rotation point in image coordinates (see reference_points.json).
        //Movement limits; null means the car defaults
        public NpcCar(NpcRoute route, IEnumerable<PolylineGeometry> polygons, System.Drawing.Point rotationPoint, MotionLimits limits = null)
            : base(StartX(route), StartY(route), route.Sprite)
        {
            this.follower = new RouteFollower(route, limits ?? MotionLimits.ForCar);
            this.Id = route.NpcId;
            this.Collideable = true;
            this.WorldObjectType = WorldObjectType.Car;
            this.ZIndex = 10;
            this.RotationPoint = rotationPoint;

            foreach (var polygon in polygons ?? Array.Empty<PolylineGeometry>())
            {
                this.RawGeometries.Add(polygon);
                this.Geometries.Add(polygon);
            }

            this.SyncFromFollower();
        }

        
        public string Id { get; }

        
        public NpcKind Kind => NpcKind.Car;

        
        public NpcState State => this.follower.State;

        
        public double SpeedMps => Units.PxPerSecToMps(this.follower.SpeedPxPerSec);

        
        public double HeadingDeg => this.follower.HeadingDeg;

        //Gets the route follower (for debugging and tests).
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

        private static int StartX(NpcRoute route) => RoundToInt(FirstWaypoint(route).X);

        private static int StartY(NpcRoute route) => RoundToInt(FirstWaypoint(route).Y);

        private static Waypoint FirstWaypoint(NpcRoute route)
        {
            ArgumentNullException.ThrowIfNull(route);
            if (route.Waypoints == null || route.Waypoints.Count == 0)
            {
                throw new ArgumentException($"Route '{route.NpcId}' has no waypoints.", nameof(route));
            }

            return route.Waypoints[0];
        }

        private static int RoundToInt(double value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

        private void SyncFromFollower()
        {
            int x = RoundToInt(this.follower.PosX);
            int y = RoundToInt(this.follower.PosY);

            // only raise PropertyChanged when something really changed
            if (x != this.X)
            {
                this.X = x;
            }

            if (y != this.Y)
            {
                this.Y = y;
            }

            if (this.follower.HeadingDeg != this.Rotation)
            {
                this.Rotation = this.follower.HeadingDeg;
            }

            this.Speed = RoundToInt(this.follower.SpeedPxPerSec);
        }
    }
}
