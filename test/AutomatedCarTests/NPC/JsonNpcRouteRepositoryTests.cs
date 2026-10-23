namespace AutomatedCarTests.Npc
{
    using AutomatedCar.Npc;
    using AutomatedCar.Npc.Routing;
    using AutomatedCar.NPC;
    using AutomatedCar.NPC.Routing;
    using System;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class JsonNpcRouteRepositoryTests
    {
        private const string ValidJson = @"{
  ""map"": ""test"",
  ""npcs"": [
    {
      ""id"": ""car1"", ""kind"": ""Car"", ""sprite"": ""car_3_black.png"", ""loop"": false,
      ""waypoints"": [
        { ""x"": 10, ""y"": 20, ""speed"": 50, ""accel"": 3 },
        { ""x"": 10, ""y"": 0, ""speed"": 30 },
        { ""x"": 50, ""y"": 0, ""speed"": 0, ""accel"": 9 }
      ]
    },
    {
      ""id"": ""ped1"", ""kind"": ""pedestrian"", ""sprite"": ""man.png"",
      ""waypoints"": [
        { ""x"": 0, ""y"": 0, ""speed"": 5 },
        { ""x"": 0, ""y"": 100, ""speed"": 5, ""wait"": 2 }
      ]
    }
  ]
}";

        [Fact]
        public void Parse_ValidFile_ReturnsAllNpcs()
        {
            var routes = JsonNpcRouteRepository.Parse(ValidJson);

            Assert.Equal(2, routes.Count);
            Assert.Equal("car1", routes[0].NpcId);
            Assert.Equal(NpcKind.Car, routes[0].Kind);
            Assert.Equal("car_3_black.png", routes[0].Sprite);
            Assert.False(routes[0].Loop);
        }

        [Fact]
        public void Parse_Waypoints_InOrder()
        {
            var waypoints = JsonNpcRouteRepository.Parse(ValidJson)[0].Waypoints;

            Assert.Equal(3, waypoints.Count);
            Assert.Equal(new Waypoint(10, 20, 50, 3), waypoints[0]);
            Assert.Equal(new Waypoint(10, 0, 30), waypoints[1]);
            Assert.Equal(new Waypoint(50, 0, 0, 9), waypoints[2]);
        }

        [Fact]
        public void Parse_OptionalFields_Defaults()
        {
            var pedestrian = JsonNpcRouteRepository.Parse(ValidJson)[1];

            Assert.True(pedestrian.Loop);
            Assert.Null(pedestrian.Waypoints[0].AccelMps2);
            Assert.Equal(0, pedestrian.Waypoints[0].WaitSeconds);
            Assert.Equal(2, pedestrian.Waypoints[1].WaitSeconds);
        }

        [Fact]
        public void Parse_KindIsCaseInsensitive()
        {
            Assert.Equal(NpcKind.Pedestrian, JsonNpcRouteRepository.Parse(ValidJson)[1].Kind);
        }

        [Fact]
        public void Parse_LessThanTwoWaypoints_ThrowsWithNpcId()
        {
            string json = @"{ ""npcs"": [ { ""id"": ""lonely"", ""kind"": ""Car"", ""sprite"": ""a.png"", ""waypoints"": [ { ""x"": 0, ""y"": 0, ""speed"": 1 } ] } ] }";

            var ex = Assert.Throws<InvalidDataException>(() => JsonNpcRouteRepository.Parse(json));
            Assert.Contains("lonely", ex.Message);
        }

        [Fact]
        public void Parse_UnknownKind_Throws()
        {
            string json = ValidJson.Replace(@"""kind"": ""Car""", @"""kind"": ""Tram""");

            Assert.Throws<InvalidDataException>(() => JsonNpcRouteRepository.Parse(json));
        }

        [Fact]
        public void Parse_MissingSpeed_Throws()
        {
            string json = @"{ ""npcs"": [ { ""id"": ""c"", ""kind"": ""Car"", ""sprite"": ""a.png"", ""waypoints"": [ { ""x"": 0, ""y"": 0 }, { ""x"": 1, ""y"": 1, ""speed"": 1 } ] } ] }";

            Assert.Throws<InvalidDataException>(() => JsonNpcRouteRepository.Parse(json));
        }

        [Fact]
        public void Parse_NegativeSpeed_Throws()
        {
            string json = ValidJson.Replace(@"""speed"": 30", @"""speed"": -30");

            Assert.Throws<InvalidDataException>(() => JsonNpcRouteRepository.Parse(json));
        }

        [Fact]
        public void Parse_InvalidJson_Throws()
        {
            Assert.Throws<InvalidDataException>(() => JsonNpcRouteRepository.Parse(@"{ ""npcs"": [ "));
        }

        [Fact]
        public void Parse_NoNpcsArray_Throws()
        {
            Assert.Throws<InvalidDataException>(() => JsonNpcRouteRepository.Parse(@"{ ""map"": ""x"" }"));
        }

        [Fact]
        public void Load_UnknownMap_ThrowsWithFileName()
        {
            var ex = Assert.Throws<FileNotFoundException>(() => new JsonNpcRouteRepository().Load("no_such_map"));

            Assert.Contains("no_such_map.json", ex.Message);
        }

        [Fact]
        public void Load_EmptyMapName_Throws()
        {
            Assert.Throws<ArgumentException>(() => new JsonNpcRouteRepository().Load(" "));
        }

        [Fact(Skip = "NPC-8")]
        public void Load_RealOvalFile_HasBlackVan()
        {
            var routes = new JsonNpcRouteRepository().Load("oval");

            var van = Assert.Single(routes);
            Assert.Equal("car_3_black.png", van.Sprite);
            Assert.Equal(NpcKind.Car, van.Kind);
            Assert.False(van.Loop);
        }

        [Fact(Skip = "NPC-8")]
        public void Load_RealTestWorldFile_HasCarAndPedestrian()
        {
            var routes = new JsonNpcRouteRepository().Load("test_world");

            Assert.Contains(routes, r => r.Kind == NpcKind.Car);
            Assert.Contains(routes, r => r.Kind == NpcKind.Pedestrian);
            Assert.Equal(routes.Count, routes.Select(r => r.NpcId).Distinct().Count());
        }
    }
}