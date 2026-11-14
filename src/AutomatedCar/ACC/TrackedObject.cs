using AutomatedCar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar.ACC
{
    // SpeedMps is null when the object does not tell its speed
    public sealed record TrackedObject(
        object Source,
        WorldObjectType Type,
        bool Collideable,
        IReadOnlyList<Vec2> WorldPolygon,
        Vec2 Position,
        double? SpeedMps);
}
