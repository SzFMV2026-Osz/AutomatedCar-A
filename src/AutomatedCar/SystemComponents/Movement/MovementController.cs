namespace AutomatedCar.SystemComponents.Movement
{
    using System;
    using AutomatedCar.SystemComponents.Packets;
    using Avalonia.Input;

    /// <summary>Moves the controlled car: powertrain (pedals, gearbox, motor) and steering.</summary>
    public class MovementController : SystemComponent
    {
        private const double MetersPerSecondToKmh = 3.6;

        private readonly Models.AutomatedCar car;
        private readonly InputResolver inputResolver = new ();
        private readonly Transmission transmission = new ();
        private readonly ElectricMotor motor = new ();
        private readonly SteeringSystem steering = new ();
        private readonly VehicleDynamics dynamics = new ();
        private readonly PowertrainPacket powertrainPacket = new ();

        // the car's coordinates are integers, the sub-pixel part of the movement is kept here
        private double x;
        private double y;

        public MovementController(VirtualFunctionBus virtualFunctionBus, Models.AutomatedCar car)
            : base(virtualFunctionBus)
        {
            this.car = car;
            this.x = car.X;
            this.y = car.Y;
            virtualFunctionBus.PowertrainPacket = this.powertrainPacket;
        }

        public Transmission Transmission => this.transmission;

        public ElectricMotor Motor => this.motor;

        public VehicleDynamics Dynamics => this.dynamics;

        public override void Process()
        {
            this.Process(1.0 / GameBase.TicksPerSecond);
        }

        public void Process(double dt)
        {
            // only the selected car listens to the keyboard
            if (this.IsSelectedCar())
            {
                this.ReadKeyboard(dt);
            }

            this.inputResolver.ApplyReturnToZero(dt);
            DriverInput input = this.inputResolver.Resolve();

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
            double speed = this.dynamics.UpdateSpeed(driveForce, brake, dt);

            double wheelAngle = this.steering.GetWheelAngle(input.Steering);
            double yawRate = this.steering.GetYawRate(speed, wheelAngle);
            MotionVector motion = this.dynamics.CalculateMotion(speed, yawRate, this.car.Rotation, dt);

            this.ApplyMotion(motion);
            this.UpdateState(input, wheelAngle);
        }

        public void SetHmiInput(DriverInput input)
        {
            this.inputResolver.SetHmiInput(input);
        }

        public void SetAssistInput(InputSource source, DriverInput input)
        {
            this.inputResolver.SetAssistInput(source, input);
        }

        /// <summary>Steps the requested gear position forward on the P-R-N-D sequence.</summary>
        public void ShiftUp()
        {
            if (this.inputResolver.HmiGear < GearPosition.D)
            {
                this.inputResolver.SetHmiGear(this.inputResolver.HmiGear + 1);
            }
        }

        /// <summary>Steps the requested gear position backward on the P-R-N-D sequence.</summary>
        public void ShiftDown()
        {
            if (this.inputResolver.HmiGear > GearPosition.P)
            {
                this.inputResolver.SetHmiGear(this.inputResolver.HmiGear - 1);
            }
        }

        private bool IsSelectedCar()
        {
            var world = Models.World.Instance;
            return world.controlledCars.Count > 0 && world.ControlledCar == this.car;
        }

        private void ReadKeyboard(double dt)
        {
            if (Keyboard.IsKeyDown(Key.Up))
            {
                this.inputResolver.PressThrottle(dt);
            }

            if (Keyboard.IsKeyDown(Key.Down))
            {
                this.inputResolver.PressBrake(dt);
            }

            int steeringDirection = (Keyboard.IsKeyDown(Key.Right) ? 1 : 0) - (Keyboard.IsKeyDown(Key.Left) ? 1 : 0);
            if (steeringDirection != 0)
            {
                this.inputResolver.TurnSteering(steeringDirection, dt);
            }
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

        private void UpdateState(DriverInput input, double wheelAngle)
        {
            double speed = this.dynamics.Speed;
            this.car.Speed = (int)Math.Round(speed * VehicleDynamics.PixelsPerMeter);
            this.car.Velocity = (int)Math.Round(Math.Abs(speed) * MetersPerSecondToKmh);
            this.car.Revolution = (int)Math.Round(this.motor.Rpm);

            this.powertrainPacket.Gear = this.transmission.Position;
            this.powertrainPacket.InternalGear = this.transmission.CurrentGear;
            this.powertrainPacket.Throttle = input.Throttle;
            this.powertrainPacket.Brake = input.Brake;
            this.powertrainPacket.Steering = input.Steering;
            this.powertrainPacket.WheelAngle = wheelAngle;
            this.powertrainPacket.Rpm = this.car.Revolution;
            this.powertrainPacket.Velocity = this.car.Velocity;
        }
    }
}
