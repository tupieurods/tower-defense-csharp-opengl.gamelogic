#define Debug

using System;
using System.Drawing;
using System.Linq;
using System.Globalization;
using GameCoClassLibrary.Interfaces;
using GameCoClassLibrary.Loaders;

namespace GameCoClassLibrary.Classes
{
  //About Magic numbers, they are all documented on the list of paper
  internal sealed class GraphicEngine
  {

    #region Private vars
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

    #endregion

    /// <summary>
    /// Gets or sets a value indicating need or not to call Map.GetConstantBitmap metho.
    /// </summary>
    internal bool RepaintConstImage { private get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicEngine"/> class.
    /// </summary>
    /// <param name="graphObject">The graph object.</param>
    internal GraphicEngine(IGraphic graphObject)
    {
      _graphObject = graphObject;
    }

    /// <summary>
    /// Gets the graphObject.
    /// </summary>
    /// <returns></returns>
    internal IGraphic GetGraphObject()
    {
      return _graphObject;
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
      _graphObject.FillRectangle(new SolidBrush(_backgroundColor), 0, 0,
        Convert.ToInt32(Settings.WindowWidth * gameObj.Scaling),
        Convert.ToInt32(Settings.WindowHeight * gameObj.Scaling));
      //Show map area
      MapAreaShowing(gameObj);
#if Debug
      _graphObject.DrawString(gameObj.Monsters.Count.ToString(CultureInfo.InvariantCulture), new Font("Arial", Settings.ElemSize),
        new SolidBrush(Color.Black), new Point(0, 0));
#endif

      #region Controls

      {
        //The line of breakup
        _graphObject.DrawLine(new Pen(new SolidBrush(Color.White), Settings.PenWidth * gameObj.Scaling),
          new Point(Convert.ToInt32(Settings.BreakupLineXPosition * gameObj.Scaling), 0),
          new Point(Convert.ToInt32(Settings.BreakupLineXPosition * gameObj.Scaling), Convert.ToInt32(Settings.WindowHeight * gameObj.Scaling)));
        ShowMoney(gameObj);//Gold
        ShowLives(gameObj);//lives
        if ((gameObj.TowerConfSelectedID != -1) || (gameObj.TowerMapSelectedID != -1))//If needs to show tower params
          ShowTowerParams(gameObj);
      }

      #endregion

      //Will be removed later(May be), useless thing, change picture when game paused
      if (gameObj.Paused)
        _graphObject.MakeGray(0, 0, Convert.ToInt32(Settings.WindowWidth * gameObj.Scaling), Convert.ToInt32(Settings.WindowHeight * gameObj.Scaling));
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
      _graphObject.Clip = new Region(new Rectangle(Convert.ToInt32(Settings.DeltaX * gameObj.Scaling), Convert.ToInt32(Settings.DeltaY * gameObj.Scaling),
                                            Convert.ToInt32((gameObj.Map.VisibleXFinish - gameObj.Map.VisibleXStart) * Settings.ElemSize * gameObj.Scaling),
                                            Convert.ToInt32((gameObj.Map.VisibleYFinish - gameObj.Map.VisibleYStart) * Settings.ElemSize * gameObj.Scaling)));
      //Map showing
      _graphObject.DrawImage(_constantMapImage, Convert.ToInt32(Settings.DeltaX * gameObj.Scaling), Convert.ToInt32(Settings.DeltaY * gameObj.Scaling), _constantMapImage.Width, _constantMapImage.Height);
      Point visibleStart = new Point(gameObj.Map.VisibleXStart, gameObj.Map.VisibleYStart);
      Point visibleFinish = new Point(gameObj.Map.VisibleXFinish, gameObj.Map.VisibleYFinish);
      //Towers showing
      foreach (Tower tower in gameObj.Towers)
        tower.ShowTower(_graphObject, visibleStart, visibleFinish);
      //Monsters showing
      foreach (Monster monster in gameObj.Monsters.Where(monster => !monster.DestroyMe))
        monster.ShowMonster(_graphObject, visibleStart, visibleFinish);

      #region Showing square and circle around selected tower or around the tower, which player want to stand

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

      #endregion Showing square and circle around selected tower or around the tower, which player want to stand

      //Missle showing
      foreach (Missle missle in gameObj.Missels.Where(missle => !missle.DestroyMe))
        missle.Show(_graphObject, visibleStart, visibleFinish, gameObj.Monsters);
      _graphObject.Clip = new Region();
    }

    #region Tower Sector

    /// <summary>
    /// Shows the tower params.
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    //TODO: remove magic
    private void ShowTowerParams(Game gameObj)
    {
      string strToShow = "";
      int xLeftPos = Convert.ToInt32(Settings.TowerParamsPos.X * gameObj.Scaling);
      int yLeftPos = Convert.ToInt32(Settings.TowerParamsPos.Y * gameObj.Scaling);
      //Information about tower, which player want to build
      if (gameObj.TowerConfSelectedID != -1)
      {
        strToShow =
          gameObj.TowerParamsForBuilding[gameObj.TowerConfSelectedID]
          + gameObj.TowerParamsForBuilding[gameObj.TowerConfSelectedID].UpgradeParams[0].ToString();
        _graphObject.DrawString(
          "Cost: " + gameObj.TowerParamsForBuilding[gameObj.TowerConfSelectedID].UpgradeParams[0].Cost,
          new Font("Arial", 16 * gameObj.Scaling, FontStyle.Italic | FontStyle.Bold),
          new SolidBrush(Color.Black),
          new Point(xLeftPos, Convert.ToInt32(yLeftPos - Settings.DeltaY * 2.5 * gameObj.Scaling)));
      }
      //Information about tower, which player selected on the map
      if (gameObj.TowerMapSelectedID != -1)
      {
        strToShow = gameObj.Towers[gameObj.TowerMapSelectedID].ToString();
        //Icon
        _graphObject.DrawImage(
          gameObj.Towers[gameObj.TowerMapSelectedID].Icon,
          xLeftPos,
          Convert.ToInt32(yLeftPos - (Settings.TowerIconSize + Settings.DeltaY / 2.0) * gameObj.Scaling),
          Convert.ToInt32(gameObj.Towers[gameObj.TowerMapSelectedID].Icon.Width * gameObj.Scaling),
          Convert.ToInt32(gameObj.Towers[gameObj.TowerMapSelectedID].Icon.Height * gameObj.Scaling));
        //Current tower level
        _graphObject.DrawString(
          "Level: " + gameObj.Towers[gameObj.TowerMapSelectedID].Level.ToString(CultureInfo.InvariantCulture),
          new Font("Arial", 16 * gameObj.Scaling, FontStyle.Italic | FontStyle.Bold),
          new SolidBrush(Color.Black),
          new Point(
            Convert.ToInt32(xLeftPos + gameObj.Towers[gameObj.TowerMapSelectedID].Icon.Width * gameObj.Scaling),
            Convert.ToInt32(yLeftPos - Settings.DeltaY * 2.5 * gameObj.Scaling)));
        //Upgrading cost
        ShowUpgradeCost(gameObj);
      }
      //Parametrs
      _graphObject.DrawString(
          strToShow,
          new Font("Arial", 10 * gameObj.Scaling, FontStyle.Italic | FontStyle.Bold),
          new SolidBrush(Color.Black),
          new Point(xLeftPos, yLeftPos));
      //Border line
      _graphObject.DrawRectangle(
        new Pen(Color.Black),
        xLeftPos,
        yLeftPos,
        Convert.ToInt32((200 - Settings.DeltaX * 2) * gameObj.Scaling),
        Convert.ToInt32((Settings.WindowHeight - Settings.DeltaX / 2) * gameObj.Scaling - yLeftPos));
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
          Convert.ToInt32((position.X * Settings.ElemSize + Settings.DeltaX) * gameObj.Scaling),
          Convert.ToInt32((position.Y * Settings.ElemSize + Settings.DeltaY) * gameObj.Scaling),
          Convert.ToInt32(Settings.ElemSize * 2 * gameObj.Scaling),
          Convert.ToInt32(Settings.ElemSize * 2 * gameObj.Scaling));
      //Circle
      //+1 for position centering
      _graphObject.DrawEllipse(
        new Pen(circleColor),
        ((position.X + 1) * Settings.ElemSize - radius) * gameObj.Scaling + Settings.DeltaX,
        ((position.Y + 1) * Settings.ElemSize - radius) * gameObj.Scaling + Settings.DeltaY,
        radius * 2 * gameObj.Scaling, radius * 2 * gameObj.Scaling);
    }
    #endregion Tower Sector

