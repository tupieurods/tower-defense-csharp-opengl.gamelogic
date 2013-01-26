using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;

namespace GameCoClassLibrary.Classes
{
  internal class TowerShop: Shop
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TowerShop"/> class.
    /// </summary>
    /// <param name="icons">The tower icons.</param>
    /// <param name="paginatorPos">The pagination pos.</param>
    /// <param name="pagePos">The page pos.</param>
    internal TowerShop(ReadOnlyCollection<Bitmap> icons, Point paginatorPos, Point pagePos)
      : base(icons, paginatorPos, pagePos)
    {
    }

    /// <summary>
    /// Rectangle Building for shop page menu
    /// </summary>
    protected override Rectangle BuildRectPageSelector(int x, int dy)
    {
      return new Rectangle(
        Convert.ToInt32((PaginatorPos.X
                         + (x % 3) * ("Page " + (x + 1).ToString(CultureInfo.InvariantCulture)).Length
                         * Settings.PixelsForOneSymbol) * ScalingValue),
        Convert.ToInt32((PaginatorPos.Y + PaginatorElementHeight * dy) * ScalingValue),
        Convert.ToInt32(("Page " + (x + 1).ToString(CultureInfo.InvariantCulture)).Length
                        * (Settings.PixelsForOneSymbol - 1) * ScalingValue),
        Convert.ToInt32(Settings.PixelsForOneSymbol * 2 * ScalingValue));
    }

    /// <summary>
    /// Rectangle Building for shop page element
    /// </summary>
    protected override Rectangle BuildRectPage(int x, int y)
    {
      return new Rectangle(
        Convert.ToInt32((PagePos.X + x * (Settings.TowerIconSize + Settings.DeltaX)) * ScalingValue),
        Convert.ToInt32((PagePos.Y + y * (Settings.TowerIconSize + Settings.DeltaY)) * ScalingValue),
        Convert.ToInt32(Settings.TowerIconSize * ScalingValue),
        Convert.ToInt32(Settings.TowerIconSize * ScalingValue));
    }
  }
}