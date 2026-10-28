namespace AutomatedCar.SystemComponents.Movement
{
    public class DriverInput
    {
        public const int PedalMax = 100;
        public const int SteeringMax = 100;

        public GearPosition Gear { get; set; } = GearPosition.P;

        /// <summary>Gets or sets the throttle pedal position on a [0..100] scale.</summary>
        public int Throttle { get; set; }

        /// <summary>Gets or sets the brake pedal position on a [0..100] scale.</summary>
        public int Brake { get; set; }

        /// <summary>Gets or sets the steering wheel position on a [-100..100] scale, negative is left.</summary>
        public int Steering { get; set; }
    }
}