    //This region have so small methods, thats for future development
    #region Information for player

    /// <summary>
    /// Shows the lives.
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    private void ShowLives(Game gameObj)
    {
      _graphObject.DrawString(
        "Lives: " + gameObj.NumberOfLives.ToString(CultureInfo.InvariantCulture),
        new Font("Arial", Settings.FontSize * gameObj.Scaling),
        new SolidBrush(Color.Black),
        new Point(
          Convert.ToInt32((Settings.BreakupLineXPosition + Settings.DeltaX) * gameObj.Scaling),
          Convert.ToInt32((Res.MoneyPict.Height + Settings.DeltaX) * gameObj.Scaling)));
    }

    /// <summary>
    /// Shows the money.
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    private void ShowMoney(Game gameObj)
    {
      //Money pict
      _graphObject.DrawImage(Res.MoneyPict,
        Convert.ToInt32((Settings.BreakupLineXPosition + Settings.DeltaX) * gameObj.Scaling),
        Convert.ToInt32(Settings.MoneyYPos * gameObj.Scaling),
        Convert.ToInt32(Res.MoneyPict.Width * gameObj.Scaling),
        Convert.ToInt32(Res.MoneyPict.Height * gameObj.Scaling));
      //Number of gold
      _graphObject.DrawString(gameObj.Gold.ToString(CultureInfo.InvariantCulture),
        new Font("Arial", 14 * gameObj.Scaling),
        new SolidBrush(Color.Black),
        new Point(
          Convert.ToInt32((Settings.BreakupLineXPosition + Res.MoneyPict.Width + Settings.DeltaX) * gameObj.Scaling),
          Convert.ToInt32(Settings.DeltaY * gameObj.Scaling)));
    }

    /// <summary>
    /// Shows cost for tower upgrading
    /// </summary>
    /// <param name="gameObj">The game obj.</param>
    private void ShowUpgradeCost(Game gameObj)
    {
      if (!gameObj.Towers[gameObj.TowerMapSelectedID].CanUpgrade) return;
      _graphObject.DrawString(
        "Upgrade cost: " + gameObj.Towers[gameObj.TowerMapSelectedID].GetUpgradeCost,
        new Font("Arial", 16 * gameObj.Scaling,
          FontStyle.Italic | FontStyle.Bold),
        new SolidBrush(Color.Black),
        new Point(
          Convert.ToInt32((Settings.BreakupLineXPosition + Settings.DeltaX) * gameObj.Scaling),
            gameObj.GetUpgradeButtonPos.Y - Convert.ToInt32(25 * gameObj.Scaling)));
    }

    #endregion Information for player
  }
}