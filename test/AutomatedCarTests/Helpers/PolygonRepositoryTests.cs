namespace AutomatedCarTests.Helpers
{
    using AutomatedCar.Helpers;
    using Xunit;

    public class PolygonRepositoryTests
    {
        [Fact]
        public void GetRawPoints_Car3Black_Returns39Points()
        {
            var polygons = PolygonRepository.GetRawPoints("car_3_black");

            Assert.Single(polygons);
            Assert.Equal(39, polygons[0].Count);
        }

        [Fact]
        public void GetRawPoints_AcceptsPngSuffix()
        {
            Assert.Equal(
                PolygonRepository.GetRawPoints("car_3_black")[0],
                PolygonRepository.GetRawPoints("car_3_black.png")[0]);
        }

        [Fact]
        public void GetRawPoints_UnknownType_ReturnsEmpty()
        {
            Assert.Empty(PolygonRepository.GetRawPoints("no_such_object"));
            Assert.Empty(PolygonRepository.GetRawPoints(null));
        }

        [Fact]
        public void GetRawPoints_ReturnsCopy()
        {
            var first = PolygonRepository.GetRawPoints("car_3_black");
            first[0].Clear();

            var second = PolygonRepository.GetRawPoints("car_3_black");

            Assert.Equal(39, second[0].Count);
        }

        [Fact]
        public void GetRawPoints_RoadHasLaneLines()
        {
            Assert.Equal(3, PolygonRepository.GetRawPoints("road_2lane_straight").Count);
        }

        [Theory]
        [InlineData("car_3_black", 60, 145)]
        [InlineData("car_1_white.png", 54, 120)]
        [InlineData("man", 19, 36)]
        public void TryGetReferencePoint_KnownTypes(string type, int x, int y)
        {
            Assert.True(PolygonRepository.TryGetReferencePoint(type, out var point));
            Assert.Equal(new System.Drawing.Point(x, y), point);
        }

        [Fact]
        public void TryGetReferencePoint_UnknownType_ReturnsFalse()
        {
            Assert.False(PolygonRepository.TryGetReferencePoint("tree", out _));
        }
    }
}