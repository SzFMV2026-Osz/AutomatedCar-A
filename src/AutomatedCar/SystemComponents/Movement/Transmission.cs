namespace AutomatedCar.SystemComponents.Movement
{
    using System;

    public class Transmission
    {
        public const double FinalDriveRatio = 4.1;
        public const double ReverseGearRatio = 3.6;

        private readonly double[] gearRatios = { 3.6, 2.2, 1.5, 1.1, 0.85 };

        public GearPosition Position { get; private set; } = GearPosition.P;

        /// <summary>Gets the internal gear (1..5) while in D, 0 otherwise.</summary>
        public int CurrentGear { get; private set; }

        public double UpshiftRpm { get; set; } = 4500;

        public double DownshiftRpm { get; set; } = 1800;

        public int GearCount => this.gearRatios.Length;

        public void SelectNextPosition()
        {
            if (this.Position < GearPosition.D)
            {
                this.SetPosition(this.Position + 1);
            }
        }

        public void SelectPreviousPosition()
        {
            if (this.Position > GearPosition.P)
            {
                this.SetPosition(this.Position - 1);
            }
        }

        /// <summary>Shifts the internal gears in D based on the motor rpm.</summary>
        /// <param name="rpm">Current motor rpm.</param>
        /// <param name="speed">Current speed of the car in m/s.</param>
        public void UpdateGear(double rpm, double speed)
        {
            if (this.Position != GearPosition.D)
            {
                return;
            }

            if (Math.Abs(speed) < 0.1)
            {
                this.CurrentGear = 1;
            }
            else if (rpm > this.UpshiftRpm && this.CurrentGear < this.GearCount)
            {
                this.CurrentGear++;
            }
            else if (rpm < this.DownshiftRpm && this.CurrentGear > 1)
            {
                this.CurrentGear--;
            }
        }

        /// <summary>Gets the overall ratio between the motor and the wheels, negative in reverse, 0 when there is no drive.</summary>
        /// <returns>The overall gear ratio.</returns>
        public double GetRatio()
        {
            return this.Position switch
            {
                GearPosition.D => this.gearRatios[this.CurrentGear - 1] * FinalDriveRatio,
                GearPosition.R => -ReverseGearRatio * FinalDriveRatio,
                _ => 0,
            };
        }

        public bool IsDriveEnabled()
        {
            return this.Position == GearPosition.D || this.Position == GearPosition.R;
        }

        private void SetPosition(GearPosition position)
        {
            this.Position = position;
            this.CurrentGear = position == GearPosition.D ? 1 : 0;
        }
    }
}
