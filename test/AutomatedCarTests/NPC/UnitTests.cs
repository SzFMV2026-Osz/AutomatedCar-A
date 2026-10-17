using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomatedCar.NPC;
using Xunit;

namespace Tests.NPC
{
    public class UnitTests
    {

        [Theory]
        [InlineData(50, 694.444)]
        [InlineData(30, 416.667)]
        [InlineData(0, 0)]
        public void KmhToPxPerSec_ReturnsExpected(double kmh, double expected)
        {
            Assert.Equal(expected, Units.KmhToPxPerSec(kmh), 2);
        }

        [Fact]
        public void MpsToPxPerSec_9_Returns450()
        {
            Assert.Equal(450, Units.MpsToPxPerSec(9),6);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(1.4)]
        [InlineData(13.89)]
        public void PxPerSecToMps_RoundTrip(double mps)
        {
            Assert.Equal(mps, Units.PxPerSecToMps(Units.MpsToPxPerSec(mps)), 9);
        }

        [Fact]
        public void PxPerSecToKmh_RoundTrip()
        {
            Assert.Equal(50, Units.PxPerSecToKmh(Units.KmhToPxPerSec(50)), 9);
        }
    }
}
