using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using GameCoClassLibrary.Enums;
using GraphicLib.Interfaces;

namespace GameCoClassLibrary.Classes
{
  internal abstract class Shop
  {
    #region Vars

    #region Private

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
    private int _confSelectedID = -1;

    /// <summary>
    /// Left X,Y page selector position
    /// </summary>
    protected readonly Point PaginatorPos;

    /// <summary>
    /// Left X,Y page position
    /// </summary>
    protected readonly Point PagePos;

    /// <summary>
    /// Shop element icons
    /// </summary>
    private readonly ReadOnlyCollection<Bitmap> _icons;

    /// <summary>
    /// Height of one element in paginator
    /// </summary>
    protected const int PaginatorElementHeight = 35;

    /// <summary>
    /// Graphic scaling
    /// </summary>
    protected static float ScalingValue = 1.0f;

    #endregion

    #region Internal

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
    internal static float Scaling { set { ScalingValue = value; } }

    /// <summary>
    /// Get/Set number of selected in shop configuration
    /// </summary>
    internal int ConfSelectedID
    {
      get { return _confSelectedID; }
      set
      {
        _confSelectedID = value >= -1 && value < _icons.Count ? value : -1;
      }
    }

    #endregion

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Shop"/> class.
    /// </summary>
    /// <param name="icons">The icons.</param>
    /// <param name="paginatorPos">The paginator pos.</param>
    /// <param name="pagePos">The page pos.</param>
    protected Shop(ReadOnlyCollection<Bitmap> icons, Point paginatorPos, Point pagePos)
    {
      _icons = icons;
      ConfSelectedID = -1;
      PaginatorPos = paginatorPos;
      PagePos = pagePos;
      _pageCount = ((_icons.Count / (Settings.LinesInOnePage * Settings.MaxTowersInLine)) >= Settings.MaxTowerShopPageCount)
        ? Settings.MaxTowerShopPageCount
        : (_icons.Count / (Settings.LinesInOnePage * Settings.MaxTowersInLine)) + 1;
    }

    #region Logic
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
      int atCurrentPage = GetNumberOfElementsAtPage(_currentShopPage);
      int offset = 0;
      for (int j = 0; j < Settings.LinesInOnePage; j++)
      {
        int elementsInThisLane =
          (atCurrentPage - j * Settings.MaxTowersInLine) >= Settings.MaxTowersInLine
          ? Settings.MaxTowersInLine
          : atCurrentPage - j * Settings.MaxTowersInLine;
        for (int i = 0; i < elementsInThisLane; i++)
        {
          if (act(i, j, offset, xMouse, yMouse))
            return true;
          offset++;
        }
      }
      return false;
    }

    /// <summary>
    /// Gets the number of elements at shop page.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <returns>Number of elements at shop page</returns>
    private int GetNumberOfElementsAtPage(int pageNumber = 1)
    {
      return (_pageCount != pageNumber)
               ? (Settings.LinesInOnePage * Settings.MaxTowersInLine)
               : _icons.Count - (pageNumber - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine);
    }
    #endregion

    #region Mouse click coords checking
    /// <summary>
    /// On mouse up event
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    /// <param name="status">Mouse up result</param>
    public void MouseUp(MouseEventArgs e, out ShopActStatus status)
    {
      //Page Selection
      if (PaginatorClickChecking(e, out status))
        return;

      //selection in shop page
      if (ShopPageClickChecking(e, out status))
        return;

      status = ShopActStatus.Normal;
    }

