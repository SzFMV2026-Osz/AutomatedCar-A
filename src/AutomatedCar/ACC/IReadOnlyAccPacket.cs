using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar.ACC
{
    public interface IReadOnlyAccPacket
    {
        AccState State { get; }

        bool IsControlling { get; }

        AccDeactivationReason DeactivationReason { get; }

        int SetSpeedKmh { get; }

        double TimeGapSeconds { get; }

        double TargetSpeedKmh { get; }

        bool HasLead { get; }

        double LeadDistanceM { get; }

        double LeadSpeedKmh { get; }

        double DesiredGapM { get; }

        double AccelerationRequestMps2 { get; }

        // 0-100, the same scale as DriverInput
        int ThrottleRequest { get; }

        int BrakeRequest { get; }
    }
}
