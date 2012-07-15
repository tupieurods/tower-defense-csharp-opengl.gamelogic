using System.Drawing;
using GameCoClassLibrary.Loaders;

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
    public const int WindowWidth = 730;

    /// <summary>
    /// Window height for drawing
    /// </summary>
    public const int WindowHeight = 600;

    /// <summary>
    /// Size of square tower icon
    /// </summary>
    internal const int TowerIconSize = 32;

    /// <summary>
    /// Money pict y position
    /// </summary>
    internal const int MoneyYPos = DeltaY / 2;

    /// <summary>
    /// Font size in game
    /// </summary>
    internal const int FontSize = 14;

    /// <summary>
    /// Map area size in pixels
    /// MapAreaSize==map area height==map area width
    /// MapAreaSize!=map area height*map area width
    /// </summary>
    internal const int MapAreaSize = ElemSize * 30;

    /// <summary>
    /// Delta X graphic between elements
    /// </summary>
    internal const int DeltaX = 10;

    /// <summary>
    /// Delta Y graphic between elements
    /// </summary>
    internal const int DeltaY = 10;

    /// <summary>
    /// Maximum number of lines in one shop page
    /// </summary>
    internal const int LinesInOnePage = 3;

    /// <summary>
    /// Maximum number of tower in one shop page line
    /// </summary>
    internal const int MaxTowersInLine = 5;

    /// <summary>
    /// Maximal number of pages in tower shop
    /// </summary>
    internal const int MaxTowerShopPageCount = 6;

    /// <summary>
    /// Pen windth for monster life bar
    /// </summary>
    internal const int PenWidth = 3;

    /// <summary>
    /// X position of breakup line
    /// </summary>
    internal const int BreakupLineXPosition = MapAreaSize + DeltaX * 2;

    /// <summary>
    /// Number of pixels for one rendered symbol
    /// </summary>
    internal const int PixelsForOneSymbol = 12;

    /// <summary>
    /// Left X,Y tower shop page selector position
    /// </summary>
    internal static readonly Point TowerShopPageSelectorPos = new Point(BreakupLineXPosition + DeltaX, 67);

    /// <summary>
    /// Left X,Y tower shop page position
    /// </summary>
    internal static readonly Point TowerShopPagePos = new Point(BreakupLineXPosition + DeltaX, 135);

  }
}