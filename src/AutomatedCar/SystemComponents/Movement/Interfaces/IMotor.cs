namespace AutomatedCar.SystemComponents.Movement.Interfaces
{
    public interface IMotor
    {
        double Rpm { get; }

        /// <summary>Gets the current torque in Nm.</summary>
        double Torque { get; }

        /// <summary>Calculates the torque based on the throttle, and lets the motor rev freely towards the throttle dependent rpm.</summary>
        /// <param name="throttle">Throttle pedal position [0..100].</param>
        /// <param name="dt">Elapsed time in seconds.</param>
        void Update(double throttle, double dt);

        /// <summary>Sets the rpm according to the wheels when the motor is connected to them.</summary>
        /// <param name="wheelRpm">Rpm of the wheels.</param>
        /// <param name="ratio">Overall gear ratio.</param>
        void SyncWithWheels(double wheelRpm, double ratio);

        /// <summary>Gets the force on the wheels, negative when driving backwards.</summary>
        /// <param name="ratio">Overall gear ratio.</param>
        /// <returns>The drive force in N.</returns>
        double GetDriveForce(double ratio);
    }
}
