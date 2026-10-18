using System.Collections.Generic;
using System;

namespace AutomatedCar.Npc.Routing
{
    public interface INpcRouteRepository
    {
        IReadOnlyList<NpcRoute> Load(string mapName);
    }
}