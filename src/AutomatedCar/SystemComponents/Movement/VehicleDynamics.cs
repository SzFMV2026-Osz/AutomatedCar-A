namespace AutomatedCar.SystemComponents.Movement
{
    using System;

    public class VehicleDynamics
    {
        public const double PixelsPerMeter = 50;

        public double Mass { get; set; } = 1500; // kg

        /// <summary>Gets or sets the constant resistance force (rolling resistance and drivetrain losses) in N.</summary>
        public double RollingResistance { get; set; } = 600;

        /// <summary>Gets or sets the air drag coefficient, the drag force is AirDrag * v^2 in N.</summary>
        public double AirDrag { get; set; } = 0.4;

        public double BrakeForceMax { get; set; } = 13500; // N, ~9 m/s^2

        /// <summary>Gets or sets the speed in m/s, negative when going backwards.</summary>
        public double Speed { get; set; }

        /// <summary>Calculates the new speed. The resistance and brake forces can stop the car but never move it.</summary>
        /// <param name="driveForce">Force of the motor on the wheels in N.</param>
        /// <param name="brake">Brake pedal position [0..100].</param>
        /// <param name="dt">Elapsed time in seconds.</param>
        /// <returns>The new speed in m/s.</returns>
        public double UpdateSpeed(double driveForce, double brake, double dt)
        {
            double brakeForce = this.BrakeForceMax * Math.Clamp(brake, 0, DriverInput.PedalMax) / DriverInput.PedalMax;
            double resistance = this.RollingResistance + (this.AirDrag * this.Speed * this.Speed) + brakeForce;

            double speed = this.Speed + (driveForce / this.Mass * dt);
            double speedLoss = resistance / this.Mass * dt;
            this.Speed = Math.Abs(speed) <= speedLoss ? 0 : speed - (Math.Sign(speed) * speedLoss);

            return this.Speed;
        }

        /// <summary>Gets the rpm of the wheels at the current speed.</summary>
        /// <returns>Wheel rpm, negative when going backwards.</returns>
        public double GetWheelRpm()
        {
            return this.Speed / (2 * Math.PI * ElectricMotor.WheelRadius) * 60;
        }

        /// <summary>Calculates the movement of the car on the screen. Rotation 0 means the car faces up.</summary>
        /// <param name="speed">Speed in m/s.</param>
        /// <param name="yawRate">Yaw rate in degrees/s.</param>
        /// <param name="heading">Current rotation of the car in degrees.</param>
        /// <param name="dt">Elapsed time in seconds.</param>
        /// <returns>The motion vector in px and degrees.</returns>
        public MotionVector CalculateMotion(double speed, double yawRate, double heading, double dt)
        {
            double deltaRotation = yawRate * dt;
            double averageHeading = (heading + (deltaRotation / 2)) * Math.PI / 180;
            double distance = speed * dt * PixelsPerMeter;

            return new MotionVector(
                distance * Math.Sin(averageHeading),
                -distance * Math.Cos(averageHeading),
                deltaRotation);
        }
    }
}
