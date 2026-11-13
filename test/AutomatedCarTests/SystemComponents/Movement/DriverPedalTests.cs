using AutomatedCar.SystemComponents.Movement;
using Xunit;

namespace AutomatedCarTests.SystemComponents.Movement
{
    public class DriverPedalTests
    {
        private const double Dt = 1.0 / 60;

        [Fact]
        public void ResolverKeepsDriverBrakeSeparateFromAssistBrake()
        {
            var resolver = new InputResolver();
            resolver.SetHmiInput(new DriverInput { Gear = GearPosition.D, Throttle = 20 });

            resolver.SetAssistInput(InputSource.ACC, new DriverInput { Brake = 30 });

            Assert.Equal(30, resolver.Resolve().Brake);
            Assert.Equal(0, resolver.DriverBrake);
            Assert.Equal(20, resolver.DriverThrottle);
        }

        [Fact]
        public void PacketShowsWhoPressedTheBrake()
        {
            var car = CreateCar();
            car.MovementController.SetHmiInput(new DriverInput { Gear = GearPosition.D });
            car.MovementController.SetAssistInput(InputSource.ACC, new DriverInput { Brake = 30 });

            car.MovementController.Process(Dt);
            var packet = car.VirtualFunctionBus.PowertrainPacket;

            Assert.Equal(30, packet.Brake);
            Assert.Equal(0, packet.DriverBrake);
        }

        [Fact]
        public void PacketShowsDriverThrottleWhileEmergencyBrakeCutsIt()
        {
            var car = CreateCar();
            car.MovementController.SetHmiInput(new DriverInput { Gear = GearPosition.D, Throttle = 100, Brake = 50 });
            car.MovementController.SetAssistInput(InputSource.AEB, new DriverInput { Brake = 100 });

            car.MovementController.Process(Dt);
            var packet = car.VirtualFunctionBus.PowertrainPacket;

            Assert.Equal(0, packet.Throttle);
            Assert.Equal(100, packet.Brake);
            Assert.InRange(packet.DriverThrottle, 98, 100);
            Assert.InRange(packet.DriverBrake, 48, 50);
        }

        private static AutomatedCar.Models.AutomatedCar CreateCar()
        {
            return new AutomatedCar.Models.AutomatedCar(1000, 1000, "car_1_white.png");
        }
    }
}