    /// <summary>
    /// Checks, user click on element in shop page or not
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    /// <param name="status">The status.</param>
    /// <returns></returns>
    private bool ShopPageClickChecking(MouseEventArgs e, out ShopActStatus status)
    {
      //Если в границах
      if (e.X >= Convert.ToInt32(PagePos.X * ScalingValue)
          && (e.Y >= Convert.ToInt32(PagePos.Y * ScalingValue))
          && (e.Y <= Convert.ToInt32((PagePos.Y +
              (Settings.TowerIconSize + Settings.DeltaY) * ((GetNumberOfElementsAtPage(_currentShopPage) / Settings.MaxTowersInLine) + 1)) * ScalingValue)))
      {
        Func<int, int, int, int, int, bool> clickChecker =
          (int i, int j, int offset, int xMouse, int yMouse) =>
          {
            //Если нашли выделенную башню
            if (BuildRectPage(i, j).Contains(xMouse, yMouse))
            {
              ConfSelectedID = (_currentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset;
              return true;
            }
            return false;
          };
        if (ShopPageAction(clickChecker, e.X, e.Y))
        {
          status = ShopActStatus.MapActFinish;
          return true;
        }
      }
      status = ShopActStatus.Normal;
      return false;
    }

    /// <summary>
    /// Checks, user click on page button in paginator or not
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    /// <param name="status">The status.</param>
    /// <returns></returns>
    private bool PaginatorClickChecking(MouseEventArgs e, out ShopActStatus status)
    {
      if (_icons.Count > Settings.LinesInOnePage * Settings.MaxTowersInLine
          && (e.X >= Convert.ToInt32(PaginatorPos.X * ScalingValue)
              && e.Y >= Convert.ToInt32(PaginatorPos.Y * ScalingValue)
              && e.Y <= Convert.ToInt32((PaginatorPos.Y + PaginatorElementHeight * 2) * ScalingValue)))
      {
        Func<int, int, int, int, bool> clickChecker =
          (int i, int dy, int xMouse, int yMouse) =>
          {
            if (BuildRectPageSelector(i, dy).Contains(xMouse, yMouse))
            {
              _currentShopPage = i + 1;
              return true;
            }
            return false;
          };
        if (ShopPageSelectorAction(clickChecker, e.X, e.Y))
        {
          ConfSelectedID = -1;
          status = ShopActStatus.ShopActFinish;
          return true;
        }
      }
      status = ShopActStatus.Normal;
      return false;
    }
    #endregion

    #region Graphical class part
    /// <summary>
    /// Shop rendering
    /// </summary>
    /// <param name="graphObject">The graph object.</param>
    internal void Show(IGraphic graphObject)
    {
      ShowPageSelector(graphObject);
      ShowShopPage(graphObject);
    }

    /// <summary>
    /// Shows the page selector.(Pages in shop)
    /// </summary>
    /// <param name="graphObject">Graphic render object</param>
    private void ShowPageSelector(IGraphic graphObject)
    {
      if (_icons.Count > Settings.LinesInOnePage * Settings.MaxTowersInLine)
      {
        ShopPageSelectorAction(
          (int i, int dy, int xMouse, int yMouse) =>
          {
            Rectangle tmp = BuildRectPageSelector(i, dy);
            //String
            graphObject.DrawString("Page " + (i + 1).ToString(CultureInfo.InvariantCulture), new Font("Arial", 14 * ScalingValue), new SolidBrush(Color.Black), tmp.Location);
            //Border line
            Color penColor = ((i + 1) == CurrentShopPage) ? Color.Red : Color.White;
            graphObject.DrawRectangle(new Pen(penColor, Settings.PenWidth * ScalingValue), tmp);
            return false;
          });
      }
    }

    /// <summary>
    /// Shows the shop page.
    /// </summary>
    /// <param name="graphObject">Graphic render object</param>
    private void ShowShopPage(IGraphic graphObject)
    {
      ShopPageAction((int i, int j, int offset, int xMouse, int yMouse) =>
      {
        graphObject.DrawImage(_icons[(CurrentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset], BuildRectPage(i, j));
        if (ConfSelectedID == (CurrentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset)
          //Border line
          graphObject.DrawRectangle(new Pen(Color.Red, Settings.PenWidth * ScalingValue), BuildRectPage(i, j));
        return false;
      });
    }
    #endregion

    #region Rectangle builders

    protected abstract Rectangle BuildRectPageSelector(int x, int dy);
    protected abstract Rectangle BuildRectPage(int x, int y);

    #endregion
  }
}
