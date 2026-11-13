namespace AutomatedCar.SystemComponents.Packets
{
    using AutomatedCar.SystemComponents.Movement;

    public interface IReadOnlyPowertrainPacket
    {
        GearPosition Gear { get; }

        int InternalGear { get; }

        int Throttle { get; }

        int Brake { get; }

        /// <summary>Gets the throttle pressed by the driver alone, without the driver assistance systems.</summary>
        int DriverThrottle { get; }

        /// <summary>Gets the brake pressed by the driver alone, without the driver assistance systems.</summary>
        int DriverBrake { get; }

        int Steering { get; }

        double WheelAngle { get; }

        int Rpm { get; }

        int Velocity { get; }
    }
}
