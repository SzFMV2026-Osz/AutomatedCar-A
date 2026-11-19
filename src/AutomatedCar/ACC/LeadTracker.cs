namespace AutomatedCar.ACC
{
    using System;

    public sealed class LeadTracker
    {
        private readonly double smoothing;
        private object source;
        private double lastDistanceM;
        private double relativeSpeedMps;

        public LeadTracker(double smoothing = 0.2)
        {
            if (smoothing <= 0 || smoothing > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(smoothing), "Smoothing must be in (0, 1].");
            }

            this.smoothing = smoothing;
        }

        public LeadState Update(LeadCandidate candidate, double egoSpeedMps, double deltaSeconds)
        {
            if (candidate == null)
            {
                this.Reset();
                return null;
            }

            if (candidate.Object.SpeedMps is double reported)
            {
                this.Remember(candidate);
                this.relativeSpeedMps = reported - egoSpeedMps;
                return new LeadState(candidate.Object.Source, candidate.DistanceM, reported, this.relativeSpeedMps);
            }

            if (!ReferenceEquals(this.source, candidate.Object.Source) || deltaSeconds <= 0)
            {
                this.Remember(candidate);
                this.relativeSpeedMps = 0;
            }
            else
            {
                double measured = (candidate.DistanceM - this.lastDistanceM) / deltaSeconds;
                this.relativeSpeedMps += this.smoothing * (measured - this.relativeSpeedMps);
                this.lastDistanceM = candidate.DistanceM;
            }

            double leadSpeed = Math.Max(0, egoSpeedMps + this.relativeSpeedMps);
            return new LeadState(candidate.Object.Source, candidate.DistanceM, leadSpeed, leadSpeed - egoSpeedMps);
        }

        public void Reset()
        {
            this.source = null;
            this.lastDistanceM = 0;
            this.relativeSpeedMps = 0;
        }

        private void Remember(LeadCandidate candidate)
        {
            this.source = candidate.Object.Source;
            this.lastDistanceM = candidate.DistanceM;
        }
    }
}