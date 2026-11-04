using AutomatedCar.SystemComponents;
using AutomatedCar.SystemComponents.Movement;
using AutomatedCar.SystemComponents.Movement.Interfaces;
using Xunit;

namespace AutomatedCarTests.SystemComponents.Movement
{
    public class MovementControllerInjectionTests
    {
        private const double Dt = 1.0 / 60;

        [Fact]
        public void ReadsHeldControlsFromTheInputProvider()
        {
            var car = CreateCar();
            var provider = new HeldThrottleProvider();
            var controller = CreateController(car, provider, new SteeringSystem());
            controller.SetHmiInput(new DriverInput { Gear = GearPosition.D });

            for (int i = 0; i < 120; i++)
            {
                controller.Process(Dt);
            }

            Assert.True(provider.Calls >= 120);
            Assert.True(controller.Speed > 0);
            Assert.True(car.Y < 1000);
        }

        [Fact]
        public void RotatesTheCarByTheYawRateOfTheSteeringSystem()
        {
            var car = CreateCar();
            var controller = CreateController(car, new NoInputProvider(), new ConstantYawSteering(30));

            controller.Process(1);

            Assert.Equal(30, car.Rotation, 6);
        }

        private static AutomatedCar.Models.AutomatedCar CreateCar()
        {
            return new AutomatedCar.Models.AutomatedCar(1000, 1000, "car_1_white.png");
        }

        private static MovementController CreateController(AutomatedCar.Models.AutomatedCar car, IHmiInputProvider provider, ISteeringSystem steering)
        {
            return new MovementController(
                new VirtualFunctionBus(),
                car,
                provider,
                new InputResolver(),
                new Transmission(),
                new ElectricMotor(),
                steering,
                new VehicleDynamics());
        }

        private class HeldThrottleProvider : IHmiInputProvider
        {
            public int Calls { get; private set; }

            public void ReadInput(IPedalControls controls, double dt)
            {
                this.Calls++;
                controls.PressThrottle(dt);
            }
        }

        private class NoInputProvider : IHmiInputProvider
        {
            public void ReadInput(IPedalControls controls, double dt)
            {
            }
        }

        private class ConstantYawSteering : ISteeringSystem
        {
            private readonly double yawRate;

            public ConstantYawSteering(double yawRate)
            {
                this.yawRate = yawRate;
            }

            public double GetWheelAngle(int input) => 0;

            public double GetTurningRadius(double angle) => double.PositiveInfinity;

            public double GetYawRate(double speed, double angle) => this.yawRate;
        }
    }
}
