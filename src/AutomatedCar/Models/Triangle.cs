namespace AutomatedCar.Models
{
    using Avalonia;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Triangle
    {
        public double CentralAngle { get; set; }
        public double Height { get; set; }
        public double Rotation { get; set; }
        public Point MainPoint { get; set; }
        public Point Pt1 { get; set; }
        public Point Pt2 { get; set; }

        public Triangle(double CentralAngle, double Height, double Rotation, Point point)
        {
            this.CentralAngle = CentralAngle;
            this.Height = Height;
            this.Rotation = Rotation;
            this.MainPoint = point;

            SetPoints();
        }

        public void SetPoints()
        {
            //before function set the mainPoint to the car x-y coordinate
            double halfBase = this.Height * Math.Tan(DegreeToRadian(this.CentralAngle / 2));
            double rotation = DegreeToRadian(this.Rotation);

            double sin = Math.Sin(rotation);
            double cos = Math.Cos(rotation);

            double centerX = this.MainPoint.X + (this.Height * sin);
            double centerY = this.MainPoint.Y - (this.Height * cos);

            this.Pt1 = new Point(centerX - (halfBase * cos), centerY - (halfBase * sin));
            this.Pt2 = new Point(centerX + (halfBase * cos), centerY + (halfBase * sin));
        }
        private double DegreeToRadian(double degree)
        {
            return degree * Math.PI / 180.0;
        }
    }
}
