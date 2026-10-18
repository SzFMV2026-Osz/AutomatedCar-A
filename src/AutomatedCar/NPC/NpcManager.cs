using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Steps every registered NPC with its own 60 Hz timer.

namespace AutomatedCar.NPC
{
    public class NpcManager : GameBase
    {
        private readonly List<INpc> npcs = new List<INpc>();

        // gets the registered NPCs as a read-only list
        public IReadOnlyList<IReadOnlyNpc> Npcs => npcs.AsReadOnly();

        public void Register(INpc npc)
        {
            ArgumentNullException.ThrowIfNull(npc);
            if (!this.npcs.Contains(npc))
            {
                this.npcs.Add(npc);
            }
        }

        public void Step(double deltaSeconds)
        {
            foreach (var npc in this.npcs)
            {
                npc.Update(deltaSeconds);
            }
        }

        public void StepOneTick() => this.Step(1.0 / TicksPerSecond);

        public void ResetAll()
        {
            foreach (var npc in this.npcs)
            {
                npc.Reset();
            }
        }

        protected override void Tick() => this.StepOneTick();
    }
}
