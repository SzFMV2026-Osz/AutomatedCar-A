using AutomatedCar.SystemComponents.Movement;
using Xunit;

namespace AutomatedCarTests.SystemComponents.Movement
{
    public class InputResolverTests
    {
        private const double Dt = 1.0 / 60;

        [Fact]
        public void PedalDependsOnHoldTime()
        {
            var resolver = new InputResolver();

            PressThrottleFor(resolver, 0.5);
            int afterHalfSecond = resolver.Resolve().Throttle;
            PressThrottleFor(resolver, 0.5);
            int afterOneSecond = resolver.Resolve().Throttle;

            Assert.InRange(afterHalfSecond, 1, 99);
            Assert.True(afterOneSecond > afterHalfSecond);
        }

        [Fact]
        public void PedalIsLimitedTo100()
        {
            var resolver = new InputResolver();

            PressThrottleFor(resolver, 10);

            Assert.Equal(100, resolver.Resolve().Throttle);
        }

        [Fact]
        public void ReturnsToZeroInOneSecond()
        {
            var resolver = new InputResolver();
            resolver.SetHmiInput(new DriverInput { Throttle = 100, Brake = 100, Steering = -100 });

            Wait(resolver, 0.5);
            var halfway = resolver.Resolve();
            Assert.InRange(halfway.Throttle, 1, 99);
            Assert.InRange(halfway.Steering, -99, -1);

            Wait(resolver, 0.55);
            var input = resolver.Resolve();
            Assert.Equal(0, input.Throttle);
            Assert.Equal(0, input.Brake);
            Assert.Equal(0, input.Steering);
        }

        [Fact]
        public void SteeringIsLimitedToRange()
        {
            var resolver = new InputResolver();

            for (int i = 0; i < 300; i++)
            {
                resolver.TurnSteering(-1, Dt);
                resolver.ApplyReturnToZero(Dt);
            }

            Assert.Equal(-100, resolver.Resolve().Steering);
        }

        [Fact]
        public void EmergencyBrakeOverridesDriver()
        {
            var resolver = new InputResolver();
            resolver.SetHmiInput(new DriverInput { Gear = GearPosition.D, Throttle = 80, Brake = 0 });

            resolver.SetAssistInput(InputSource.AEB, new DriverInput { Brake = 100 });
            var input = resolver.Resolve();

            Assert.Equal(100, input.Brake);
            Assert.Equal(0, input.Throttle);
            Assert.Equal(GearPosition.D, input.Gear);
        }

        [Fact]
        public void AdaptiveCruiseControlCanAccelerate()
        {
            var resolver = new InputResolver();

            resolver.SetAssistInput(InputSource.ACC, new DriverInput { Throttle = 40 });

            Assert.Equal(40, resolver.Resolve().Throttle);
        }

        [Fact]
        public void LaneKeepingOverridesSteeringUntilReleased()
        {
            var resolver = new InputResolver();
            resolver.SetHmiInput(new DriverInput { Steering = 10 });

            resolver.SetAssistInput(InputSource.LKA, new DriverInput { Steering = -30 });
            Assert.Equal(-30, resolver.Resolve().Steering);

            resolver.SetAssistInput(InputSource.LKA, null);
            Assert.Equal(10, resolver.Resolve().Steering);
        }

        private static void PressThrottleFor(InputResolver resolver, double seconds)
        {
            for (int i = 0; i < seconds / Dt; i++)
            {
                resolver.PressThrottle(Dt);
                resolver.ApplyReturnToZero(Dt);
            }
        }

        private static void Wait(InputResolver resolver, double seconds)
        {
            for (int i = 0; i < seconds / Dt; i++)
            {
                resolver.ApplyReturnToZero(Dt);
            }
        }
    }
}
