using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Interfaces;

namespace GameCoClassLibrary.Classes
{
  internal class TowerShop
  {

    /// <summary>
    /// Number of pages in shop
    /// </summary>
    private readonly int _pageCount = 1;

    /// <summary>
    /// Page, which selected in shop
    /// </summary>
    private int _currentShopPage = 1;

    /// <summary>
    /// Number of selected element in Shop
    /// </summary>
    private int _towerConfSelectedID = -1;

    /// <summary>
    /// Left X,Y page selector position
    /// </summary>
    private readonly Point _paginatorPos;

    /// <summary>
    /// Left X,Y page position
    /// </summary>
    private readonly Point _pagePos;

    /// <summary>
    /// Delta between page elements
    /// </summary>
    private const int ElementsDelta = 3;

    /// <summary>
    /// Height of one element in paginator
    /// </summary>
    private const int PaginatorElementHeight = 35;

    /// <summary>
    /// Shop element icons
    /// </summary>
    private readonly ReadOnlyCollection<Bitmap> _towerIcons;

    /// <summary>
    /// Gets the current shop page.
    /// </summary>
    internal int CurrentShopPage
    {
      get { return _currentShopPage; }
      set { _currentShopPage = value > 0 && value <= _pageCount ? value : 1; }
    }

    /// <summary>
    /// Gets or sets the scaling.
    /// </summary>
    /// <value>
    /// The scaling.
    /// </value>
    internal static float Scaling { private get; set; }

