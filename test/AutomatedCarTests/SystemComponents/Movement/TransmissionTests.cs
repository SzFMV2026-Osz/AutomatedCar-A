using AutomatedCar.SystemComponents.Movement;
using Xunit;

namespace AutomatedCarTests.SystemComponents.Movement
{
    public class TransmissionTests
    {
        [Fact]
        public void StartsInPark()
        {
            var transmission = new Transmission();

            Assert.Equal(GearPosition.P, transmission.Position);
            Assert.False(transmission.IsDriveEnabled());
            Assert.Equal(0, transmission.GetRatio());
        }

        [Fact]
        public void ShiftsSequentiallyUpAndDown()
        {
            var transmission = new Transmission();

            transmission.SelectNextPosition();
            Assert.Equal(GearPosition.R, transmission.Position);
            transmission.SelectNextPosition();
            Assert.Equal(GearPosition.N, transmission.Position);
            transmission.SelectNextPosition();
            Assert.Equal(GearPosition.D, transmission.Position);
            transmission.SelectNextPosition();
            Assert.Equal(GearPosition.D, transmission.Position);

            transmission.SelectPreviousPosition();
            Assert.Equal(GearPosition.N, transmission.Position);
            transmission.SelectPreviousPosition();
            transmission.SelectPreviousPosition();
            transmission.SelectPreviousPosition();
            Assert.Equal(GearPosition.P, transmission.Position);
        }

        [Fact]
        public void ReverseHasNegativeRatio()
        {
            var transmission = new Transmission();
            transmission.SelectNextPosition();

            Assert.True(transmission.IsDriveEnabled());
            Assert.True(transmission.GetRatio() < 0);
        }

        [Fact]
        public void ShiftsInternalGearsByRpm()
        {
            var transmission = ShiftToDrive();
            Assert.Equal(1, transmission.CurrentGear);
            double firstGearRatio = transmission.GetRatio();

            transmission.UpdateGear(transmission.UpshiftRpm + 1, 10);
            Assert.Equal(2, transmission.CurrentGear);
            Assert.True(transmission.GetRatio() < firstGearRatio);

            transmission.UpdateGear(transmission.DownshiftRpm - 1, 5);
            Assert.Equal(1, transmission.CurrentGear);
        }

        [Fact]
        public void DoesNotShiftAboveTopGear()
        {
            var transmission = ShiftToDrive();

            for (int i = 0; i < 10; i++)
            {
                transmission.UpdateGear(transmission.UpshiftRpm + 1, 50);
            }

            Assert.Equal(transmission.GearCount, transmission.CurrentGear);
        }

        private static Transmission ShiftToDrive()
        {
            var transmission = new Transmission();
            transmission.SelectNextPosition();
            transmission.SelectNextPosition();
            transmission.SelectNextPosition();
            return transmission;
        }
    }
}
