namespace AutomatedCar.SystemComponents.Movement.Interfaces
{
    public interface IVehicleDynamics
    {
        /// <summary>Gets the speed in m/s, negative when going backwards.</summary>
        double Speed { get; }

        /// <summary>Calculates the new speed. The resistance and brake forces can stop the car but never move it.</summary>
        /// <param name="driveForce">Force of the motor on the wheels in N.</param>
        /// <param name="brake">Brake pedal position [0..100].</param>
        /// <param name="dt">Elapsed time in seconds.</param>
        /// <returns>The new speed in m/s.</returns>
        double UpdateSpeed(double driveForce, double brake, double dt);

        /// <summary>Gets the rpm of the wheels at the current speed.</summary>
        /// <returns>Wheel rpm, negative when going backwards.</returns>
        double GetWheelRpm();

        /// <summary>Calculates the movement of the car on the screen. Rotation 0 means the car faces up.</summary>
        /// <param name="speed">Speed in m/s.</param>
        /// <param name="yawRate">Yaw rate in degrees/s.</param>
        /// <param name="heading">Current rotation of the car in degrees.</param>
        /// <param name="dt">Elapsed time in seconds.</param>
        /// <returns>The motion vector in px and degrees.</returns>
        MotionVector CalculateMotion(double speed, double yawRate, double heading, double dt);
    }
}