    /// <summary>
    /// Get/Set number of selected in shop tower configuration
    /// </summary>
    internal int TowerConfSelectedID
    {
      get { return _towerConfSelectedID; }
      set
      {
        _towerConfSelectedID = value >= -1 && value < _towerIcons.Count ? value : -1;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TowerShop"/> class.
    /// </summary>
    /// <param name="towerIcons">The tower icons.</param>
    /// <param name="paginatorPos">The paginator pos.</param>
    /// <param name="pagePos">The page pos.</param>
    internal TowerShop(ReadOnlyCollection<Bitmap> towerIcons, Point paginatorPos, Point pagePos)
    {
      _towerIcons = towerIcons;
      TowerConfSelectedID = -1;
      _paginatorPos = paginatorPos;
      _pagePos = pagePos;
      _pageCount = ((_towerIcons.Count / (Settings.LinesInOnePage * Settings.MaxTowersInLine)) >= Settings.MaxTowerShopPageCount)
        ? Settings.MaxTowerShopPageCount
        : (_towerIcons.Count / (Settings.LinesInOnePage * Settings.MaxTowersInLine)) + 1;
    }

    /// <summary>
    /// Shops the page selector action.
    /// </summary>
    /// <param name="act">The act.</param>
    /// <param name="xMouse">The X mouse.</param>
    /// <param name="yMouse">The Y mouse.</param>
    /// <returns>If called for mouse coords checking, returns result of check</returns>
    private bool ShopPageSelectorAction(Func<int, int, int, int, bool> act, int xMouse = 0, int yMouse = 0)
    {
      int dy = 0;//Will change, if we have more than one line of pages in shop
      for (int i = 0; i < _pageCount; i++)
      {
        if ((i != 0) && (i % 3 == 0))
          dy++;
        if (act(i, dy, xMouse, yMouse))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Shops the page action.
    /// </summary>
    /// <param name="act">The act.</param>
    /// <param name="xMouse">The X mouse.</param>
    /// <param name="yMouse">The Y mouse.</param>
    /// <returns>If called for mouse coords checking, returns result of check</returns>
    private bool ShopPageAction(Func<int, int, int, int, int, bool> act, int xMouse = 0, int yMouse = 0)
    {
      int towersAtCurrentPage = GetNumberOfTowersAtPage(_currentShopPage);
      int offset = 0;
      for (int j = 0; j < Settings.LinesInOnePage; j++)
      {
        int towersInThisLane =
          (towersAtCurrentPage - j * Settings.MaxTowersInLine) >= Settings.MaxTowersInLine
          ? Settings.MaxTowersInLine
          : towersAtCurrentPage - j * Settings.MaxTowersInLine;
        for (int i = 0; i < towersInThisLane; i++)
        {
          if (act(i, j, offset, xMouse, yMouse))
            return true;
          offset++;
        }
      }
      return false;
    }

    /// <summary>
    /// Gets the number of towers at shop page.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <returns>Number of towers at shop page</returns>
    private int GetNumberOfTowersAtPage(int pageNumber = 1)
    {
      return (_pageCount != pageNumber)
               ? (Settings.LinesInOnePage * Settings.MaxTowersInLine)
               : _towerIcons.Count - (pageNumber - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine);
    }

    /// <summary>
    /// On mouse up event
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    /// <param name="status">Mouse up result</param>
    public void MouseUp(MouseEventArgs e, out TowerShopActStatus status)
    {
      bool result;

      #region Tower Page Selection

      if (_towerIcons.Count > Settings.LinesInOnePage * Settings.MaxTowersInLine
        && (e.X >= Convert.ToInt32(_paginatorPos.X * Scaling)
            && e.Y >= Convert.ToInt32(_paginatorPos.Y * Scaling)
            && e.Y <= Convert.ToInt32((_paginatorPos.Y + PaginatorElementHeight * 2) * Scaling)))
      {
        result = ShopPageSelectorAction((int i, int dy, int xMouse, int yMouse) =>
        {
          if (BuildRectPageSelector(i, dy).Contains(xMouse, yMouse))
          {
            _currentShopPage = i + 1;
            //FinishTowerShopAct();
            return true;
          }
          return false;
        }, e.X, e.Y);
        if (result)
        {
          TowerConfSelectedID = -1;
          status = TowerShopActStatus.ShopActFinish;
          return;
        }
      }

      #endregion Tower Page Selection

      #region Tower Selected in Shop

      if (e.X >= Convert.ToInt32(_pagePos.X * Scaling)
          && (e.Y >= Convert.ToInt32(_pagePos.Y * Scaling))
          && (e.Y <= Convert.ToInt32(_pagePos.Y + (Settings.TowerIconSize + Settings.DeltaY) * ((GetNumberOfTowersAtPage(_currentShopPage) / Settings.MaxTowersInLine) + 1) * Scaling)))//Если в границах
      {
        result = ShopPageAction((int i, int j, int offset, int xMouse, int yMouse) =>
        {
          if (BuildRectPage(i, j).Contains(xMouse, yMouse))//Если нашли выделенную башню
          {
            TowerConfSelectedID = (_currentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset;
            return true;
          }
          return false;
        }, e.X, e.Y);
        if (result)
        {
          status = TowerShopActStatus.MapActFinish;
          return;
        }
      }

      #endregion Tower Selected in Shop

      status = TowerShopActStatus.Normal;
    }

    /// <summary>
    /// Shop rendering
    /// </summary>
    /// <param name="graphObject">The graph object.</param>
    internal void Show(IGraphic graphObject)
    {
      ShowPageSelector(graphObject);
      ShowTowerShopPage(graphObject);
    }

    /// <summary>
    /// Shows the page selector.(Pages in shop)
    /// </summary>
    /// <param name="graphObject">Graphic render object</param>
    private void ShowPageSelector(IGraphic graphObject)
    {
      if (_towerIcons.Count > Settings.LinesInOnePage * Settings.MaxTowersInLine)
      {
        ShopPageSelectorAction((int i, int dy, int xMouse, int yMouse) =>
        {
          //String
          graphObject.DrawString("Page " + (i + 1).ToString(CultureInfo.InvariantCulture), new Font("Arial", 14 * Scaling), new SolidBrush(Color.Black),
            new Point(
              Convert.ToInt32((_paginatorPos.X + (i % ElementsDelta) * ("Page " + (i + 1).ToString(CultureInfo.InvariantCulture)).Length * Settings.PixelsForOneSymbol) * Scaling),
              Convert.ToInt32((_paginatorPos.Y + PaginatorElementHeight * dy) * Scaling)));
          //Border line
          Color penColor = ((i + 1) == CurrentShopPage) ? Color.Red : Color.White;
          graphObject.DrawRectangle(new Pen(penColor, Settings.PenWidth * Scaling), BuildRectPageSelector(i, dy));
          return false;
        });
      }
    }

    /// <summary>
    /// Shows the tower shop page.
    /// </summary>
    /// <param name="graphObject">Graphic render object</param>
    private void ShowTowerShopPage(IGraphic graphObject)
    {
      ShopPageAction((int i, int j, int offset, int xMouse, int yMouse) =>
      {
        graphObject.DrawImage(_towerIcons[(CurrentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset],
          BuildRectPage(i, j));
        if (TowerConfSelectedID == (CurrentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset)
          //Border line
          graphObject.DrawRectangle(new Pen(Color.Red, Settings.PenWidth * Scaling), BuildRectPage(i, j));
        return false;
      });
    }

    /// <summary>
    /// Rectangle Building for shop page menu
    /// </summary>
    private Rectangle BuildRectPageSelector(int x, int dy)
    {
      return new Rectangle(
        Convert.ToInt32((_paginatorPos.X + (x % ElementsDelta) * ("Page " + (x + 1).ToString(CultureInfo.InvariantCulture)).Length * Settings.PixelsForOneSymbol) * Scaling),
        Convert.ToInt32((_paginatorPos.Y + PaginatorElementHeight * dy) * Scaling),
        Convert.ToInt32(("Page " + (x + 1).ToString(CultureInfo.InvariantCulture)).Length * (Settings.PixelsForOneSymbol - 1) * Scaling),
        Convert.ToInt32(Settings.PixelsForOneSymbol * 2 * Scaling));
    }

    /// <summary>
    /// Rectangle Building for shop page element
    /// </summary>
    private Rectangle BuildRectPage(int x, int y)
    {
      return new Rectangle(
        Convert.ToInt32((_pagePos.X + x * (Settings.TowerIconSize + Settings.DeltaX)) * Scaling),
        Convert.ToInt32((_pagePos.Y + y * (Settings.TowerIconSize + Settings.DeltaY)) * Scaling),
        Convert.ToInt32(Settings.TowerIconSize * Scaling),
        Convert.ToInt32(Settings.TowerIconSize * Scaling));
    }
  }
}