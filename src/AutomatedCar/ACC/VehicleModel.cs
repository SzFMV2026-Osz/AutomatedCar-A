using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar.ACC
{
    public sealed record VehicleModel(
        double MassKg,
        double RollingResistanceN,
        double AirDrag,
        double BrakeForceMaxN,
        double MaxDriveForceN)
    {
        // same as the Hajtaslanc defaults, first gear
        public static VehicleModel Default { get; } = new VehicleModel(1500, 600, 0.4, 13500, 200 * 3.6 * 4.1 / 0.33);
    }
}
