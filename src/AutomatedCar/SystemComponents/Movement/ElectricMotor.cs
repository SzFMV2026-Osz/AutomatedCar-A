namespace AutomatedCar.SystemComponents.Movement
{
    using System;

    public class ElectricMotor
    {
        public const double IdleRpm = 800;
        public const double MaxTorque = 200; // Nm
        public const double WheelRadius = 0.33; // m

        // rpm change per second when the motor is not connected to the wheels
        private const double FreeRevRate = 4000;

        // the torque starts to drop above this portion of the max rpm
        private const double TorqueFalloffStart = 0.7;

        public double Rpm { get; private set; } = IdleRpm;

        public double Torque { get; private set; }

        public double MaxRpm { get; set; } = 6500;

        /// <summary>Calculates the torque based on the throttle, and lets the motor rev freely towards the throttle dependent rpm.</summary>
        /// <param name="throttle">Throttle pedal position [0..100].</param>
        /// <param name="dt">Elapsed time in seconds.</param>
        public void Update(double throttle, double dt)
        {
            double throttleRatio = Math.Clamp(throttle, 0, DriverInput.PedalMax) / DriverInput.PedalMax;
            this.Torque = MaxTorque * throttleRatio * this.GetTorqueFactor();

            double targetRpm = IdleRpm + ((this.MaxRpm - IdleRpm) * throttleRatio);
            double maxChange = FreeRevRate * dt;
            this.Rpm += Math.Clamp(targetRpm - this.Rpm, -maxChange, maxChange);
        }

        /// <summary>Sets the rpm according to the wheels when the motor is connected to them.</summary>
        /// <param name="wheelRpm">Rpm of the wheels.</param>
        /// <param name="ratio">Overall gear ratio.</param>
        public void SyncWithWheels(double wheelRpm, double ratio)
        {
            this.Rpm = Math.Clamp(Math.Abs(wheelRpm * ratio), IdleRpm, this.MaxRpm);
        }

        /// <summary>Gets the force on the wheels, negative when driving backwards.</summary>
        /// <param name="ratio">Overall gear ratio.</param>
        /// <returns>The drive force in N.</returns>
        public double GetDriveForce(double ratio)
        {
            return this.Torque * ratio / WheelRadius;
        }

        private double GetTorqueFactor()
        {
            double falloffStartRpm = this.MaxRpm * TorqueFalloffStart;
            if (this.Rpm <= falloffStartRpm)
            {
                return 1;
            }

            return Math.Max(0, (this.MaxRpm - this.Rpm) / (this.MaxRpm - falloffStartRpm));
        }
    }
}
