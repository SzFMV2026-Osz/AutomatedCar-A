using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar.ACC
{
    public sealed class AccSettings
    {
        public const int MinSetSpeedKmh = 30;
        public const int MaxSetSpeedKmh = 160;
        public const int SetSpeedStepKmh = 10;

        private int timeGapIndex;

        public AccSettings()
        {
            this.SetSpeedKmh = MinSetSpeedKmh;
            this.timeGapIndex = 1;
        }

        public static IReadOnlyList<double> TimeGaps { get; } = new[] { 0.8, 1.0, 1.2, 1.4 };

        public int SetSpeedKmh { get; private set; }

        public double TimeGapSeconds => TimeGaps[this.timeGapIndex];

        public void InitFromCurrentSpeed(double currentSpeedKmh)
        {
            this.SetSpeedKmh = Clamp((int)Math.Round(currentSpeedKmh, MidpointRounding.AwayFromZero));
        }

        // 47 -> 50, 50 -> 60
        public void Increase()
        {
            int next = ((this.SetSpeedKmh / SetSpeedStepKmh) + 1) * SetSpeedStepKmh;
            this.SetSpeedKmh = Clamp(next);
        }

        // 47 -> 40, 50 -> 40
        public void Decrease()
        {
            int previous = this.SetSpeedKmh % SetSpeedStepKmh == 0
                ? this.SetSpeedKmh - SetSpeedStepKmh
                : (this.SetSpeedKmh / SetSpeedStepKmh) * SetSpeedStepKmh;
            this.SetSpeedKmh = Clamp(previous);
        }

        public void CycleTimeGap()
        {
            this.timeGapIndex = (this.timeGapIndex + 1) % TimeGaps.Count;
        }

        private static int Clamp(int speedKmh) => Math.Clamp(speedKmh, MinSetSpeedKmh, MaxSetSpeedKmh);
    }
}
