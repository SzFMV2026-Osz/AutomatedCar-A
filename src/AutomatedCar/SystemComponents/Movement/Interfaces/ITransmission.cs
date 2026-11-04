namespace AutomatedCar.SystemComponents.Movement.Interfaces
{
    /// <summary>Sequential automatic gearbox with external positions (P, R, N, D) and internal gears in D.</summary>
    public interface ITransmission
    {
        GearPosition Position { get; }

        /// <summary>Gets the internal gear while in D, 0 otherwise.</summary>
        int CurrentGear { get; }

        void SelectNextPosition();

        void SelectPreviousPosition();

        /// <summary>Shifts the internal gears in D based on the motor rpm.</summary>
        /// <param name="rpm">Current motor rpm.</param>
        /// <param name="speed">Current speed of the car in m/s.</param>
        void UpdateGear(double rpm, double speed);

        /// <summary>Gets the overall ratio between the motor and the wheels, negative in reverse, 0 when there is no drive.</summary>
        /// <returns>The overall gear ratio.</returns>
        double GetRatio();

        bool IsDriveEnabled();
    }
}
