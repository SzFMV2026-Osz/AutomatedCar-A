using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar.ACC
{
    public class AccPacket : ReactiveObject, IReadOnlyAccPacket
    {
        private AccState state;
        private AccDeactivationReason deactivationReason;
        private int setSpeedKmh;
        private double timeGapSeconds;
        private double targetSpeedKmh;
        private bool hasLead;
        private double leadDistanceM;
        private double leadSpeedKmh;
        private double desiredGapM;
        private double accelerationRequestMps2;
        private int throttleRequest;
        private int brakeRequest;

        public AccState State
        {
            get => this.state;
            set
            {
                this.RaiseAndSetIfChanged(ref this.state, value);
                this.RaisePropertyChanged(nameof(this.IsControlling));
            }
        }

        public bool IsControlling => this.state == AccState.Active;

        public AccDeactivationReason DeactivationReason
        {
            get => this.deactivationReason;
            set => this.RaiseAndSetIfChanged(ref this.deactivationReason, value);
        }

        public int SetSpeedKmh
        {
            get => this.setSpeedKmh;
            set => this.RaiseAndSetIfChanged(ref this.setSpeedKmh, value);
        }

        public double TimeGapSeconds
        {
            get => this.timeGapSeconds;
            set => this.RaiseAndSetIfChanged(ref this.timeGapSeconds, value);
        }

        public double TargetSpeedKmh
        {
            get => this.targetSpeedKmh;
            set => this.RaiseAndSetIfChanged(ref this.targetSpeedKmh, value);
        }

        public bool HasLead
        {
            get => this.hasLead;
            set => this.RaiseAndSetIfChanged(ref this.hasLead, value);
        }

        public double LeadDistanceM
        {
            get => this.leadDistanceM;
            set => this.RaiseAndSetIfChanged(ref this.leadDistanceM, value);
        }

        public double LeadSpeedKmh
        {
            get => this.leadSpeedKmh;
            set => this.RaiseAndSetIfChanged(ref this.leadSpeedKmh, value);
        }

        public double DesiredGapM
        {
            get => this.desiredGapM;
            set => this.RaiseAndSetIfChanged(ref this.desiredGapM, value);
        }

        public double AccelerationRequestMps2
        {
            get => this.accelerationRequestMps2;
            set => this.RaiseAndSetIfChanged(ref this.accelerationRequestMps2, value);
        }

        public int ThrottleRequest
        {
            get => this.throttleRequest;
            set => this.RaiseAndSetIfChanged(ref this.throttleRequest, value);
        }

        public int BrakeRequest
        {
            get => this.brakeRequest;
            set => this.RaiseAndSetIfChanged(ref this.brakeRequest, value);
        }
    }
}
