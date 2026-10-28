namespace AutomatedCar.Npc.Routing
{
    using AutomatedCar.NPC;
    using AutomatedCar.NPC.Routing;
    using System;

    /// <summary>
    /// Moves a point along an <see cref="NpcRoute"/>: position, speed (with acceleration limits),
    /// heading (with a turn rate limit), looping and waiting. Pure logic without UI dependencies.
    /// </summary>
    public sealed class RouteFollower
    {
        private const double Epsilon = 1e-9;

        private readonly NpcRoute route;
        private readonly MotionLimits limits;
        private int targetIndex;
        private double targetSpeed;
        private double currentAccelMps2;
        private double waitLeft;

        // Initializes a new instance of the "RouteFollower"
        // Route -> The route to follow (at least 2 waypoints).
        // Limits -> Movement limits; null means the defaults for the route's NPC kind.
        public RouteFollower(NpcRoute route, MotionLimits limits = null)
        {
            this.route = route ?? throw new ArgumentNullException(nameof(route));
            if (route.Waypoints == null || route.Waypoints.Count < 2)
            {
                throw new ArgumentException($"Route '{route.NpcId}' needs at least 2 waypoints.", nameof(route));
            }

            this.limits = limits ?? MotionLimits.For(route.Kind);
            this.Reset();
        }

        /// <summary>Gets the X coordinate in world px.</summary>
        public double PosX { get; private set; }

        /// <summary>Gets the Y coordinate in world px.</summary>
        public double PosY { get; private set; }

        /// <summary>Gets the heading in degrees in [0, 360); 0° = up, clockwise positive.</summary>
        public double HeadingDeg { get; private set; }

        /// <summary>Gets the current speed in px/s.</summary>
        public double SpeedPxPerSec { get; private set; }

        /// <summary>Gets the current target speed in px/s.</summary>
        public double TargetSpeedPxPerSec => this.targetSpeed;

        /// <summary>Gets the index of the waypoint the follower is heading to.</summary>
        public int TargetIndex => this.targetIndex;

        /// <summary>Gets a value indicating whether a non-looping route has been completed.</summary>
        public bool IsFinished { get; private set; }

        /// <summary>Gets a value indicating whether the follower is waiting at a waypoint.</summary>
        public bool IsWaiting => this.waitLeft > 0;

        /// <summary>Gets the movement state.</summary>
        public NpcState State { get; private set; }

        /// <summary>Moves the follower back to the first waypoint with zero speed.</summary>
        public void Reset()
        {
            var first = this.route.Waypoints[0];
            var second = this.route.Waypoints[1];

            this.PosX = first.X;
            this.PosY = first.Y;
            this.SpeedPxPerSec = 0;
            this.targetIndex = 1;
            this.targetSpeed = Units.KmhToPxPerSec(first.SpeedKmh);
            this.currentAccelMps2 = first.AccelMps2 ?? this.limits.DefaultAccelMps2;
            this.waitLeft = first.WaitSeconds;
            this.IsFinished = false;
            this.HeadingDeg = HeadingOf(second.X - first.X, second.Y - first.Y) ?? 0;
            this.State = this.IsWaiting ? NpcState.Waiting : NpcState.Idle;
        }

        /// <summary>Advances the follower by the given time.</summary>
        /// <param name="deltaSeconds">Elapsed time in seconds.</param>
        public void Step(double deltaSeconds)
        {
            if (this.IsFinished || deltaSeconds <= 0)
            {
                return;
            }

            if (this.waitLeft > 0)
            {
                // stand still, but already turn towards the next waypoint (e.g. a pedestrian turning around)
                this.waitLeft = Math.Max(0, this.waitLeft - deltaSeconds);
                this.UpdateHeading(deltaSeconds);
                this.State = NpcState.Waiting;
                return;
            }

            this.UpdateSpeed(deltaSeconds);
            this.Advance(this.SpeedPxPerSec * deltaSeconds);
            if (!this.IsFinished)
            {
                this.UpdateHeading(deltaSeconds);
            }

            this.UpdateState();
        }

        /// <summary>Normalizes an angle to [0, 360).</summary>
        /// <param name="degrees">Angle in degrees.</param>
        /// <returns>The normalized angle.</returns>
        public static double NormalizeDeg(double degrees)
        {
            double result = degrees % 360;
            return result < 0 ? result + 360 : result;
        }

        /// <summary>Heading of a direction vector in screen coordinates (y down): 0° = up, 90° = right.</summary>
        /// <param name="dx">X component.</param>
        /// <param name="dy">Y component.</param>
        /// <returns>The heading in [0, 360), or null for a zero-length vector.</returns>
        public static double? HeadingOf(double dx, double dy)
        {
            if ((dx * dx) + (dy * dy) < Epsilon)
            {
                return null;
            }

            return NormalizeDeg(Math.Atan2(dx, -dy) * 180.0 / Math.PI);
        }

        private static double MoveTowards(double current, double target, double maxDelta)
        {
            return Math.Abs(target - current) <= maxDelta
                ? target
                : current + (Math.Sign(target - current) * maxDelta);
        }

        private void UpdateSpeed(double deltaSeconds)
        {
            double maxDelta = Units.MpsToPxPerSec(this.currentAccelMps2) * deltaSeconds;
            this.SpeedPxPerSec = MoveTowards(this.SpeedPxPerSec, this.targetSpeed, maxDelta);
        }

        private void Advance(double distance)
        {
            // guard against routes made of identical points
            int safety = (this.route.Waypoints.Count * 2) + 2;

            while (distance > 0 && !this.IsFinished && safety-- > 0)
            {
                var waypoint = this.route.Waypoints[this.targetIndex];
                double dx = waypoint.X - this.PosX;
                double dy = waypoint.Y - this.PosY;
                double length = Math.Sqrt((dx * dx) + (dy * dy));

                if (length <= distance || length < this.limits.ArrivalTolerancePx)
                {
                    this.PosX = waypoint.X;
                    this.PosY = waypoint.Y;
                    distance -= length;
                    this.OnWaypointReached(waypoint);

                    if (this.waitLeft > 0)
                    {
                        break;
                    }
                }
                else
                {
                    this.PosX += dx / length * distance;
                    this.PosY += dy / length * distance;
                    distance = 0;
                }
            }
        }

        private void OnWaypointReached(Waypoint waypoint)
        {
            this.targetSpeed = Units.KmhToPxPerSec(waypoint.SpeedKmh);
            this.currentAccelMps2 = waypoint.AccelMps2 ?? this.limits.DefaultAccelMps2;
            this.waitLeft = waypoint.WaitSeconds;

            if (this.targetIndex < this.route.Waypoints.Count - 1)
            {
                this.targetIndex++;
            }
            else if (this.route.Loop)
            {
                this.targetIndex = 0;
            }
            else
            {
                this.IsFinished = true;
                this.SpeedPxPerSec = 0;
                this.waitLeft = 0;
            }
        }

        private void UpdateHeading(double deltaSeconds)
        {
            var target = this.route.Waypoints[this.targetIndex];
            double? targetHeading = HeadingOf(target.X - this.PosX, target.Y - this.PosY);
            if (targetHeading == null)
            {
                return;
            }

            // signed difference in [-180, 180): always turn the shorter way
            double diff = NormalizeDeg(targetHeading.Value - this.HeadingDeg + 180) - 180;
            double maxTurn = this.limits.MaxTurnRateDegPerSec * deltaSeconds;
            this.HeadingDeg = NormalizeDeg(this.HeadingDeg + Math.Clamp(diff, -maxTurn, maxTurn));
        }

        private void UpdateState()
        {
            if (this.IsFinished)
            {
                this.State = NpcState.Finished;
            }
            else if (this.waitLeft > 0)
            {
                this.State = NpcState.Waiting;
            }
            else if (this.SpeedPxPerSec > this.targetSpeed + Epsilon)
            {
                this.State = NpcState.Braking;
            }
            else if (this.SpeedPxPerSec > Epsilon)
            {
                this.State = NpcState.Moving;
            }
            else
            {
                this.State = NpcState.Idle;
            }
        }
    }
}