using System;
using System.Drawing;
using System.Linq;
using System.Globalization;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Interfaces;
using GameCoClassLibrary.Loaders;

namespace GameCoClassLibrary.Classes
{
  //About Magic numbers, they are all documented on the list of paper
  internal sealed class GraphicEngine
  {
    /// <summary>
    /// IGraphic interface
    /// </summary>
    private readonly IGraphic _graphObject;
    /// <summary>
    /// Caching
    /// </summary>
    private Bitmap _constantMapImage;
    /// <summary>
    /// Background color
    /// </summary>
    private readonly Color _backgroundColor = Color.Silver;//Цвет заднего фона

    /// <summary>
    /// Gets or sets a value indicating need or not to call Map.GetConstantBitmap metho.
    /// </summary>
    internal bool RepaintConstImage { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicEngine"/> class.
    /// </summary>
    /// <param name="graphObject">The graph object.</param>
    internal GraphicEngine(IGraphic graphObject)
    {
      _graphObject = graphObject;
    }

    /// <summary>
    /// Sets the new graph buffer.
    /// For WinForms Graphic only
    /// </summary>
    /// <param name="graphicalBuffer">The graphical buffer.</param>
    internal void SetNewGraphBuffer(BufferedGraphics graphicalBuffer)
    {
      WinFormsGraphic winFormsGraphic = _graphObject as WinFormsGraphic;
      if (winFormsGraphic != null)
        winFormsGraphic.SetNewGraphBuffer(graphicalBuffer);
    }

    /// <summary>
    /// Recreates the constant map image.
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    /// <param name="scale">The scale.</param>
    internal void RecreateConstantImage(Game gameObj, float scale)
    {
      _constantMapImage = new Bitmap(Convert.ToInt32((gameObj.Map.VisibleXFinish - gameObj.Map.VisibleXStart) * Settings.ElemSize * scale),
            Convert.ToInt32((gameObj.Map.VisibleYFinish - gameObj.Map.VisibleYStart) * Settings.ElemSize * scale));
      RepaintConstImage = true;
    }

    /// <summary>
    /// Render all game objects
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    internal void Show(Game gameObj)
    {
      //Fill background
      _graphObject.FillRectangle(new SolidBrush(_backgroundColor), 0, 0, Convert.ToInt32(Settings.WindowWidth * gameObj.Scaling),
        Convert.ToInt32(Settings.WindowHeight * gameObj.Scaling));
      //Show map areaS
      MapAreaShowing(gameObj);
      //StartLevelButton
      BStartLevelShow(gameObj);
      _graphObject.DrawString(gameObj.Monsters.Count.ToString(CultureInfo.InvariantCulture), new Font("Arial", Settings.ElemSize),
        new SolidBrush(Color.Black), new Point(0, 0));

      #region GUI

      {
        //The line of breakup
        _graphObject.DrawLine(new Pen(new SolidBrush(Color.White), 3 * gameObj.Scaling),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling), 0),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling), Convert.ToInt32(Settings.WindowHeight * gameObj.Scaling)));
        ShowMoney(gameObj);//Gold
        ShowLives(gameObj);//lives
        ShowPageSelector(gameObj);//Shop menu
        ShowTowerShopPage(gameObj);//Shop page
        if ((gameObj.TowerConfSelectedID != -1) || (gameObj.TowerMapSelectedID != -1))//Нужно ли выводить параметры
          ShowTowerParams(gameObj);
      }

      #endregion GUI

      _graphObject.Render();
    }

    /// <summary>
    /// Renders map area
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    private void MapAreaShowing(Game gameObj)
    {
      //If needs to change constant map image
      if (RepaintConstImage)
      {
        gameObj.Map.GetConstantBitmap(_constantMapImage, Convert.ToInt32(Settings.MapAreaSize * gameObj.Scaling),
                                      Convert.ToInt32(Settings.MapAreaSize * gameObj.Scaling));
        RepaintConstImage = false;
        //Memory leak fix
        GC.Collect();
      }
      //Limitation of the area for drawing
      _graphObject.Clip = new Region(new Rectangle(Settings.DeltaX, Settings.DeltaY,
                                            Convert.ToInt32((gameObj.Map.VisibleXFinish - gameObj.Map.VisibleXStart) *
                                                            Settings.ElemSize * gameObj.Scaling),
                                            Convert.ToInt32((gameObj.Map.VisibleYFinish - gameObj.Map.VisibleYStart) *
                                                            Settings.ElemSize * gameObj.Scaling)));
      //Map showing
      _graphObject.DrawImage(_constantMapImage, Settings.DeltaX, Settings.DeltaY, _constantMapImage.Width,
                      _constantMapImage.Height);
      Point visibleStart = new Point(gameObj.Map.VisibleXStart, gameObj.Map.VisibleYStart);
      Point visibleFinish = new Point(gameObj.Map.VisibleXFinish, gameObj.Map.VisibleYFinish);
      //Towers showing
      foreach (Tower tower in gameObj.Towers)
        tower.ShowTower(_graphObject, visibleStart, visibleFinish);
      //Monsters showing
      foreach (Monster monster in gameObj.Monsters.Where(monster => !monster.DestroyMe))
        monster.ShowMonster(_graphObject, visibleStart, visibleFinish);

      #region Showing square and circle aroun selected tower or arount the tower, which player want to stand

      if (gameObj.ArrayPosForTowerStanding.X != -1)
      {
        ShowSquareAndCircleAtTower(gameObj,
          gameObj.ArrayPosForTowerStanding,
          gameObj.TowerParamsForBuilding[gameObj.TowerConfSelectedID].UpgradeParams[0].AttackRadius,
          gameObj.Check(gameObj.ArrayPosForTowerStanding) ? Color.White : Color.Red);
      }
      else if (gameObj.TowerMapSelectedID != -1)
      {
        ShowSquareAndCircleAtTower(gameObj,
          new Point(gameObj.Towers[gameObj.TowerMapSelectedID].ArrayPos.X - gameObj.Map.VisibleXStart,
          gameObj.Towers[gameObj.TowerMapSelectedID].ArrayPos.Y - gameObj.Map.VisibleYStart),
          gameObj.Towers[gameObj.TowerMapSelectedID].CurrentTowerParams.AttackRadius, Color.White);
      }

      #endregion Showing square and circle aroun selected tower or arount the tower, which player want to stand

      //Missle showing
      foreach (Missle missle in gameObj.Missels.Where(missle => !missle.DestroyMe))
        missle.Show(_graphObject, visibleStart, visibleFinish, gameObj.Monsters);
      _graphObject.Clip = new Region();
    }

    #region Tower shop Sector

    /// <summary>
    /// Shows the page selector.(Pages in shop)
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    private void ShowPageSelector(Game gameObj)
    {
      if (gameObj.TowerParamsForBuilding.Count > Settings.ElemSize)
      {
        // ReSharper disable InconsistentNaming
        gameObj.ShopPageSelectorAction((int i, int dy, int XMouse, int YMouse) =>
        // ReSharper restore InconsistentNaming
        {
          //String
          _graphObject.DrawString("Page " + (i + 1).ToString(CultureInfo.InvariantCulture), new Font("Arial", 14 * gameObj.Scaling), new SolidBrush(Color.Black),
            new Point(
              Convert.ToInt32((Settings.MapAreaSize + 10 + (i % 3) * ("Page " + (i + 1).ToString(CultureInfo.InvariantCulture)).Length * 12 + Settings.DeltaX * 2) * gameObj.Scaling),
              Convert.ToInt32((Res.MoneyPict.Height + 35 * (dy + 1)) * gameObj.Scaling)));
          //Border line
          Color penColor = ((i + 1) == gameObj.CurrentShopPage) ? Color.Red : Color.White;
          _graphObject.DrawRectangle(new Pen(penColor, 2 * gameObj.Scaling), Helpers.LambdaBuildRectPageSelector(gameObj, i, dy));
          return false;
        });
      }
    }

    /// <summary>
    /// Shows the tower shop page.
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    private void ShowTowerShopPage(Game gameObj)
    {
      // ReSharper disable InconsistentNaming
      gameObj.ShopPageAction((int i, int j, int offset, int XMouse, int YMouse) =>
      // ReSharper restore InconsistentNaming
      {
        _graphObject.DrawImage(gameObj.TowerParamsForBuilding[(gameObj.CurrentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset].Icon,
          Helpers.LambdaBuildRectPage(gameObj, i, j));
        if (gameObj.TowerConfSelectedID == (gameObj.CurrentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset)
          //Border line
          _graphObject.DrawRectangle(new Pen(Color.Red, 3 * gameObj.Scaling), Helpers.LambdaBuildRectPage(gameObj, i, j));
        return false;
      });
    }

    /// <summary>
    /// Shows the tower params.
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    private void ShowTowerParams(Game gameObj)
    {
      string strToShow = "";
      if (gameObj.TowerConfSelectedID != -1)//Information about tower, which player want to build
      {
        strToShow = gameObj.TowerParamsForBuilding[gameObj.TowerConfSelectedID]
          + gameObj.TowerParamsForBuilding[gameObj.TowerConfSelectedID].UpgradeParams[0].ToString();
        _graphObject.DrawString("Cost: " + gameObj.TowerParamsForBuilding[gameObj.TowerConfSelectedID].UpgradeParams[0].Cost,
          new Font("Arial", Settings.ElemSize * gameObj.Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling) + 5, Convert.ToInt32(390 * gameObj.Scaling)));
      }
      if (gameObj.TowerMapSelectedID != -1)//Information about tower, which player selected on the map
      {
        strToShow = gameObj.Towers[gameObj.TowerMapSelectedID].ToString();
        //Icon
        _graphObject.DrawImage(gameObj.Towers[gameObj.TowerMapSelectedID].Icon,
          Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling) + 5, Convert.ToInt32(375 * gameObj.Scaling),
          Convert.ToInt32(gameObj.Towers[gameObj.TowerMapSelectedID].Icon.Width * gameObj.Scaling),
           Convert.ToInt32(gameObj.Towers[gameObj.TowerMapSelectedID].Icon.Height * gameObj.Scaling));
        //Current tower level
        _graphObject.DrawString("Level: " + gameObj.Towers[gameObj.TowerMapSelectedID].Level.ToString(CultureInfo.InvariantCulture),
          new Font("Arial", Settings.ElemSize * gameObj.Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + gameObj.Towers[gameObj.TowerMapSelectedID].Icon.Width + Settings.DeltaX * 2) * gameObj.Scaling) + 5,
            Convert.ToInt32((375 + gameObj.Towers[gameObj.TowerMapSelectedID].Icon.Height / 2) * gameObj.Scaling)));
        //Destroy and Upgrade buttons
        BDestroyShow(gameObj);
        BUpgradeShow(gameObj);
      }
      //Parametrs
      _graphObject.DrawString(strToShow,
          new Font("Arial", 10 * gameObj.Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling) + 5, Convert.ToInt32(415 * gameObj.Scaling)));
      //Border line
      _graphObject.DrawRectangle(new Pen(Color.Black), Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling) + 5,
        Convert.ToInt32(415 * gameObj.Scaling), Convert.ToInt32((200 - Settings.DeltaX * 2) * gameObj.Scaling),
        Convert.ToInt32((184) * gameObj.Scaling));
    }

    /// <summary>
    /// Shows the square and circle at tower.
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    /// <param name="position">The position.</param>
    /// <param name="radius">The radius.</param>
    /// <param name="circleColor">Color of the circle.</param>
    private void ShowSquareAndCircleAtTower(Game gameObj, Point position, int radius, Color circleColor)
    {
      //Square
      _graphObject.DrawRectangle(new Pen(Color.Black),
          Convert.ToInt32(position.X * Settings.ElemSize * gameObj.Scaling + Settings.DeltaX),
          Convert.ToInt32(position.Y * Settings.ElemSize * gameObj.Scaling + Settings.DeltaY),
          Convert.ToInt32(Settings.ElemSize * 2 * gameObj.Scaling),
          Convert.ToInt32(Settings.ElemSize * 2 * gameObj.Scaling));
      //Circle
      _graphObject.DrawEllipse(new Pen(circleColor), ((position.X + 1) * Settings.ElemSize - radius) * gameObj.Scaling + Settings.DeltaX,
          ((position.Y + 1) * Settings.ElemSize - radius) * gameObj.Scaling + Settings.DeltaY, radius * 2 * gameObj.Scaling, radius * 2 * gameObj.Scaling);
    }

    #endregion Сектор башен

    //This region have so small methods, thats for future development
    #region Buttons
    /// <summary>
    /// Start level button rendering
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    private void BStartLevelShow(Game gameObj)
    {
      if (gameObj.LevelStarted)
      {
        _graphObject.DrawImage(Res.BStartLevelDisabled, Helpers.BuildRect(RectBuilder.NewLevelDisabled, gameObj.Scaling));
      }
      else
      {
        _graphObject.DrawImage(Res.BStartLevelEnabled, Helpers.BuildRect(RectBuilder.NewLevelEnabled, gameObj.Scaling));
      }
    }

    /// <summary>
    /// Destroy tower button rendering
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    private void BDestroyShow(Game gameObj)
    {
      _graphObject.DrawImage(Res.BDestroyTower, Helpers.BuildRect(RectBuilder.Destroy, gameObj.Scaling));
    }

    /// <summary>
    /// Upgrade tower button rendering
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    private void BUpgradeShow(Game gameObj)
    {
      if (!gameObj.Towers[gameObj.TowerMapSelectedID].CanUpgrade) return;
      //Вводится Tmp, т.к этот прямоугольник будет использоваться три раза
      Rectangle tmp = Helpers.BuildRect(RectBuilder.Upgrade, gameObj.Scaling);
      _graphObject.DrawImage(Res.BUpgradeTower, tmp);
      _graphObject.DrawString("Upgrade cost: " + gameObj.Towers[gameObj.TowerMapSelectedID].GetUpgradeCost,
                              new Font("Arial", Settings.ElemSize * gameObj.Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
                              new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling) + 3, tmp.Y - Convert.ToInt32(25 * gameObj.Scaling)));
    }

    #endregion Buttons

    //This region have so small methods, thats for future development
    #region Information for player

    /// <summary>
    /// Shows the lives.
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    private void ShowLives(Game gameObj)
    {
      _graphObject.DrawString("Lives: " + gameObj.NumberOfLives.ToString(CultureInfo.InvariantCulture), new Font("Arial", 14 * gameObj.Scaling), new SolidBrush(Color.Black),
        new Point(Convert.ToInt32((Settings.MapAreaSize + 10 + Settings.DeltaX * 2) * gameObj.Scaling), Convert.ToInt32((Res.MoneyPict.Height + 10) * gameObj.Scaling)));
    }

    /// <summary>
    /// Shows the money.
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    private void ShowMoney(Game gameObj)
    {
      //Money pict
      _graphObject.DrawImage(Res.MoneyPict, Convert.ToInt32((Settings.MapAreaSize + 10 + Settings.DeltaX * 2) * gameObj.Scaling),
        Convert.ToInt32(5 * gameObj.Scaling),
        Convert.ToInt32(Res.MoneyPict.Width * gameObj.Scaling),
        Convert.ToInt32(Res.MoneyPict.Height * gameObj.Scaling));
      //Number of gold
      _graphObject.DrawString(gameObj.Gold.ToString(CultureInfo.InvariantCulture), new Font("Arial", 14 * gameObj.Scaling), new SolidBrush(Color.Black),
        new Point(Convert.ToInt32((Settings.MapAreaSize + 10 + Res.MoneyPict.Width + Settings.DeltaX * 2) * gameObj.Scaling), Convert.ToInt32(9 * gameObj.Scaling)));
    }

    #endregion Information for player
  }
}