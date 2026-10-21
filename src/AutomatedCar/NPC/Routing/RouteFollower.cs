using System;
using AutomatedCar.NPC;

namespace AutomatedCar.NPC.Routing
{
    /// Moves an NPC along its route. Call <see cref="Step"/> once per tick.
    /// Coordinates are world pixels, the heading is in degrees (0 = up, clockwise positive),
    /// the speed is in px/s (see <see cref="Units"/>).
    /// A waypoint's speed, acceleration and wait time apply from the moment the NPC reaches it:
    /// the speed is the target speed while heading to the next waypoint.
    public sealed class RouteFollower
    {
        private const double FullCircleDegrees = 360.0;
        private const double HalfCircleDegrees = 180.0;

        private readonly NpcRoute route;
        private readonly MotionLimits limits;

        private int targetIndex;
        private double targetSpeedPxPerSec;
        private double accelerationMps2;
        private double waitRemainingSeconds;

        public RouteFollower(NpcRoute route, MotionLimits limits = null)
        {
            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            if (route.Waypoints == null || route.Waypoints.Count < 2)
            {
                throw new ArgumentException("A route needs at least two waypoints.", nameof(route));
            }

            this.route = route;
            this.limits = limits ?? MotionLimits.For(route.Kind);
            this.Reset();
        }

        public double X { get; private set; }

        public double Y { get; private set; }

        /// Gets the heading in degrees, 0 = up, clockwise positive, range [0, 360).</summary>
        public double HeadingDeg { get; private set; }

        public double SpeedPxPerSec { get; private set; }

        public double SpeedMps => Units.PxPerSecToMps(this.SpeedPxPerSec);

        public NpcState State { get; private set; }

        /// Gets the index of the waypoint the NPC is heading to.</summary>
        public int TargetWaypointIndex => this.targetIndex;

        /// Puts the NPC back to the first waypoint, standing still, and starts the route again.
        public void Reset()
        {
            Waypoint start = this.route.Waypoints[0];
            this.X = start.X;
            this.Y = start.Y;
            this.SpeedPxPerSec = 0;
            this.waitRemainingSeconds = 0;
            this.targetIndex = 1;
            this.State = NpcState.Idle;

            this.ArriveAt(0);

            // At the start the NPC is already facing the first target.
            this.HeadingDeg = this.DesiredHeadingDeg();
            this.UpdateState();
        }

        /// <summary>
        /// Advances the NPC by the given time.
        /// </summary>
        /// <param name="deltaSeconds">Elapsed time in seconds.</param>
        public void Step(double deltaSeconds)
        {
            if (double.IsNaN(deltaSeconds) || deltaSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Time step must not be negative.");
            }

            if (this.State == NpcState.Finished || deltaSeconds == 0)
            {
                return;
            }

            double remainingTime = deltaSeconds;

            if (this.waitRemainingSeconds > 0)
            {
                // Standing still, but already turning toward the next waypoint.
                double waitedTime = Math.Min(this.waitRemainingSeconds, deltaSeconds);
                this.Turn(waitedTime);
                this.waitRemainingSeconds -= waitedTime;
                remainingTime = deltaSeconds - waitedTime;
            }

            if (this.waitRemainingSeconds > 0 || remainingTime <= 0)
            {
                this.UpdateState();
                return;
            }

            this.Drive(remainingTime);
            this.UpdateState();
        }

        private static double NormalizeDegrees(double degrees)
        {
            double normalized = degrees % FullCircleDegrees;
            return normalized < 0 ? normalized + FullCircleDegrees : normalized;
        }

        /// <summary>Maps an angle difference to (-180, 180], so turning always takes the shorter way.</summary>
        private static double ShortestAngleDifference(double fromDeg, double toDeg)
        {
            double difference = NormalizeDegrees(toDeg - fromDeg);
            return difference > HalfCircleDegrees ? difference - FullCircleDegrees : difference;
        }

