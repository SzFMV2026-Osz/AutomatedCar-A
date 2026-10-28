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
            this.AllObjects = World.Instance.GetObjectsInTriangle(this.triangle.MainPoint, this.triangle.Pt1, this.triangle.Pt2);
            this.CameraPacket.RelevantObjects.Clear();
            this.CameraPacket.RelevantRoads.Clear();
            this.CameraPacket.RelevantEverything.Clear();
            foreach (WorldObject wb in this.AllObjects)
            {
                if (wb != this.car)
                {
                    CameraPacket.RelevantEverything.Add(wb);
                    if(wb.WorldObjectType == WorldObjectType.Road)
                    {
                        CameraPacket.RelevantRoads.Add(wb);
                    }
                    else if (wb.WorldObjectType == WorldObjectType.RoadSign)
                    {
                        CameraPacket.RelevantObjects.Add(wb);
                    }
                }
            }

            if (this.CameraPacket.ClosestObject != null)
            {
                this.CameraPacket.ClosestObject.Highlighted = false;
            }
            this.CameraPacket.ClosestObject = this.CameraPacket.RelevantObjects.OrderBy(x => this.DistanceFromRadar(x)).FirstOrDefault();
            if (this.CameraPacket.ClosestObject != null)
            {
                this.CameraPacket.ClosestObject.Highlighted = true;
            }

            //upload datas to the bus
            this.virtualFunctionBus.CameraPacket = this.CameraPacket;

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
            double frontDistance = this.car.RotationPoint.Y / 2.0;
            this.triangle.MainPoint = new Point(
                this.car.X + (frontDistance * Math.Sin(rotation)),
                this.car.Y - (frontDistance * Math.Cos(rotation)));
            this.triangle.Rotation = this.car.Rotation;
            this.triangle.SetPoints();
        }
    }
}
