namespace AutomatedCar.SystemComponents
{
    using AutomatedCar.Helpers;
    using AutomatedCar.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CollisionDetector : SystemComponent
    {
        private List<WorldObject> touchedObjects = new List<WorldObject>();

        public CollisionDetector(VirtualFunctionBus virtualFunctionBus, AutomatedCar car) : base(virtualFunctionBus)
        {
            this.Car = car;
        }

        public event EventHandler<WorldObject> Collision;

        public AutomatedCar Car { get; }

        public override void Process()
        {
            var touching = World.Instance.WorldObjects.ToList()
                .Where(wb => wb != this.Car && (wb.Collideable || wb.WorldObjectType == WorldObjectType.Car || wb.WorldObjectType == WorldObjectType.Pedestrian))
                .Where(wb => GeometryHelper.Touches(this.Car, wb))
                .ToList();

            foreach (WorldObject wb in touching.Where(wb => !this.touchedObjects.Contains(wb)))
            {
                this.Collision?.Invoke(this, wb);
            }

            this.touchedObjects = touching;
        }
    }
}
