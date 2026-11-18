namespace AutomatedCarTests.ACC
{
    using AutomatedCar.ACC;
    using Xunit;
    using static AutomatedCarTests.ACC.AccTestHelper;

    public class LeadTrackerTests
    {
        [Fact]
        public void NoCandidate_NoLead()
        {
            Assert.Null(new LeadTracker().Update(null, 10, Dt));
        }

        [Fact]
        public void ReportedSpeed_IsUsedDirectly()
        {
            var tracker = new LeadTracker();
            var obj = CarAhead(20, speedMps: 8);

            var lead = tracker.Update(new LeadCandidate(obj, 20), 10, Dt);

            Assert.Equal(8, lead.LeadSpeedMps, 9);
            Assert.Equal(-2, lead.RelativeSpeedMps, 9);
            Assert.Equal(20, lead.DistanceM, 9);
        }

        [Fact]
        public void UnknownSpeed_ComesFromDistanceChange()
        {
            var tracker = new LeadTracker();
            var source = new object();
            double distance = 30;

            LeadState lead = null;
            for (int i = 0; i < 300; i++)
            {
                lead = tracker.Update(new LeadCandidate(CarAhead(distance, source: source), distance), 10, Dt);
                distance -= 4 * Dt;
            }

            Assert.Equal(-4, lead.RelativeSpeedMps, 2);
            Assert.Equal(6, lead.LeadSpeedMps, 2);
        }

        [Fact]
        public void NewObject_StartsWithoutHistory()
        {
            var tracker = new LeadTracker();
            tracker.Update(new LeadCandidate(CarAhead(30, source: new object()), 30), 10, Dt);

            var lead = tracker.Update(new LeadCandidate(CarAhead(5, source: new object()), 5), 10, Dt);

            Assert.Equal(0, lead.RelativeSpeedMps, 9);
            Assert.Equal(10, lead.LeadSpeedMps, 9);
        }

        [Fact]
        public void LeadSpeed_NeverNegative()
        {
            var tracker = new LeadTracker(smoothing: 1);
            var source = new object();
            tracker.Update(new LeadCandidate(CarAhead(30, source: source), 30), 1, Dt);

            var lead = tracker.Update(new LeadCandidate(CarAhead(29, source: source), 29), 1, Dt);

            Assert.Equal(0, lead.LeadSpeedMps, 9);
        }
    }
}