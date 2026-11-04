namespace AutomatedCar.SystemComponents.Movement.Interfaces
{
    /// <summary>The controls a driver can hold down: pedals and steering wheel.</summary>
    public interface IPedalControls
    {
        void PressThrottle(double dt);

        void PressBrake(double dt);

        /// <summary>Turns the steering wheel.</summary>
        /// <param name="direction">-1 for left, 1 for right.</param>
        /// <param name="dt">Elapsed time in seconds.</param>
        void TurnSteering(int direction, double dt);
    }
}
