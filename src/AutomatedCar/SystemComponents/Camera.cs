namespace AutomatedCar.SystemComponents
{
    using AutomatedCar.Models;
    using AutomatedCar.SystemComponents.Packets;
    using Avalonia;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Camera : SystemComponent
    {
        public AutomatedCar car { get; set; }
        public Triangle triangle { get; set; }
        public List<WorldObject> AllObjects { get; set; }
        public CameraPacket CameraPacket { get; set; }

        public Camera(VirtualFunctionBus virtualFunctionBus, Triangle triangle, AutomatedCar car) : base(virtualFunctionBus)
        {
            this.triangle = triangle;
            this.AllObjects = new List<WorldObject>();
            this.CameraPacket = new CameraPacket();
            this.car = car;
        }
        public override void Process()
        {
            //set the sensor triangle
            this.UpdataTriangle();

            //get all wordobj
            this.AllObjects = World.Instance.WorldObjects.ToList();
            this.CameraPacket.RelevantObjects.Clear();
            foreach (WorldObject wb in this.AllObjects)
            {
                if (IsItIn(wb) && wb != this.car)
                {
                    CameraPacket.RelevantEverything.Add(wb);
                    if(wb.WorldObjectType == WorldObjectType.Road)
                    {
                        CameraPacket.RelevantRoads.Add(wb);
                    }
                    else
                    {
                        CameraPacket.RelevantObjects.Add(wb);
                    }
                }
            }

            if (this.CameraPacket.ClosestObject != null)
            {
                this.CameraPacket.ClosestObject.Highlighted = false;
                this.CameraPacket.ClosestObject = this.CameraPacket.RelevantObjects.OrderBy(x => this.DistanceFromRadar(x)).FirstOrDefault();
                this.CameraPacket.ClosestObject.Highlighted = true;
            }

            //upload datas to the bus
            this.virtualFunctionBus.CameraPacket = this.CameraPacket;

        }

        private bool IsItIn(WorldObject wb)
        {
            Point p = new Point(wb.X, wb.Y);

            Point a = this.triangle.MainPoint;
            Point b = this.triangle.Pt1;
            Point c = this.triangle.Pt2;

            double d1 = Sign(p, a, b);
            double d2 = Sign(p, b, c);
            double d3 = Sign(p, c, a);

            bool hasNegative = d1 < 0 || d2 < 0 || d3 < 0;
            bool hasPositive = d1 > 0 || d2 > 0 || d3 > 0;

            return !(hasNegative && hasPositive);
        }

        private double Sign(Point p1, Point p2, Point p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
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
