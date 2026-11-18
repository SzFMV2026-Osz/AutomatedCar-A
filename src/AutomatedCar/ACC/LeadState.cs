namespace AutomatedCar.ACC
{
    public sealed record LeadState(object Source, double DistanceM, double LeadSpeedMps, double RelativeSpeedMps);
}