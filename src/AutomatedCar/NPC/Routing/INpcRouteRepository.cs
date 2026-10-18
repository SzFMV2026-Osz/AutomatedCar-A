using System.Collections.Generic;
using System;
using AutomatedCar.NPC;

namespace AutomatedCar.NPC.Routing
{
    public interface INpcRouteRepository
    {
        IReadOnlyList<NpcRoute> Load(string mapName);
    }
}