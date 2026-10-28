using AutomatedCar.SystemComponents.Movement;
using Xunit;

namespace AutomatedCarTests.SystemComponents.Movement
{
    public class MovementControllerTests
    {
        private const double Dt = 1.0 / 60;

        [Fact]
        public void DoesNotMoveInPark()
        {
            var car = CreateCar();

            car.MovementController.SetHmiInput(new DriverInput { Gear = GearPosition.P, Throttle = 100 });
            Run(car, 60);

            Assert.Equal(0, car.MovementController.Dynamics.Speed);
            Assert.Equal(1000, car.Y);
        }

        [Fact]
        public void AcceleratesForwardInDrive()
        {
            var car = CreateCar();

            Hold(car, GearPosition.D, throttle: 50, seconds: 3);

            Assert.True(car.MovementController.Dynamics.Speed > 0);
            Assert.True(car.Y < 1000);
            Assert.Equal(1000, car.X);
            Assert.True(car.Velocity > 0);
        }

        [Fact]
        public void MoreThrottleAcceleratesFaster()
        {
            var slow = CreateCar();
            var fast = CreateCar();

            Hold(slow, GearPosition.D, throttle: 30, seconds: 2);
            Hold(fast, GearPosition.D, throttle: 90, seconds: 2);

            Assert.True(fast.MovementController.Dynamics.Speed > slow.MovementController.Dynamics.Speed);
        }

        [Fact]
        public void UsesInternalGearsWhileAccelerating()
        {
            var car = CreateCar();

            Hold(car, GearPosition.D, throttle: 100, seconds: 8);

            Assert.True(car.MovementController.Transmission.CurrentGear > 1);
        }

        [Fact]
        public void ReversesInR()
        {
            var car = CreateCar();

            Hold(car, GearPosition.R, throttle: 50, seconds: 2);

            Assert.True(car.MovementController.Dynamics.Speed < 0);
            Assert.True(car.Y > 1000);
        }

        [Fact]
        public void NeutralDoesNotAccelerate()
        {
            var car = CreateCar();

            Hold(car, GearPosition.N, throttle: 100, seconds: 2);

            Assert.Equal(0, car.MovementController.Dynamics.Speed);
            Assert.True(car.Revolution > 3000);
        }

        [Fact]
        public void SlowsDownAndStopsAfterReleasingThrottle()
        {
            var car = CreateCar();
            Hold(car, GearPosition.D, throttle: 60, seconds: 3);

            // the released pedal returns to zero within 1 second
            Run(car, 60);
            double speed = car.MovementController.Dynamics.Speed;

            Run(car, 60);
            Assert.InRange(car.MovementController.Dynamics.Speed, 0.01, speed - 0.01);

            Run(car, 60 * 120);
            Assert.Equal(0, car.MovementController.Dynamics.Speed);
        }

        [Fact]
        public void TurnsRightWhenSteeringRight()
        {
            var car = CreateCar();

            for (int i = 0; i < 120; i++)
            {
                car.MovementController.SetHmiInput(new DriverInput { Gear = GearPosition.D, Throttle = 40, Steering = 50 });
                car.MovementController.Process(Dt);
            }

            Assert.True(car.Rotation > 0);
            Assert.True(car.X > 1000);
        }

        [Fact]
        public void EmergencyBrakeStopsTheCarDespiteThrottle()
        {
            var car = CreateCar();
            Hold(car, GearPosition.D, throttle: 60, seconds: 3);

            car.MovementController.SetAssistInput(InputSource.AEB, new DriverInput { Brake = 100 });
            Hold(car, GearPosition.D, throttle: 100, seconds: 3);

            Assert.Equal(0, car.MovementController.Dynamics.Speed);
        }

        [Fact]
        public void GearboxStepsOnePositionPerCycle()
        {
            var car = CreateCar();

            car.MovementController.ShiftUp();
            car.MovementController.ShiftUp();
            car.MovementController.ShiftUp();
            car.MovementController.ShiftUp();
            car.MovementController.Process(Dt);
            Assert.Equal(GearPosition.R, car.MovementController.Transmission.Position);

            Run(car, 5);
            Assert.Equal(GearPosition.D, car.MovementController.Transmission.Position);

            car.MovementController.ShiftDown();
            Run(car, 5);
            Assert.Equal(GearPosition.N, car.MovementController.Transmission.Position);
        }

        private static AutomatedCar.Models.AutomatedCar CreateCar()
        {
            return new AutomatedCar.Models.AutomatedCar(1000, 1000, "car_1_white.png");
        }

        private static void Hold(AutomatedCar.Models.AutomatedCar car, GearPosition gear, int throttle, double seconds)
        {
            for (int i = 0; i < seconds / Dt; i++)
            {
                car.MovementController.SetHmiInput(new DriverInput { Gear = gear, Throttle = throttle });
                car.MovementController.Process(Dt);
            }
        }

        private static void Run(AutomatedCar.Models.AutomatedCar car, int ticks)
        {
            for (int i = 0; i < ticks; i++)
            {
                car.MovementController.Process(Dt);
            }
        }
    }
}
