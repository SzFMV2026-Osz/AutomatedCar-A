namespace AutomatedCar.SystemComponents.Packets
{
    using AutomatedCar.SystemComponents.Movement;
    using ReactiveUI;

    public class PowertrainPacket : ReactiveObject, IReadOnlyPowertrainPacket
    {
        private GearPosition gear;
        private int internalGear;
        private int throttle;
        private int brake;
        private int steering;
        private double wheelAngle;
        private int rpm;
        private int velocity;

        public GearPosition Gear
        {
            get => this.gear;
            set => this.RaiseAndSetIfChanged(ref this.gear, value);
        }

        public int InternalGear
        {
            get => this.internalGear;
            set => this.RaiseAndSetIfChanged(ref this.internalGear, value);
        }

        public int Throttle
        {
            get => this.throttle;
            set => this.RaiseAndSetIfChanged(ref this.throttle, value);
        }

        public int Brake
        {
            get => this.brake;
            set => this.RaiseAndSetIfChanged(ref this.brake, value);
        }

        public int Steering
        {
            get => this.steering;
            set => this.RaiseAndSetIfChanged(ref this.steering, value);
        }

        public double WheelAngle
        {
            get => this.wheelAngle;
            set => this.RaiseAndSetIfChanged(ref this.wheelAngle, value);
        }

        public int Rpm
        {
            get => this.rpm;
            set => this.RaiseAndSetIfChanged(ref this.rpm, value);
        }

        public int Velocity
        {
            get => this.velocity;
            set => this.RaiseAndSetIfChanged(ref this.velocity, value);
        }
    }
}