        private double DesiredHeadingDeg()
        {
            Waypoint target = this.route.Waypoints[this.targetIndex];
            double deltaX = target.X - this.X;
            double deltaY = target.Y - this.Y;

            if (deltaX == 0 && deltaY == 0)
            {
                return this.HeadingDeg;
            }

            // The world's y axis points down, so "up" (0 degrees) is negative y.
            return NormalizeDegrees(Math.Atan2(deltaX, -deltaY) * HalfCircleDegrees / Math.PI);
        }

        private void Turn(double seconds)
        {
            double maxTurn = this.limits.MaxTurnRateDegPerSec * seconds;
            double difference = ShortestAngleDifference(this.HeadingDeg, this.DesiredHeadingDeg());
            double turn = Math.Max(-maxTurn, Math.Min(maxTurn, difference));
            this.HeadingDeg = NormalizeDegrees(this.HeadingDeg + turn);
        }

        private void Drive(double seconds)
        {
            this.Turn(seconds);

            double previousSpeed = this.SpeedPxPerSec;
            double speedChange = Units.MpsToPxPerSec(this.accelerationMps2) * seconds;

            if (previousSpeed < this.targetSpeedPxPerSec)
            {
                this.SpeedPxPerSec = Math.Min(this.targetSpeedPxPerSec, previousSpeed + speedChange);
            }
            else if (previousSpeed > this.targetSpeedPxPerSec)
            {
                this.SpeedPxPerSec = Math.Max(this.targetSpeedPxPerSec, previousSpeed - speedChange);
            }

            double averageSpeed = (previousSpeed + this.SpeedPxPerSec) / 2.0;
            this.MoveAlongHeading(averageSpeed * seconds);
        }

        private void MoveAlongHeading(double distance)
        {
            // Guards against zero-length routes where waypoints sit on top of each other.
            int remainingSwitches = this.route.Waypoints.Count * 2;

            while (distance > 0 && this.State != NpcState.Finished && this.waitRemainingSeconds <= 0)
            {
                Waypoint target = this.route.Waypoints[this.targetIndex];
                double distanceToTarget = Math.Sqrt(Math.Pow(target.X - this.X, 2) + Math.Pow(target.Y - this.Y, 2));

                if (distanceToTarget <= distance || distanceToTarget <= this.limits.ArrivalTolerancePx)
                {
                    if (remainingSwitches-- <= 0)
                    {
                        return;
                    }

                    // Waypoint reached: the leftover distance is continued on the next leg.
                    this.X = target.X;
                    this.Y = target.Y;
                    distance = Math.Max(0, distance - distanceToTarget);
                    this.ArriveAt(this.targetIndex);
                    continue;
                }

                double headingRadians = this.HeadingDeg * Math.PI / HalfCircleDegrees;
                this.X += Math.Sin(headingRadians) * distance;
                this.Y -= Math.Cos(headingRadians) * distance;
                distance = 0;
            }
        }

        private void ArriveAt(int waypointIndex)
        {
            Waypoint waypoint = this.route.Waypoints[waypointIndex];
            bool isLast = waypointIndex == this.route.Waypoints.Count - 1;

            this.targetSpeedPxPerSec = Units.KmhToPxPerSec(waypoint.SpeedKmh);
            this.accelerationMps2 = waypoint.AccelMps2 ?? this.limits.DefaultAccelMps2;

            if (isLast && !this.route.Loop)
            {
                this.SpeedPxPerSec = 0;
                this.waitRemainingSeconds = 0;
                this.State = NpcState.Finished;
                return;
            }

            this.targetIndex = isLast ? 0 : waypointIndex + 1;

            if (waypoint.WaitSeconds > 0)
            {
                this.SpeedPxPerSec = 0;
                this.waitRemainingSeconds = waypoint.WaitSeconds;
            }
        }

        private void UpdateState()
        {
            if (this.State == NpcState.Finished)
            {
                return;
            }

            if (this.waitRemainingSeconds > 0)
            {
                this.State = NpcState.Waiting;
            }
            else if (this.SpeedPxPerSec > this.targetSpeedPxPerSec)
            {
                this.State = NpcState.Braking;
            }
            else if (this.SpeedPxPerSec == 0 && this.targetSpeedPxPerSec == 0)
            {
                this.State = NpcState.Idle;
            }
            else
            {
                this.State = NpcState.Moving;
            }
        }
    }
}