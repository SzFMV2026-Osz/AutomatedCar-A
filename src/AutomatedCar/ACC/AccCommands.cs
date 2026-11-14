using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar.ACC
{
    public readonly record struct AccCommands(bool Toggle, bool SpeedUp, bool SpeedDown, bool CycleTimeGap)
    {
        public static AccCommands None => default;
    }
}
