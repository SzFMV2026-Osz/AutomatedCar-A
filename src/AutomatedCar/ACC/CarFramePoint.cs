namespace AutomatedCar.ACC
{
    // ForwardPx: ahead of our rotation point, LateralPx: to the right
    public readonly record struct CarFramePoint(double ForwardPx, double LateralPx);
}