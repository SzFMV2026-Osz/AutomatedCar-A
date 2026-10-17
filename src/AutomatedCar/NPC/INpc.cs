using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedCar.NPC
{
    public interface INpc :IReadOnlyNpc
    {
        /// Advances the NPC by the given time.
        /// Elapsed time in seconds.
        void Update(double deltaSeconds);

        ///Moves the NPC back to the start of its route.
        void Reset();
    }
}
