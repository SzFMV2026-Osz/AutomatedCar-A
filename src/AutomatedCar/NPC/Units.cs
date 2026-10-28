using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar.NPC
{
    //Unit conversions. The world uses the 1 m = 50 px scale
     

    public static class Units
    {
        public const double PxPerMeter = 50.0;  //Pixels per meter in the world coordinate system.

        public static double KmhToPxPerSec(double kmh) => kmh / 3.6 * PxPerMeter;   //Converts km/h to px/s.

        public static double MpsToPxPerSec(double mps) => mps * PxPerMeter; //Converts m/s (or m/s²) to px/s (or px/s²)

        public static double PxPerSecToMps(double pxPerSec) => pxPerSec / PxPerMeter;   //Converts px/s to m/s.

        public static double PxPerSecToKmh(double pxPerSec) => pxPerSec / PxPerMeter * 3.6; //Converts px/s to km/h.

    }
}
