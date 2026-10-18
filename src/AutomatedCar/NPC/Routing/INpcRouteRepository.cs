using System.Collections.Generic;
using System;
using AutomatedCar.Npc;
using AutomatedCar.Npc.Routing;

namespace AutomatedCar.Npc.Routing
{
    public interface INpcRouteRepository
    {
        IReadOnlyList<NpcRoute> Load(string mapName);
    }
}