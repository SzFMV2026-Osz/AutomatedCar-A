namespace AutomatedCar.SystemComponents.Packets
{
    using AutomatedCar.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CameraPacket : ICameraPacket
    {
        public List<WorldObject> RelevantEverything { get; set; } = new List<WorldObject>();
        public List<WorldObject> RelevantObjects { get; set; } = new List<WorldObject>();
        public List<WorldObject> RelevantRoads { get; set; } = new List<WorldObject>();
        public WorldObject ClosestObject { get; set; }
    }
}
