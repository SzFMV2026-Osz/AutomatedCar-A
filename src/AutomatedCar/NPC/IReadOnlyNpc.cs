using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// Read-only view of a non-player character. Other modules (radar, collision detection, ACC, AEB)
/// may use this to read the state of an NPC found in <c>World.WorldObjects</c>.

namespace AutomatedCar.NPC
{
    public interface IReadOnlyNpc
    {
        /// Gets the unique identifier of the NPC (the route id)
        string Id { get; }
        /// >Gets the kind of the NPC.
        NpcKind Kind { get; }
        /// Gets the current movement state.
        NpcState State { get; }
        ///Gets the current speed in m/s.
        double SpeedMps { get; }
        ///Gets the heading in degrees, using the WorldObject.Rotation convention (0° = up, clockwise positive).
        double HeadingDeg { get; }

    }
}
