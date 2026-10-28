namespace AutomatedCar.SystemComponents.Movement
{
    using System;
    using System.Collections.Generic;

    /// <summary>Merges the driver (HMI) input with the higher priority inputs of the driver assistance systems.</summary>
    public class InputResolver
    {
        public const double PedalPressRate = 50; // units per second, full press in 2 s
        public const double SteeringTurnRate = 100; // units per second
        public const double ReturnToZeroRate = 100; // units per second, full return in 1 s

        private readonly Dictionary<InputSource, DriverInput> assistInputs = new ();

        private double throttle;
        private double brake;
        private double steering;
        private bool throttleHeld;
        private bool brakeHeld;
        private bool steeringHeld;

        public GearPosition HmiGear { get; private set; } = GearPosition.P;

        /// <summary>Overrides the HMI input with absolute values.</summary>
        /// <param name="input">The new HMI input.</param>
        public void SetHmiInput(DriverInput input)
        {
            this.HmiGear = input.Gear;
            this.throttle = Math.Clamp(input.Throttle, 0, DriverInput.PedalMax);
            this.brake = Math.Clamp(input.Brake, 0, DriverInput.PedalMax);
            this.steering = Math.Clamp(input.Steering, -DriverInput.SteeringMax, DriverInput.SteeringMax);
        }

        public void SetHmiGear(GearPosition gear)
        {
            this.HmiGear = gear;
        }

        /// <summary>Sets the input of an assistance system, null releases the control.</summary>
        /// <param name="source">The assistance system.</param>
        /// <param name="input">Its input, or null.</param>
        public void SetAssistInput(InputSource source, DriverInput input)
        {
            if (source == InputSource.HMI)
            {
                this.SetHmiInput(input);
            }
            else if (input == null)
            {
                this.assistInputs.Remove(source);
            }
            else
            {
                this.assistInputs[source] = input;
            }
        }

        public void PressThrottle(double dt)
        {
            this.throttle = Math.Min(DriverInput.PedalMax, this.throttle + (PedalPressRate * dt));
            this.throttleHeld = true;
        }

        public void PressBrake(double dt)
        {
            this.brake = Math.Min(DriverInput.PedalMax, this.brake + (PedalPressRate * dt));
            this.brakeHeld = true;
        }

        /// <summary>Turns the steering wheel.</summary>
        /// <param name="direction">-1 for left, 1 for right.</param>
        /// <param name="dt">Elapsed time in seconds.</param>
        public void TurnSteering(int direction, double dt)
        {
            this.steering = Math.Clamp(this.steering + (Math.Sign(direction) * SteeringTurnRate * dt), -DriverInput.SteeringMax, DriverInput.SteeringMax);
            this.steeringHeld = true;
        }

        /// <summary>Gradually returns the pedals and the steering wheel that were not held in this cycle to their default positions.</summary>
        /// <param name="dt">Elapsed time in seconds.</param>
        public void ApplyReturnToZero(double dt)
        {
            double step = ReturnToZeroRate * dt;

            if (!this.throttleHeld)
            {
                this.throttle = MoveTowardsZero(this.throttle, step);
            }

            if (!this.brakeHeld)
            {
                this.brake = MoveTowardsZero(this.brake, step);
            }

            if (!this.steeringHeld)
            {
                this.steering = MoveTowardsZero(this.steering, step);
            }

            this.throttleHeld = false;
            this.brakeHeld = false;
            this.steeringHeld = false;
        }

        /// <summary>Resolves the final input. Brake: the strongest wins, an emergency brake cuts the throttle.
        /// Throttle: the strongest of HMI, ACC and PP. Steering: PP, then LKA, then HMI.</summary>
        /// <returns>The resolved input.</returns>
        public DriverInput Resolve()
        {
            var result = new DriverInput
            {
                Gear = this.HmiGear,
                Throttle = (int)Math.Round(this.throttle),
                Brake = (int)Math.Round(this.brake),
                Steering = (int)Math.Round(this.steering),
            };

            foreach (var source in new[] { InputSource.ACC, InputSource.PP, InputSource.AEB })
            {
                if (this.assistInputs.TryGetValue(source, out DriverInput assist))
                {
                    result.Brake = Math.Max(result.Brake, Math.Clamp(assist.Brake, 0, DriverInput.PedalMax));
                    if (source != InputSource.AEB)
                    {
                        result.Throttle = Math.Max(result.Throttle, Math.Clamp(assist.Throttle, 0, DriverInput.PedalMax));
                    }
                }
            }

            if (this.assistInputs.TryGetValue(InputSource.AEB, out DriverInput emergency) && emergency.Brake > 0)
            {
                result.Throttle = 0;
            }

            foreach (var source in new[] { InputSource.PP, InputSource.LKA })
            {
                if (this.assistInputs.TryGetValue(source, out DriverInput assist))
                {
                    result.Steering = Math.Clamp(assist.Steering, -DriverInput.SteeringMax, DriverInput.SteeringMax);
                    break;
                }
            }

            return result;
        }

        private static double MoveTowardsZero(double value, double step)
        {
            return Math.Abs(value) <= step ? 0 : value - (Math.Sign(value) * step);
        }
    }
}
