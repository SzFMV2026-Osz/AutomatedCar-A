using AutomatedCar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.NPC
{
    public class MapSelectionTests
    {
        [Fact]
        public void NoArgument_DefaultsToTestWorld()
        {
            Assert.Equal("test_world", MapSelection.Resolve(new string[0]));
            Assert.Equal("test_world", MapSelection.Resolve(null));
        }

        [Fact]
        public void MapArgument_SelectsOval()
        {
            Assert.Equal("oval", MapSelection.Resolve(new[] { "--map", "oval" }));
            Assert.Equal("oval", MapSelection.Resolve(new[] { "--map=OVAL" }));
        }

        [Fact]
        public void UnknownMap_Throws()
        {
            Assert.Throws<ArgumentException>(() => MapSelection.Resolve(new[] { "--map", "mars" }));
        }
    }
}
