namespace AutomatedCar.SystemComponents
{
    using Avalonia;
    using AutomatedCar.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutomatedCar.SystemComponents.Packets;

    public class Radar : SystemComponent
    {
        private const double LaneHalfWidth = 82.5;

        private readonly AutomatedCar car;

        private Dictionary<WorldObject, double> previousLateralOffsets = new Dictionary<WorldObject, double>();
        public Triangle triangle { get; set; }
        public List<WorldObject> AllObjects { get; set; }
        public RadarPacket RadarPacket { get; set; }

        public Radar(VirtualFunctionBus virtualFunctionBus, Triangle triangle, AutomatedCar car) : base(virtualFunctionBus)
        {
            this.triangle = triangle;
            this.AllObjects = new List<WorldObject>();
            this.RadarPacket = new RadarPacket();
            this.car = car;
        }

        

        public override void Process()
        {
            //set the sensor triangle
            this.UpdataTriangle();

            this.AllObjects = World.Instance.GetObjectsInTriangle(this.triangle.MainPoint, this.triangle.Pt1, this.triangle.Pt2);
            this.RadarPacket.RelevantObjects.Clear();
            foreach(WorldObject wb in  this.AllObjects)
            {
                if(wb != this.car && (wb.Collideable || wb.WorldObjectType == WorldObjectType.Car || wb.WorldObjectType == WorldObjectType.Pedestrian))
                {
                    RadarPacket.RelevantObjects.Add(wb);
                }
            }

            this.FindInLaneObjects();

            if (this.RadarPacket.ClosestObject != null)
            {
                this.RadarPacket.ClosestObject.Highlighted = false;
            }
            this.RadarPacket.ClosestObject = this.RadarPacket.RelevantObjects.OrderBy(x => this.DistanceFromRadar(x)).FirstOrDefault();
            if (this.RadarPacket.ClosestObject != null)
            {
               this.RadarPacket.ClosestObject.Highlighted = true;
            }

            //upload datas to the bus
            this.virtualFunctionBus.RadarPacket = this.RadarPacket;

        }

        private void FindInLaneObjects()
        {
            var lateralOffsets = new Dictionary<WorldObject, double>();
            double nearestForward = double.MaxValue;
            this.RadarPacket.NearestInLane = null;
            this.RadarPacket.AebRelevantObjects = new List<WorldObject>();

            double rotation = this.car.Rotation * Math.PI / 180.0;
            foreach (WorldObject wb in this.RadarPacket.RelevantObjects)
            {
                double dx = wb.X - this.car.X;
                double dy = wb.Y - this.car.Y;
                double lateralOffset = Math.Abs((dx * Math.Cos(rotation)) + (dy * Math.Sin(rotation)));
                double forward = (dx * Math.Sin(rotation)) - (dy * Math.Cos(rotation));
                bool inLane = lateralOffset <= LaneHalfWidth;

                if (inLane && forward < nearestForward)
                {
                    this.RadarPacket.NearestInLane = wb;
                    nearestForward = forward;
                }

                bool approaching = this.previousLateralOffsets.TryGetValue(wb, out double previous) && lateralOffset < previous;
                if (inLane || approaching)
                {
                    this.RadarPacket.AebRelevantObjects.Add(wb);
                }

                lateralOffsets[wb] = lateralOffset;
            }

            this.previousLateralOffsets = lateralOffsets;
        }

        private double DistanceFromRadar(WorldObject worldObject)
        {
            double dx = worldObject.X - this.triangle.MainPoint.X;
            double dy = worldObject.Y - this.triangle.MainPoint.Y;
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        private void UpdataTriangle()
        {
            double rotation = this.car.Rotation * Math.PI / 180.0;
            double frontDistance = this.car.RotationPoint.Y;
            this.triangle.MainPoint = new Point(
                this.car.X + (frontDistance * Math.Sin(rotation)),
                this.car.Y - (frontDistance * Math.Cos(rotation)));
            this.triangle.Rotation = this.car.Rotation;
            this.triangle.SetPoints();
        }
    }
}
