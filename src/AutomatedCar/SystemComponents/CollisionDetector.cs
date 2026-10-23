namespace AutomatedCar.SystemComponents
{
    using AutomatedCar.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CollisionDetector : SystemComponent
    {
        public AutomatedCar Car { get; set; } 
        public WorldObject ClosestObject { get; set; }

        public event Action? DistanceFromClosestObject;
        public CollisionDetector(VirtualFunctionBus virtualFunctionBus, AutomatedCar car) : base(virtualFunctionBus)
        {
            this.Car = car;
            ClosestObject = virtualFunctionBus.RadarPacket.ClosestObject;
        }
        
        public override void Process()
        {
            ClosestObject = virtualFunctionBus.RadarPacket.ClosestObject;
            DistanceFromClosestObject.Invoke();
        }
    }
}
