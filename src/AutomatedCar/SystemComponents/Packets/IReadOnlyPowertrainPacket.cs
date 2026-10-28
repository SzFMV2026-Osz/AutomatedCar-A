namespace AutomatedCar.SystemComponents.Packets
{
    using AutomatedCar.SystemComponents.Movement;

    public interface IReadOnlyPowertrainPacket
    {
        GearPosition Gear { get; }

        int InternalGear { get; }

        int Throttle { get; }

        int Brake { get; }

        int Steering { get; }

        double WheelAngle { get; }

        int Rpm { get; }

        /// <summary>Gets the speed in km/h.</summary>
        int Velocity { get; }
    }
}
