namespace AutomatedCar.SystemComponents.Movement
{
    using System;

    public class SteeringSystem
    {
        public double MaxWheelAngle { get; set; } = 60; // degrees

        public double WheelBase { get; set; } = 2.8; // m

        public double CarWidth { get; set; } = 2.16; // m

        /// <summary>Maps the steering input to the angle of the steered wheels.</summary>
        /// <param name="input">Steering input [-100..100].</param>
        /// <returns>Wheel angle in degrees, negative is left.</returns>
        public double GetWheelAngle(int input)
        {
            return Math.Clamp(input, -DriverInput.SteeringMax, DriverInput.SteeringMax) * this.MaxWheelAngle / DriverInput.SteeringMax;
        }

        /// <summary>Radius of the turning circle, from atan(wheelbase / (r - width)) = angle.</summary>
        /// <param name="angle">Wheel angle in degrees.</param>
        /// <returns>Turning radius in m, infinity when going straight.</returns>
        public double GetTurningRadius(double angle)
        {
            double tan = Math.Tan(Math.Abs(angle) * Math.PI / 180);
            return tan < 1e-9 ? double.PositiveInfinity : (this.WheelBase / tan) + this.CarWidth;
        }

        /// <summary>Gets how fast the car turns. Positive is clockwise (right) on the screen.</summary>
        /// <param name="speed">Speed in m/s, negative when reversing.</param>
        /// <param name="angle">Wheel angle in degrees.</param>
        /// <returns>Yaw rate in degrees/s.</returns>
        public double GetYawRate(double speed, double angle)
        {
            double radius = this.GetTurningRadius(angle);
            if (double.IsPositiveInfinity(radius))
            {
                return 0;
            }

            return Math.Sign(angle) * speed / radius * 180 / Math.PI;
        }
    }
}
