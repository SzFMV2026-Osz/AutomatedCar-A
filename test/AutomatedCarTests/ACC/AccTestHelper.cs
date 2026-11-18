namespace AutomatedCarTests.ACC
{
    using System.Collections.Generic;
    using AutomatedCar.ACC;
    using AutomatedCar.Models;
    using AutomatedCar.SystemComponents.Movement;

    public static class AccTestHelper
    {
        public const double PxPerMeter = 50.0;
        public const double FrontOffsetPx = 120.0;
        public const double Dt = 1.0 / 60;

        // our car at (0, 0), facing up
        public static EgoPose Ego { get; } = new EgoPose(0, 0, 0, FrontOffsetPx);

        // a 2 m wide, 5 m long car, its rear bumper gapM ahead of our front bumper
        public static TrackedObject CarAhead(double gapM, double lateralM = 0, double? speedMps = null, WorldObjectType type = WorldObjectType.Car, bool collideable = true, object source = null)
        {
            double rearY = -(FrontOffsetPx + (gapM * PxPerMeter));
            double frontY = rearY - (5 * PxPerMeter);
            double cx = lateralM * PxPerMeter;
            var polygon = new List<Vec2>
            {
                new Vec2(cx - 50, rearY),
                new Vec2(cx + 50, rearY),
                new Vec2(cx + 50, frontY),
                new Vec2(cx - 50, frontY),
            };

            return new TrackedObject(source ?? new object(), type, collideable, polygon, new Vec2(cx, rearY - 125), speedMps);
        }

        // in D, no pedals, no AEB, no speed limit
        public static AccInputs Inputs(double speedMps, params TrackedObject[] objects) =>
            new AccInputs(Ego, speedMps, true, false, false, false, null, objects, VehicleModel.Default);
    }

    public sealed class FakeAccInputSource : IAccInputSource
    {
        public AccInputs Current { get; set; } = AccTestHelper.Inputs(0);

        public AccInputs Read() => this.Current;
    }

    public sealed class FakeAccCommandSource : IAccCommandSource
    {
        private readonly Queue<AccCommands> queue = new Queue<AccCommands>();

        public void Press(AccCommands commands) => this.queue.Enqueue(commands);

        public AccCommands Poll() => this.queue.Count > 0 ? this.queue.Dequeue() : AccCommands.None;
    }

    public sealed class FakePowertrain
    {
        public DriverInput Last { get; private set; }

        public int Calls { get; private set; }

        public void Send(DriverInput input)
        {
            this.Last = input;
            this.Calls++;
        }
    }
}