namespace AutomatedCar.SystemComponents.Movement.Interfaces
{
    /// <summary>What the other modules (HMI, driver assistance systems) can see and use of the movement module.</summary>
    public interface IMovementController
    {
        /// <summary>Gets the speed in m/s, negative when going backwards.</summary>
        double Speed { get; }

        GearPosition Gear { get; }

        /// <summary>Gets the internal gear while in D, 0 otherwise.</summary>
        int InternalGear { get; }

        /// <summary>Calculates one cycle of the movement.</summary>
        /// <param name="dt">Elapsed time in seconds.</param>
        void Process(double dt);

        void SetHmiInput(DriverInput input);

        /// <summary>Sets the input of an assistance system, null releases the control.</summary>
        /// <param name="source">The assistance system.</param>
        /// <param name="input">Its input, or null.</param>
        void SetAssistInput(InputSource source, DriverInput input);

        /// <summary>Steps the requested gear position forward on the P-R-N-D sequence.</summary>
        void ShiftUp();

        /// <summary>Steps the requested gear position backward on the P-R-N-D sequence.</summary>
        void ShiftDown();
    }
}
