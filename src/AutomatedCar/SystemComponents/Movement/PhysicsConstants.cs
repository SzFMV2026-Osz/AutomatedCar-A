namespace AutomatedCar.SystemComponents.Movement
{
    using System;

    /// <summary>Unit conversions and shared physical properties of the car.</summary>
    public static class PhysicsConstants
    {
        /// <summary>1 m = 50 px, as the handout suggests.</summary>
        public const double PixelsPerMeter = 50;

        public const double MetersPerSecondToKmh = 3.6;

        public const double WheelRadius = 0.33; // m

        public static double ToRadians(double degrees) => degrees * Math.PI / 180;

        public static double ToDegrees(double radians) => radians * 180 / Math.PI;
    }
}
