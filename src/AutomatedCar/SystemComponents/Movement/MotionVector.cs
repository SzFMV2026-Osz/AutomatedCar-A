namespace AutomatedCar.SystemComponents.Movement
{
    /// <summary>Change of the car's position (px) and orientation (degrees) within one cycle.</summary>
    public class MotionVector
    {
        public MotionVector(double deltaX, double deltaY, double deltaRotation)
        {
            this.DeltaX = deltaX;
            this.DeltaY = deltaY;
            this.DeltaRotation = deltaRotation;
        }

        public double DeltaX { get; }

        public double DeltaY { get; }

        public double DeltaRotation { get; }
    }
}
