using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar.ACC
{
    // X, Y: rotation point of our car in px; FrontOffsetPx: rotation point -> front bumper
    public sealed record EgoPose(double X, double Y, double HeadingDeg, double FrontOffsetPx);
}
