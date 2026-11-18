using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar.ACC
{
    public enum AccState
    {
        Off,
        Active,

        // the driver presses the throttle, the ACC waits until it is released
        Overridden,
    }
}
