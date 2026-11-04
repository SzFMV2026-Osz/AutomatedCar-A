namespace AutomatedCar.SystemComponents.Movement.Interfaces
{
    public interface ISteeringSystem
    {
        /// <summary>Maps the steering input to the angle of the steered wheels.</summary>
        /// <param name="input">Steering input [-100..100].</param>
        /// <returns>Wheel angle in degrees, negative is left.</returns>
        double GetWheelAngle(int input);

        /// <summary>Radius of the turning circle.</summary>
        /// <param name="angle">Wheel angle in degrees.</param>
        /// <returns>Turning radius in m, infinity when going straight.</returns>
        double GetTurningRadius(double angle);

        /// <summary>Gets how fast the car turns. Positive is clockwise (right) on the screen.</summary>
        /// <param name="speed">Speed in m/s, negative when reversing.</param>
        /// <param name="angle">Wheel angle in degrees.</param>
        /// <returns>Yaw rate in degrees/s.</returns>
        double GetYawRate(double speed, double angle);
    }
}
