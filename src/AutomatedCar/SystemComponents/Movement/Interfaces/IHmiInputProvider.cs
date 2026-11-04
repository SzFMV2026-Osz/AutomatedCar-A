namespace AutomatedCar.SystemComponents.Movement.Interfaces
{
    /// <summary>Source of the driver's held controls (e.g. the keyboard), read once per cycle.</summary>
    public interface IHmiInputProvider
    {
        void ReadInput(IPedalControls controls, double dt);
    }
}
