namespace AutomatedCar.SystemComponents.Movement
{
    using AutomatedCar.SystemComponents.Movement.Interfaces;
    using Avalonia.Input;

    /// <summary>Reads the arrow keys as pedals and steering. Only the currently selected car listens to them.</summary>
    public class KeyboardInputProvider : IHmiInputProvider
    {
        private readonly Models.AutomatedCar car;

        public KeyboardInputProvider(Models.AutomatedCar car)
        {
            this.car = car;
        }

        public void ReadInput(IPedalControls controls, double dt)
        {
            if (!this.IsSelectedCar())
            {
                return;
            }

            if (Keyboard.IsKeyDown(Key.Up))
            {
                controls.PressThrottle(dt);
            }

            if (Keyboard.IsKeyDown(Key.Down))
            {
                controls.PressBrake(dt);
            }

            int steeringDirection = (Keyboard.IsKeyDown(Key.Right) ? 1 : 0) - (Keyboard.IsKeyDown(Key.Left) ? 1 : 0);
            if (steeringDirection != 0)
            {
                controls.TurnSteering(steeringDirection, dt);
            }
        }

        private bool IsSelectedCar()
        {
            var world = Models.World.Instance;
            return world.controlledCars.Count > 0 && world.ControlledCar == this.car;
        }
    }
}
