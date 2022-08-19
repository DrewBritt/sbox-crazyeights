using Sandbox;

namespace CrazyEights;

public partial class Game
{
    /// <summary>
    /// Current play direction. Default is clockwise/true.
    /// </summary>
    private bool DirectionIsClockwise = true;

    /// <summary>
    /// Value to be added to CurrentPlayerIndex, returns 1 if DirectionIsClockwise and -1 if !DirectionIsClockwise
    /// </summary>
    private int DirectionValue => DirectionIsClockwise ? 1 : -1;

}
