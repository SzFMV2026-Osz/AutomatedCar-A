namespace AutomatedCar.SystemComponents.Packets
{
    using AutomatedCar.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class RadarPacket
    {
        public List<WorldObject> RelevantObjects { get; set; }
        public WorldObject ClosestObject { get; set; }
    }
}
