namespace AutomatedCarTests.ACC
{
    using AutomatedCar.ACC;
    using Xunit;

    public class AccGeometryTests
    {
        [Fact]
        public void ToCarFrame_FacingUp_PointAboveIsAhead()
        {
            var ego = new EgoPose(100, 100, 0, 120);

            var local = AccGeometry.ToCarFrame(ego, new Vec2(130, 0));

            Assert.Equal(100, local.ForwardPx, 6);
            Assert.Equal(30, local.LateralPx, 6);
        }

        [Fact]
        public void ToCarFrame_FacingRight_PointBelowIsOnTheRight()
        {
            var ego = new EgoPose(0, 0, 90, 120);

            var local = AccGeometry.ToCarFrame(ego, new Vec2(200, 40));

            Assert.Equal(200, local.ForwardPx, 6);
            Assert.Equal(40, local.LateralPx, 6);
        }

        [Fact]
        public void ToCarFrame_PointBehindIsNegative()
        {
            var ego = new EgoPose(0, 0, 180, 120);

            var local = AccGeometry.ToCarFrame(ego, new Vec2(0, -50));

            Assert.Equal(-50, local.ForwardPx, 6);
        }

        [Fact]
        public void ToWorld_NoRotation_ShiftsByRotationPoint()
        {
            var world = AccGeometry.ToWorld(new Vec2(54, 0), new Vec2(54, 120), new Vec2(1000, 2000), 0);

            Assert.Equal(1000, world.X, 6);
            Assert.Equal(1880, world.Y, 6);
        }

        [Fact]
        public void ToWorld_Rotated90_FrontPointsRight()
        {
            // the front of a car image (top middle) ends up to the right of the rotation point when facing right
            var world = AccGeometry.ToWorld(new Vec2(54, 0), new Vec2(54, 120), new Vec2(1000, 2000), 90);

            Assert.Equal(1120, world.X, 6);
            Assert.Equal(2000, world.Y, 6);
        }

        [Fact]
        public void ToWorld_And_ToCarFrame_Agree()
        {
            var ego = new EgoPose(500, 500, 37, 120);
            var front = AccGeometry.ToWorld(new Vec2(54, 0), new Vec2(54, 120), new Vec2(500, 500), 37);

            var local = AccGeometry.ToCarFrame(ego, front);

            Assert.Equal(120, local.ForwardPx, 6);
            Assert.Equal(0, local.LateralPx, 6);
        }
    }
}