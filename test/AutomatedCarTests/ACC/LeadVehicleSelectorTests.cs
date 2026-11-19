namespace AutomatedCarTests.ACC
{
    using AutomatedCar.ACC;
    using AutomatedCar.Models;
    using Xunit;
    using static AutomatedCarTests.ACC.AccTestHelper;

    public class LeadVehicleSelectorTests
    {
        private readonly LeadVehicleSelector selector = new LeadVehicleSelector();

        [Fact]
        public void NoObjects_NoLead()
        {
            Assert.Null(this.selector.Select(Ego, new TrackedObject[0]));
            Assert.Null(this.selector.Select(Ego, null));
        }

        [Fact]
        public void CarAheadInLane_IsSelectedWithBumperDistance()
        {
            var lead = this.selector.Select(Ego, new[] { CarAhead(20) });

            Assert.NotNull(lead);
            Assert.Equal(20, lead.DistanceM, 6);
        }

        [Fact]
        public void NearestCarWins()
        {
            var near = CarAhead(15);
            var far = CarAhead(40);

            var lead = this.selector.Select(Ego, new[] { far, near });

            Assert.Same(near, lead.Object);
        }

        [Fact]
        public void CarInNeighbourLane_IsIgnored()
        {
            // centre 3.5 m to the right: its left edge is 2.5 m away, outside the 1.5 m corridor
            Assert.Null(this.selector.Select(Ego, new[] { CarAhead(20, lateralM: 3.5) }));
        }

        [Fact]
        public void CarPartlyInLane_IsSelected()
        {
            // centre 2 m to the right: its left edge (1 m) reaches into our corridor
            Assert.NotNull(this.selector.Select(Ego, new[] { CarAhead(20, lateralM: 2.0) }));
        }

        [Fact]
        public void CarBehind_IsIgnored()
        {
            Assert.Null(this.selector.Select(Ego, new[] { CarAhead(-10) }));
        }

        [Fact]
        public void NonCarOrNonCollideable_IsIgnored()
        {
            var tree = CarAhead(10, type: WorldObjectType.Tree);
            var road = CarAhead(10, type: WorldObjectType.Road, collideable: false);
            var ghost = CarAhead(10, collideable: false);

            Assert.Null(this.selector.Select(Ego, new[] { tree, road, ghost }));
        }

        [Fact]
        public void BeyondRange_IsIgnored()
        {
            Assert.Null(this.selector.Select(Ego, new[] { CarAhead(250) }));
        }

        [Fact]
        public void WithoutPolygon_UsesPosition()
        {
            var obj = new TrackedObject(new object(), WorldObjectType.Car, true, new Vec2[0], new Vec2(0, -(FrontOffsetPx + 500)), null);

            var lead = this.selector.Select(Ego, new[] { obj });

            Assert.Equal(10, lead.DistanceM, 6);
        }
    }
}