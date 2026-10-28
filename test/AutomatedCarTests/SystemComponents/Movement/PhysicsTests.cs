using AutomatedCar.SystemComponents.Movement;
using Xunit;

namespace AutomatedCarTests.SystemComponents.Movement
{
    public class PhysicsTests
    {
        [Fact]
        public void MoreThrottleGivesMoreDriveForce()
        {
            var weak = new ElectricMotor();
            var strong = new ElectricMotor();

            weak.Update(20, 0.1);
            strong.Update(80, 0.1);

            Assert.True(strong.GetDriveForce(10) > weak.GetDriveForce(10));
        }

        [Fact]
        public void MotorRevsUpInNeutral()
        {
            var motor = new ElectricMotor();

            for (int i = 0; i < 120; i++)
            {
                motor.Update(100, 1.0 / 60);
            }

            Assert.True(motor.Rpm > 5000);
        }

        [Fact]
        public void ResistanceSlowsDownAndStopsTheCar()
        {
            var dynamics = new VehicleDynamics { Speed = 10 };

            dynamics.UpdateSpeed(0, 0, 1);
            Assert.InRange(dynamics.Speed, 0.01, 9.99);

            for (int i = 0; i < 1000; i++)
            {
                dynamics.UpdateSpeed(0, 0, 1);
            }

            Assert.Equal(0, dynamics.Speed);
        }

        [Fact]
        public void BrakeDoesNotMoveTheCarBackwards()
        {
            var dynamics = new VehicleDynamics { Speed = 1 };

            dynamics.UpdateSpeed(0, 100, 1);

            Assert.Equal(0, dynamics.Speed);
        }

        [Fact]
        public void StraightMotionAlongHeading()
        {
            var dynamics = new VehicleDynamics();

            var up = dynamics.CalculateMotion(1, 0, 0, 1);
            var right = dynamics.CalculateMotion(1, 0, 90, 1);

            Assert.Equal(0, up.DeltaX, 6);
            Assert.Equal(-VehicleDynamics.PixelsPerMeter, up.DeltaY, 6);
            Assert.Equal(VehicleDynamics.PixelsPerMeter, right.DeltaX, 6);
            Assert.Equal(0, right.DeltaY, 6);
        }

        [Fact]
        public void TurningRadiusFollowsHandoutFormula()
        {
            // atan(130px / (r - 90px)) = 60° → r ≈ 165px
            var steering = new SteeringSystem { WheelBase = 130, CarWidth = 90 };

            Assert.Equal(165, steering.GetTurningRadius(60), 0);
        }

        [Fact]
        public void SteeringMapsInputToWheelAngle()
        {
            var steering = new SteeringSystem();

            Assert.Equal(60, steering.GetWheelAngle(100), 6);
            Assert.Equal(-30, steering.GetWheelAngle(-50), 6);
            Assert.Equal(0, steering.GetYawRate(10, 0));
        }

        [Fact]
        public void ReversingTurnsTheOtherWay()
        {
            var steering = new SteeringSystem();

            Assert.True(steering.GetYawRate(5, 20) > 0);
            Assert.True(steering.GetYawRate(-5, 20) < 0);
        }
    }
}
