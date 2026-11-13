namespace AutomatedCar.SystemComponents.Movement
{
    using System;
    using AutomatedCar.SystemComponents.Movement.Interfaces;
    using AutomatedCar.SystemComponents.Packets;

    /// <summary>Moves the controlled car: powertrain (pedals, gearbox, motor) and steering.</summary>
    public class MovementController : SystemComponent, IMovementController
    {
        private readonly Models.AutomatedCar car;
        private readonly IHmiInputProvider hmiInputProvider;
        private readonly IInputResolver inputResolver;
        private readonly ITransmission transmission;
        private readonly IMotor motor;
        private readonly ISteeringSystem steering;
        private readonly IVehicleDynamics dynamics;
        private readonly PowertrainPacket powertrainPacket = new ();

        // the car's coordinates are integers, the sub-pixel part of the movement is kept here
        private double x;
        private double y;

        public MovementController(VirtualFunctionBus virtualFunctionBus, Models.AutomatedCar car)
            : this(
                virtualFunctionBus,
                car,
                new KeyboardInputProvider(car),
                new InputResolver(),
                new Transmission(),
                new ElectricMotor(),
                new SteeringSystem(),
                new VehicleDynamics())
        {
        }

        public MovementController(
            VirtualFunctionBus virtualFunctionBus,
            Models.AutomatedCar car,
            IHmiInputProvider hmiInputProvider,
            IInputResolver inputResolver,
            ITransmission transmission,
            IMotor motor,
            ISteeringSystem steering,
            IVehicleDynamics dynamics)
            : base(virtualFunctionBus)
        {
            this.car = car;
            this.hmiInputProvider = hmiInputProvider;
            this.inputResolver = inputResolver;
            this.transmission = transmission;
            this.motor = motor;
            this.steering = steering;
            this.dynamics = dynamics;
            this.x = car.X;
            this.y = car.Y;
            virtualFunctionBus.PowertrainPacket = this.powertrainPacket;
        }

        public double Speed => this.dynamics.Speed;

        public GearPosition Gear => this.transmission.Position;

        public int InternalGear => this.transmission.CurrentGear;

        public override void Process()
        {
            this.Process(1.0 / GameBase.TicksPerSecond);
        }

        public void Process(double dt)
        {
            DriverInput input = this.ReadInput(dt);
            double speed = this.UpdatePowertrain(input, dt);

            double wheelAngle = this.steering.GetWheelAngle(input.Steering);
            double yawRate = this.steering.GetYawRate(speed, wheelAngle);
            this.ApplyMotion(this.dynamics.CalculateMotion(speed, yawRate, this.car.Rotation, dt));

            this.PublishState(input, wheelAngle);
        }

        public void SetHmiInput(DriverInput input)
        {
            this.inputResolver.SetHmiInput(input);
        }

        public void SetAssistInput(InputSource source, DriverInput input)
        {
            this.inputResolver.SetAssistInput(source, input);
        }

        public void ShiftUp()
        {
            if (this.inputResolver.HmiGear < GearPosition.D)
            {
                this.inputResolver.SetHmiGear(this.inputResolver.HmiGear + 1);
            }
        }

        public void ShiftDown()
        {
            if (this.inputResolver.HmiGear > GearPosition.P)
            {
                this.inputResolver.SetHmiGear(this.inputResolver.HmiGear - 1);
            }
        }

        private DriverInput ReadInput(double dt)
        {
            this.hmiInputProvider.ReadInput(this.inputResolver, dt);
            this.inputResolver.ApplyReturnToZero(dt);
            return this.inputResolver.Resolve();
        }

        /// <summary>Gearbox, motor and forces: calculates the new speed from the input.</summary>
        private double UpdatePowertrain(DriverInput input, double dt)
        {
            this.SyncGearPosition(input.Gear);
            this.transmission.UpdateGear(this.motor.Rpm, this.dynamics.Speed);
            double ratio = this.transmission.GetRatio();

            this.motor.Update(input.Throttle, dt);
            double driveForce = 0;
            if (this.transmission.IsDriveEnabled())
            {
                this.motor.SyncWithWheels(this.dynamics.GetWheelRpm(), ratio);
                driveForce = this.motor.GetDriveForce(ratio);
            }

            // in P the parking lock (and the hand brake) holds the car
            int brake = this.transmission.Position == GearPosition.P ? DriverInput.PedalMax : input.Brake;
            return this.dynamics.UpdateSpeed(driveForce, brake, dt);
        }

        // the gearbox is sequential, it steps one position per cycle towards the requested one
        private void SyncGearPosition(GearPosition requested)
        {
            if (this.transmission.Position < requested)
            {
                this.transmission.SelectNextPosition();
            }
            else if (this.transmission.Position > requested)
            {
                this.transmission.SelectPreviousPosition();
            }
        }

        private void ApplyMotion(MotionVector motion)
        {
            // someone else moved the car, continue from there
            if (this.car.X != (int)Math.Round(this.x) || this.car.Y != (int)Math.Round(this.y))
            {
                this.x = this.car.X;
                this.y = this.car.Y;
            }

            this.x += motion.DeltaX;
            this.y += motion.DeltaY;
            this.car.X = (int)Math.Round(this.x);
            this.car.Y = (int)Math.Round(this.y);

            if (motion.DeltaRotation != 0)
            {
                this.car.Rotation += motion.DeltaRotation;
            }
        }

        private void PublishState(DriverInput input, double wheelAngle)
        {
            double speed = this.dynamics.Speed;
            this.car.Speed = (int)Math.Round(speed * PhysicsConstants.PixelsPerMeter);
            this.car.Velocity = (int)Math.Round(Math.Abs(speed) * PhysicsConstants.MetersPerSecondToKmh);
            this.car.Revolution = (int)Math.Round(this.motor.Rpm);

            this.powertrainPacket.Gear = this.transmission.Position;
            this.powertrainPacket.InternalGear = this.transmission.CurrentGear;
            this.powertrainPacket.Throttle = input.Throttle;
            this.powertrainPacket.Brake = input.Brake;
            this.powertrainPacket.DriverThrottle = this.inputResolver.DriverThrottle;
            this.powertrainPacket.DriverBrake = this.inputResolver.DriverBrake;
            this.powertrainPacket.Steering = input.Steering;
            this.powertrainPacket.WheelAngle = wheelAngle;
            this.powertrainPacket.Rpm = this.car.Revolution;
            this.powertrainPacket.Velocity = this.car.Velocity;
        }
    }
}
