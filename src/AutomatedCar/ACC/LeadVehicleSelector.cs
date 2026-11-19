namespace AutomatedCar.ACC
{
    using System;
    using System.Collections.Generic;
    using AutomatedCar.Models;

    public sealed class LeadVehicleSelector
    {
        private const double PxPerMeter = 50.0;

        private readonly double corridorHalfWidthPx;
        private readonly double maxRangePx;

        public LeadVehicleSelector(double corridorHalfWidthM = 1.5, double maxRangeM = 200)
        {
            this.corridorHalfWidthPx = corridorHalfWidthM * PxPerMeter;
            this.maxRangePx = maxRangeM * PxPerMeter;
        }

        public LeadCandidate Select(EgoPose ego, IEnumerable<TrackedObject> objects)
        {
            ArgumentNullException.ThrowIfNull(ego);
            if (objects == null)
            {
                return null;
            }

            LeadCandidate best = null;
            foreach (var obj in objects)
            {
                if (obj == null || !obj.Collideable || obj.Type != WorldObjectType.Car)
                {
                    continue;
                }

                double? distancePx = this.DistanceInCorridor(ego, obj);
                if (distancePx is double d && d <= this.maxRangePx && (best == null || d / PxPerMeter < best.DistanceM))
                {
                    best = new LeadCandidate(obj, d / PxPerMeter);
                }
            }

            return best;
        }

        // nearest point that is ahead of our front bumper and inside our lane
        private double? DistanceInCorridor(EgoPose ego, TrackedObject obj)
        {
            IReadOnlyList<Vec2> points = obj.WorldPolygon != null && obj.WorldPolygon.Count > 0
                ? obj.WorldPolygon
                : new[] { obj.Position };

            double? nearest = null;
            foreach (var point in points)
            {
                var local = AccGeometry.ToCarFrame(ego, point);
                double ahead = local.ForwardPx - ego.FrontOffsetPx;
                if (ahead > 0 && Math.Abs(local.LateralPx) <= this.corridorHalfWidthPx && (nearest == null || ahead < nearest))
                {
                    nearest = ahead;
                }
            }

            return nearest;
        }
    }
}