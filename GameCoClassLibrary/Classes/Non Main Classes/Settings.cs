namespace GameCoClassLibrary.Classes
{
  /// <summary>
  /// Class with settings
  /// </summary>
  static public class Settings
  {
    /// <summary>
    /// Base size of map element
    /// </summary>
    public const int ElemSize = 16;
    /// <summary>
    /// Window width for drawing
    /// </summary>
    public const int WindowWidth=730;
    /// <summary>
    /// Window height for drawing
    /// </summary>
    public const int WindowHeight = 600;
    /// <summary>
    /// Map area size in pixels
    /// </summary>
    static internal int MapAreaSize = ElemSize * 30;
    /// <summary>
    /// Delta form the 0,0 of window for map
    /// </summary>
    static internal int DeltaX = 10;
    /// <summary>
    /// Delta form the 0,0 of window for map
    /// </summary>
    static internal int DeltaY = 10;
    /// <summary>
    /// Maximum number of lines in one shop page
    /// </summary>
    static internal int LinesInOnePage = 3;
    /// <summary>
    /// Maximum number of tower in one shop page line
    /// </summary>
    static internal int MaxTowersInLine = 5;
    /// <summary>
    /// Pen windth for monster life bar
    /// </summary>
    internal static int PenWidth = 3;
  }
}