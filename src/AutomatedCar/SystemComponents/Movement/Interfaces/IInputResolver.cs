namespace AutomatedCar.SystemComponents.Movement.Interfaces
{
    /// <summary>Merges the driver (HMI) input with the higher priority inputs of the driver assistance systems.</summary>
    public interface IInputResolver : IPedalControls
    {
        GearPosition HmiGear { get; }

        /// <summary>Overrides the HMI input with absolute values.</summary>
        /// <param name="input">The new HMI input.</param>
        void SetHmiInput(DriverInput input);

        void SetHmiGear(GearPosition gear);

        /// <summary>Sets the input of an assistance system, null releases the control.</summary>
        /// <param name="source">The assistance system.</param>
        /// <param name="input">Its input, or null.</param>
        void SetAssistInput(InputSource source, DriverInput input);

        /// <summary>Gradually returns the pedals and the steering wheel that were not held in this cycle to their default positions.</summary>
        /// <param name="dt">Elapsed time in seconds.</param>
        void ApplyReturnToZero(double dt);

        DriverInput Resolve();
    }
}
