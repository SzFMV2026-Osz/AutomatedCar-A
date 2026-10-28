namespace AutomatedCar.SystemComponents.Packets
{
    using AutomatedCar.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class RadarPacket : IRadarPacket
    {
        public List<WorldObject> RelevantObjects { get; set; } = new List<WorldObject>();
        public WorldObject ClosestObject { get; set; }
        public WorldObject NearestInLane { get; set; }
        public List<WorldObject> AebRelevantObjects { get; set; } = new List<WorldObject>();
    }
}
