using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar.ACC
{
    public sealed record AccInputs(
       EgoPose Ego,
       double EgoSpeedMps,
       bool InDrive,
       bool DriverBrake,
       bool DriverThrottle,
       bool AebActive,
       double? SpeedLimitKmh,
       IReadOnlyList<TrackedObject> Objects,
       VehicleModel Vehicle);
}
