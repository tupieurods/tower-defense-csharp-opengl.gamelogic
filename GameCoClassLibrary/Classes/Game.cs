#define Debug

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Interfaces;
using GameCoClassLibrary.Loaders;
using GameCoClassLibrary.Structures;
using Button = GameCoClassLibrary.Enums.Button;

namespace GameCoClassLibrary.Classes
{
  /// <summary>
  /// Main class Game(currently GOD object, will be changed in future)
  /// </summary>
  public sealed class Game
  {
    #region Private Vars

    #region Lists

    /// <summary>
    /// Number of monsters at every level
    /// </summary>
    private readonly List<int> _numberOfMonstersAtLevel;

    /// <summary>
    /// Bonus for successful level finish
    /// </summary>
    private readonly List<int> _goldForSuccessfulLevelFinish;

    /// <summary>
    /// Bonus for monster killing
    /// </summary>
    private readonly List<int> _goldForKillMonster;

    /// <summary>
    /// Tower parametrs
    /// </summary>
    private readonly List<TowerParam> _towerParamsForBuilding;

    /// <summary>
    /// Hashes of tower configuration files
    /// </summary>
    private readonly List<string> _towerConfigsHashes;

    /// <summary>
    /// List of monsters, which were created at level
    /// </summary>
    private readonly List<Monster> _monsters;

    /// <summary>
    /// List of towers, which installed to the map
    /// </summary>
    private readonly List<Tower> _towers;

    /// <summary>
    /// List of created missels
    /// </summary>
    private readonly List<Missle> _missels;

    #endregion Lists

    #region Graphics

    /// <summary>
    /// Scaling factor
    /// </summary>
    private float _gameScale = 1.0F;

    /// <summary>
    /// GraphicEngine object
    /// </summary>
    private readonly GraphicEngine _graphicEngine;

    #endregion Graphics

    #region TowerShop

    /// <summary>
    /// Number of tower, which selected in shop 
    /// </summary>
    private int _towerConfSelectedID = -1;

    /// <summary>
    /// Position on the map, where player want install the tower. This value need + Visible{X|Y}Start
    /// </summary>
    private Point _arrayPosForTowerStanding = new Point(-1, -1);

    /// <summary>
    /// Page, which selected in shop
    /// </summary>
    private int _currentShopPage = 1;

    /// <summary>
    /// Number of pages in shop
    /// </summary>
    private readonly int _pageCount = 1;

    #endregion TowerShop

    #region Monsters

    /// <summary>
    /// Number of created monsters
    /// </summary>
    private int _monstersCreated;

    /// <summary>
    /// Monster configuration for current level
    /// </summary>
    private MonsterParam _currentLevelConf;

    /// <summary>
    /// Position in file with levels configuration
    /// </summary>
    private long _position;

    /// <summary>
    /// System path to file with levels configuration
    /// </summary>
    private readonly string _pathToLevelConfigurations;

    #endregion Monsters

    #region Game Logic

    /// <summary>
    /// Gets a value indicating whether [level started].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [level started]; otherwise, <c>false</c>.
    /// </value>
    private bool LevelStarted { get; set; }

    /// <summary>
    /// Current level number
    /// </summary>
    private int _currentLevelNumber;

    /// <summary>
    /// Number of levels(count)
    /// </summary>
    private readonly int _levelsNumber;

    /// <summary>
    /// Map object
    /// </summary>
    private readonly Map _map;

    /// <summary>
    /// User inreface menu(new level, up/destroy buttons, etc.)
    /// </summary>
    private readonly Menu _uiMenu;

    /// <summary>
    /// Pause state
    /// </summary>
    private bool _paused;

    #endregion Game Logic


    /// <summary>
    /// Number of tower, which selected on the map(ID==position ia array)
    /// </summary>
    private int _towerMapSelectedID = -1;

    #endregion Private Vars

    #region Public

    /// <summary>
    /// Gets a value indicating whether this <see cref="Game"/> is lose.
    /// </summary>
    /// <value>
    ///   <c>true</c> if lose; otherwise, <c>false</c>.
    /// </value>
    public bool Lose { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Game"/> is paused.
    /// </summary>
    /// <value>
    ///   <c>true</c> if paused; otherwise, <c>false</c>.
    /// </value>
    public bool Paused
    {
      get
      {
        return _paused;
      }
      set
      {
        _paused = value;
        _uiMenu.SetRenderState(Button.Unpause, value);
        _uiMenu.SetRenderState(Button.Pause, !value);
      }
    }

    /// <summary>
    /// Gets or sets the scaling.
    /// </summary>
    /// <value>
    /// The scaling.
    /// </value>
    public float Scaling
    {
      get
      {
        return _gameScale;
      }
      set
      {
        _gameScale = value;
        _graphicEngine.RecreateConstantImage(this, value);
        Helpers.BlackPen = new Pen(Color.Black, Settings.PenWidth * value);
        Helpers.GreenPen = new Pen(Color.Green, Settings.PenWidth * value);
        /*foreach (Monster monster in _monsters)
        {
          monster.Scaling = value;
        }
        foreach (Tower tower in _towers)
        {
          tower.Scaling = value;
        }*/
        _uiMenu.Scaling = value;
        _map.Scaling = value;
        Monster.Scaling = value;
        Tower.Scaling = value;
        Missle.Scaling = value;
      }
    }

    #endregion Public

    #region internal

    /// <summary>
    /// Gets the monsters. Read only
    /// </summary>
    internal IList<Monster> Monsters { get { return _monsters.AsReadOnly(); } }

    /// <summary>
    /// Gets the missels. Read only
    /// </summary>
    internal IEnumerable<Missle> Missels { get { return _missels.AsReadOnly(); } }

    /// <summary>
    /// Gets the towers. Read only
    /// </summary>
    internal IList<Tower> Towers { get { return _towers.AsReadOnly(); } }

    /// <summary>
    /// Gets the tower params for building.
    /// </summary>
    internal IList<TowerParam> TowerParamsForBuilding { get { return _towerParamsForBuilding.AsReadOnly(); } }

    /// <summary>
    /// Gets the map.
    /// </summary>
    internal Map Map { get { return _map; } }

    /// <summary>
    /// Gets the tower conf selected ID, which gamer want to build
    /// </summary>
    internal int TowerConfSelectedID { get { return _towerConfSelectedID; } }

    /// <summary>
    /// Gets the tower on map selected ID.
    /// </summary>
    internal int TowerMapSelectedID { get { return _towerMapSelectedID; } }

    /// <summary>
    /// Gets the array position for tower standing.(Tower need 4 array elements, this top left element)
    /// </summary>
    internal Point ArrayPosForTowerStanding { get { return _arrayPosForTowerStanding; } }

    /// <summary>
    /// Gets the current shop page.
    /// </summary>
    internal int CurrentShopPage { get { return _currentShopPage; } }

    /// <summary>
    /// Gets the number of lives.
    /// </summary>
    internal int NumberOfLives { get; private set; }

    /// <summary>
    /// Money of player
    /// </summary>
    internal int Gold { get; private set; }

    internal Point GetUpgradeButtonPos { get { return _uiMenu.GetButtonPosition(Button.UpgradeTower); } }

    #endregion internal

    #region Constructors

    /// <summary>
    /// Prevents a default instance of the <see cref="Game"/> class from being created.
    /// </summary>
    /// <param name="filename">The filename of game configuration</param>
    /// <param name="graphicObject"> IGraphic object</param>
    private Game(string filename, IGraphic graphicObject)
    {
      //Must be initialized in the first place
      LevelStarted = false;
      _paused = false;
      object[] gameSettings;
      //Getting main configuration
      using (BinaryReader loader = new BinaryReader(new FileStream(Environment.CurrentDirectory + "\\Data\\GameConfigs\\" + filename + ".tdgc", FileMode.Open, FileAccess.Read)))
      {
        _pathToLevelConfigurations = Environment.CurrentDirectory + "\\Data\\GameConfigs\\" + filename + ".tdlc";
        SaveNLoad.LoadMainGameConf(loader, out _numberOfMonstersAtLevel, out _goldForSuccessfulLevelFinish, out _goldForKillMonster, out gameSettings);
        loader.Close();
      }
      //Lists initialization
      _monsters = new List<Monster>();
      _towers = new List<Tower>();
      _towerParamsForBuilding = new List<TowerParam>();
      _missels = new List<Missle>();
      //Additional initialization
      Lose = false;
      _levelsNumber = (int)gameSettings[2];
      Gold = (int)gameSettings[4];
      NumberOfLives = (int)gameSettings[5];
      //Map loading
      _map = new Map(Environment.CurrentDirectory + "\\Data\\Maps\\" + Convert.ToString(gameSettings[0]).Substring(Convert.ToString(gameSettings[0]).LastIndexOf('\\')), true);
      #region Loading of tower configurations

      DirectoryInfo diForLoad = new DirectoryInfo(Environment.CurrentDirectory + "\\Data\\Towers\\" + Convert.ToString(gameSettings[1]));
      FileInfo[] towerConfigs = diForLoad.GetFiles();
      _towerConfigsHashes = new List<string>();
      foreach (FileInfo i in towerConfigs.Where(i => i.Extension == ".tdtc"))
      {
        if (_towerParamsForBuilding.Count == 90)//if number of towers>90. Hmm. Bad news for designer
          break;
        using (FileStream towerConfLoadStream = new FileStream(i.FullName, FileMode.Open, FileAccess.Read))
        {
          IFormatter formatter = new BinaryFormatter();
          _towerParamsForBuilding.Add((TowerParam)formatter.Deserialize(towerConfLoadStream));
        }
        _towerConfigsHashes.Add(Helpers.GetMD5ForFile(i.FullName));
      }
      _pageCount = (_towerParamsForBuilding.Count % Settings.ElemSize == 0) ? _towerParamsForBuilding.Count / Settings.ElemSize : (_towerParamsForBuilding.Count / Settings.ElemSize) + 1;

      #endregion Loading of tower configurations
#if Debug
      Gold = 1000;
#endif
      _graphicEngine = new GraphicEngine(graphicObject);
      _uiMenu = new GameUIMenu(graphicObject);
      Scaling = 1F;
    }

    #endregion Constructors

    /// <summary>
    /// Factories the game objects
    /// </summary>
    /// <param name="filename">The filename of game configuration or save file</param>
    /// <param name="act">What factory should to do: Create or Load</param>
    /// <param name="graphicObject">IGraphic object </param>
    /// <returns>Game object or null if error</returns>
    public static Game Factory(string filename, FactoryAct act, IGraphic graphicObject)
    {
      Game result;
      try
      {
        switch (act)
        {
          case FactoryAct.Create:
            result = new Game(filename, graphicObject);
            break;
          case FactoryAct.Load:
            using (BinaryReader loadGameInfo = new BinaryReader(new FileStream(Environment.CurrentDirectory + "\\Data\\SavedGames\\" + filename + ".tdsg", FileMode.Open, FileAccess.Read)))
            {
              result = new Game(loadGameInfo.ReadString(), graphicObject);
              result.Load(loadGameInfo);
            }
            break;
          default:
            throw new ArgumentOutOfRangeException("act");
        }
      }
      catch (Exception exc)
      {
        System.Windows.Forms.MessageBox.Show("Game files damadged: " + exc.Message + "\n" + exc.StackTrace, "Fatal error");
        throw;
      }
      return result;
    }

    #region Communication with player

    /// <summary>
    /// Mouses up event
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    public Button MouseUp(System.Windows.Forms.MouseEventArgs e)
    {
      if (Paused)
      {
        if ((e.Button == MouseButtons.Left) && (_uiMenu.MouseUpCheckOne(e, Button.Unpause)))
        {
          Paused = false;
        }
        return Button.Empty;
      }

      bool flag = false;

      #region Menu Buttons click checking

      Button menuClickResult = _uiMenu.MouseUp(e);
      switch (menuClickResult)
      {
        case Button.Empty:
          break;
        case Button.StartLevelEnabled:
          NewLevelButtonClick();
          return Button.Empty;
          break;
        case Button.StartLevelDisabled:
          return Button.Empty;
          break;
        case Button.DestroyTower:
          DestroyButtonClick();
          return Button.Empty;
          break;
        case Button.UpgradeTower:
          UpgdareButtonClick();
          return Button.Empty;
          break;
        case Button.BigScale:
        case Button.NormalScale:
        case Button.SmallScale:
          return menuClickResult;
          break;
        case Button.Pause:
          Paused = true;
          break;
      }

      #endregion

      #region Tower Page Selection

      if ((_towerParamsForBuilding.Count > Settings.ElemSize) && ((e.X >= Convert.ToInt32((Settings.MapAreaSize + 10 + Settings.DeltaX * 2) * Scaling))
        && (e.Y >= Convert.ToInt32((37 + Res.MoneyPict.Height) * Scaling))
        && (e.Y <= Convert.ToInt32((247 + Res.MoneyPict.Height) * Scaling))))
      {
        // ReSharper disable InconsistentNaming
        flag = ShopPageSelectorAction((int i, int dy, int XMouse, int YMouse) =>
        // ReSharper restore InconsistentNaming
        {
          if (Helpers.LambdaBuildRectPageSelector(this, i, dy).Contains(XMouse, YMouse))
          {
            _currentShopPage = i + 1;
            FinishTowerShopAct();
            return true;
          }
          return false;
        }, e.X, e.Y);
      }

      #endregion Tower Page Selection

      #region Tower Selected in Shop

      if ((!flag) && (e.X >= (Convert.ToInt32((460 + Settings.DeltaX * 2) * Scaling))
        && (e.Y >= Convert.ToInt32((90 + Res.MoneyPict.Height) * Scaling))
        && (e.Y <= Convert.ToInt32((100 + Res.MoneyPict.Height + 42 * ((_towerParamsForBuilding.Count / 5) + 1)) * Scaling))))//Если в границах
      {
        // ReSharper disable InconsistentNaming
        flag = ShopPageAction((int i, int j, int offset, int XMouse, int YMouse) =>
        // ReSharper restore InconsistentNaming
        {
          if (Helpers.LambdaBuildRectPage(this, i, j).Contains(XMouse, YMouse))//Если нашли выделенную башню
          {
            FinishTowerMapSelectAct();
            _towerConfSelectedID = (_currentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset;
            return true;
          }
          return false;
        }, e.X, e.Y);
      }

      #endregion Tower Selected in Shop

      #region Player wants to select the tower on the map

      if ((!flag) && (_towerConfSelectedID == -1)
        && ((e.X >= Settings.DeltaX) && (e.X <= (int)(Settings.MapAreaSize * Scaling) + Settings.DeltaX) && (e.Y >= Settings.DeltaY) && (e.Y <= (int)(Settings.MapAreaSize * Scaling) + Settings.DeltaY)))
      {
        switch (e.Button)
        {
          case System.Windows.Forms.MouseButtons.Left:
            Point arrPos = new Point((e.X - Settings.DeltaX) / Convert.ToInt32(Settings.ElemSize * Scaling),
              (e.Y - Settings.DeltaY) / Convert.ToInt32(Settings.ElemSize * Scaling));
            if (!Check(arrPos, true))
              break;
            if (_map.GetMapElemStatus(arrPos.X + _map.VisibleXStart, arrPos.Y + _map.VisibleYStart) == MapElemStatus.BusyByTower)
            {
              for (int i = 0; i < _towers.Count; i++)
              {
                if (!_towers[i].Contain(new Point(arrPos.X + _map.VisibleXStart, arrPos.Y + _map.VisibleYStart)))
                  continue;
                _towerMapSelectedID = i;
                _uiMenu.SetRenderState(Button.DestroyTower, true);
                if (Towers[TowerMapSelectedID].CanUpgrade)
                  _uiMenu.SetRenderState(Button.UpgradeTower, true);
                //flag = true;
                return Button.Empty;
              }
            }
            break;
          case System.Windows.Forms.MouseButtons.Right:
            if (_towerMapSelectedID != -1)
            {
              FinishTowerMapSelectAct();
            }
            break;
        }
      }

      #endregion Player wants to select the tower on the map

      #region Player wants to build the tower

      if ((!flag) && (_towerConfSelectedID != -1) && (_arrayPosForTowerStanding.X != -1))//Если !=-1 значит в границах карты и Flag=false 100%
      {
        switch (e.Button)
        {
          case System.Windows.Forms.MouseButtons.Left:
            if (Check(_arrayPosForTowerStanding) && (Gold >= _towerParamsForBuilding[_towerConfSelectedID].UpgradeParams[0].Cost))
            {
              _towers.Add(Tower.Factory(FactoryAct.Create, _towerParamsForBuilding[_towerConfSelectedID],
                new Point(_arrayPosForTowerStanding.X + _map.VisibleXStart, _arrayPosForTowerStanding.Y + _map.VisibleYStart), _towerConfigsHashes[_towerConfSelectedID], Scaling));
              Gold -= _towerParamsForBuilding[_towerConfSelectedID].UpgradeParams[0].Cost;
              SetSquareOnMapTo(_arrayPosForTowerStanding, MapElemStatus.BusyByTower);
              FinishTowerShopAct();
            }
            break;
          case System.Windows.Forms.MouseButtons.Right:
            {
              FinishTowerShopAct();
            }
            break;
        }
      }

      #endregion Player wants to build the tower

      return Button.Empty;
    }

    /// <summary>
    /// New level button was clicked.
    /// </summary>
    internal void NewLevelButtonClick()
    {
      if ((Paused) || (LevelStarted) || (_currentLevelNumber >= _levelsNumber)) return;
      LevelStarted = true;
      _currentLevelNumber++;
      _monstersCreated = 0;
      _uiMenu.SetRenderState(Button.StartLevelEnabled, false);
      _uiMenu.SetRenderState(Button.StartLevelDisabled, true);
      //_monsters.Clear();//Useless
      LoadLevel();
    }

    /// <summary>
    /// Upgdare button was clicked.
    /// </summary>
    internal void UpgdareButtonClick()
    {
      if (Paused || _towerMapSelectedID == -1
        || (!_towers[_towerMapSelectedID].CanUpgrade
        || _towers[_towerMapSelectedID].CurrentTowerParams.Cost >= Gold)) return;
      Gold -= _towers[_towerMapSelectedID].Upgrade();
    }

    /// <summary>
    /// Destroy button was clicked.
    /// </summary>
    internal void DestroyButtonClick()
    {
      if (Paused || _towerMapSelectedID == -1) return;
      SetSquareOnMapTo(_towers[_towerMapSelectedID].ArrayPos, MapElemStatus.CanBuild, false);
      _towers.RemoveAt(_towerMapSelectedID);
      FinishTowerMapSelectAct();
    }

    /// <summary>
    /// Loads the level, from file with levels configurations
    /// </summary>
    /// <param name="level">If -1, using _position. Otherwise, the level number to be loaded</param>
    private void LoadLevel(int level = -1)
    {
      #region Level configuration loading

      using (FileStream levelLoadStream = new FileStream(_pathToLevelConfigurations, FileMode.Open, FileAccess.Read))
      {
        IFormatter formatter = new BinaryFormatter();
        if (level == -1)
        {
          levelLoadStream.Seek(_position, SeekOrigin.Begin);
          _currentLevelConf = (MonsterParam)(formatter.Deserialize(levelLoadStream));
        }
        else
        {
          for (int i = 0; i < level; i++)
            _currentLevelConf = (MonsterParam)(formatter.Deserialize(levelLoadStream));
        }
        _position = levelLoadStream.Position;
        levelLoadStream.Close();
      }

      #endregion Level configuration loading

      //Chache for monsters in visible area checking
      if (_currentLevelConf.NumberOfPhases != 0)
      {
        Monster.HalfSizes = new[]
                              {
                                _currentLevelConf[MonsterDirection.Up, 0].Height/2,
                                _currentLevelConf[MonsterDirection.Right, 0].Width/2,
                                _currentLevelConf[MonsterDirection.Down, 0].Height/2,
                                _currentLevelConf[MonsterDirection.Left, 0].Width/2
                              };
      }
    }

    /// <summary>
    /// Vivible map area changing.
    /// </summary>
    /// <param name="position">The position of cursor</param>
    /// <returns>true, if visible area has been changed</returns>
    private bool MapAreaChanging(Point position)
    {
      if (Paused)
        return false;
      if ((_map.Width <= 30) || (_map.Height <= 30))
        return false;
      if (((position.X > Settings.DeltaX) && (position.X < (Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaX)))
        && ((position.Y > Settings.DeltaY) && (position.Y < (Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaY))))
      {
        //TODO Refactor this
        if ((position.X - Settings.DeltaX < Settings.ElemSize) && (_map.VisibleXStart != 0))
        {
          _map.ChangeVisibleArea(-1);
          return true;
        }
        if ((position.Y - Settings.DeltaY < Settings.ElemSize) && (_map.VisibleYStart != 0))
        {
          _map.ChangeVisibleArea(0, -1);
          return true;
        }
        if (((-position.X + Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaX) < Settings.ElemSize) && (_map.VisibleXFinish != _map.Width))
        {
          _map.ChangeVisibleArea(1);
          return true;
        }
        if (((-position.Y + Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaY) < Settings.ElemSize) && (_map.VisibleYFinish != _map.Height))
        {
          _map.ChangeVisibleArea(0, 1);
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Mouse move event
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    public void MouseMove(System.Windows.Forms.MouseEventArgs e)
    {
      if (Paused)
        return;
      #region Player trying to stand the tower(Player searching the place, where he can stand the tower)

      if ((_towerConfSelectedID != -1) && (new Rectangle(Convert.ToInt32(Settings.DeltaX * Scaling), Convert.ToInt32(Settings.DeltaY * Scaling),
        Convert.ToInt32(Settings.MapAreaSize * Scaling), Convert.ToInt32(Settings.MapAreaSize * Scaling)).Contains(e.X, e.Y)))
      {
        _arrayPosForTowerStanding =
          new Point((e.X - Settings.DeltaX) / Convert.ToInt32(Settings.ElemSize * Scaling),
                    (e.Y - Settings.DeltaY) / Convert.ToInt32(Settings.ElemSize * Scaling));
        if (!Check(_arrayPosForTowerStanding, true))
          _arrayPosForTowerStanding = new Point(-1, -1);
      }
      else
        _arrayPosForTowerStanding = new Point(-1, -1);

      #endregion Player trying to stand the tower
    }

    #endregion Communication with player

    #region Game Logic

    #region Action finalizers

    /// <summary>
    /// Finishes the tower map select act.
    /// </summary>
    private void FinishTowerMapSelectAct()
    {
      if (_towerMapSelectedID == -1)
        return;
      _uiMenu.SetRenderState(Button.DestroyTower, false);
      _uiMenu.SetRenderState(Button.UpgradeTower, false);
      //Here we can place messages for player
      _towerMapSelectedID = -1;
    }

    /// <summary>
    /// Tower standed on the map or canceled
    /// </summary>
    private void FinishTowerShopAct()
    {
      _towerConfSelectedID = -1;
      _arrayPosForTowerStanding = new Point(-1, -1);
    }

    #endregion Action finalizers

    /// <summary>
    /// Sets the square on map to status.
    /// </summary>
    /// <param name="leftTopSquarePos">The left top square pos.</param>
    /// <param name="status">The status of map elem.</param>
    /// <param name="addVisibleStart">if set to <c>true</c> add to coords _map.Visible{X|Y}Start.</param>
    private void SetSquareOnMapTo(Point leftTopSquarePos, MapElemStatus status, bool addVisibleStart = true)
    {
      Helpers.TowerSquareCycle(
        (dx, dy) =>
        {
          _map.SetMapElemStatus(
            leftTopSquarePos.X + dx + (addVisibleStart ? _map.VisibleXStart : 0),
            leftTopSquarePos.Y + dy + (addVisibleStart ? _map.VisibleYStart : 0), status);
          return true;
        }, 0);
      /*for (int dx = 0; dx <= 1; dx++)
        for (int dy = 0; dy <= 1; dy++)
          _map.SetMapElemStatus(leftTopSquarePos.X + dx + (addVisibleStart ? _map.VisibleXStart : 0),
                                leftTopSquarePos.Y + dy + (addVisibleStart ? _map.VisibleYStart : 0), status);*/
    }

    //TODO Create class from it
    #region Tower Shop

    /*
     * Why we have that? DRY
     */

    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Shops the page selector action.
    /// </summary>
    /// <param name="act">The act.</param>
    /// <param name="XMouse">The X mouse.</param>
    /// <param name="YMouse">The Y mouse.</param>
    /// <returns>If called for mouse coords checking, returns result of check</returns>
    internal bool ShopPageSelectorAction(Func<int, int, int, int, bool> act, int XMouse = 0, int YMouse = 0)
    // ReSharper restore InconsistentNaming
    {
      int dy = 0;//Will change, if we have more than one line of pages in shop
      for (int i = 0; i < _pageCount; i++)
      {
        if ((i != 0) && (i % 3 == 0))
          dy++;
        if (act(i, dy, XMouse, YMouse))
          return true;
      }
      return false;
    }


    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Shops the page action.
    /// </summary>
    /// <param name="act">The act.</param>
    /// <param name="XMouse">The X mouse.</param>
    /// <param name="YMouse">The Y mouse.</param>
    /// <returns>If called for mouse coords checking, returns result of check</returns>
    internal bool ShopPageAction(Func<int, int, int, int, int, bool> act, int XMouse = 0, int YMouse = 0)
    // ReSharper restore InconsistentNaming
    {
      int towersAtCurrentPage = GetNumberOfTowersAtPage(_currentShopPage);
      int offset = 0;
      for (int j = 0; j < Settings.LinesInOnePage; j++)
      {
        int towersInThisLane = (towersAtCurrentPage - j * Settings.MaxTowersInLine) >= Settings.MaxTowersInLine ?
          Settings.MaxTowersInLine :
          towersAtCurrentPage - j * Settings.MaxTowersInLine;
        for (int i = 0; i < towersInThisLane; i++)
        {
          if (act(i, j, offset, XMouse, YMouse))
            return true;
          offset++;
        }
      }
      return false;
    }

    #endregion Tower Shop


    /// <summary>
    /// Checks, can we stand the tower or not
    /// </summary>
    /// <param name="pos">The position to tower standing</param>
    /// <param name="simple">if set to <c>true</c> fast checking</param>
    /// <returns>Result of the check</returns>
    internal bool Check(Point pos, bool simple = false)
    {
      pos.X += _map.VisibleXStart;
      pos.Y += _map.VisibleYStart;
      if (((pos.X >= 0) && (pos.X < _map.Width - 1)) && ((pos.Y >= 0) && (pos.Y < _map.Height - 1)))
      {
        return simple || Helpers.TowerSquareCycle((dx, dy) => _map.GetMapElemStatus(pos.X + dx, pos.Y + dy) == MapElemStatus.CanBuild, 4);
        /*for (int dx = 0; dx <= 1; dx++)
          for (int dy = 0; dy <= 1; dy++)
          {
            if (_map.GetMapElemStatus(pos.X + dx, pos.Y + dy) != MapElemStatus.CanBuild)//If can't stand, return false
              return false; 
          }*/
      }
      return false;
    }

    /// <summary>
    /// Adds the monster.
    /// </summary>
    private void AddMonster()
    {
      _monsters.Add(new Monster(_currentLevelConf, _map.Way, _monstersCreated, Scaling));
      _monstersCreated++;
    }

    /// <summary>
    /// Gets the number of towers at shop page.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <returns>Number of towers at shop page</returns>
    private int GetNumberOfTowersAtPage(int pageNumber = 1)
    {
      return (_pageCount != pageNumber)
       ? (Settings.LinesInOnePage * Settings.MaxTowersInLine) :
       _towerParamsForBuilding.Count - (pageNumber - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine);
    }

    /// <summary>
    /// Player loses
    /// </summary>
    private void Looser()
    {
      //Currently small method, will be bigger later
      Lose = true;
    }

    /// <summary>
    /// Game timer tick
    /// </summary>
    public void Tick(Point mousePos)
    {
      if (Paused)
        return;
      if (LevelStarted)
      {
        #region Moving of the monsters + True sight

        foreach (Monster monster in _monsters)
        {
          Point tmp = monster.GetArrayPos;
          _map.SetMapElemStatus(tmp.X, tmp.Y, MapElemStatus.CanMove);
          int dx = 0;//The direction of movement of the monster
          int dy = 0;

          #region dx and dy Getting

          switch (monster.GetDirection)
          {
            case MonsterDirection.Up:
              dy = -1;
              break;
            case MonsterDirection.Right:
              dx = 1;
              break;
            case MonsterDirection.Down:
              dy = 1;
              break;
            case MonsterDirection.Left:
              dx = -1;
              break;
          }

          #endregion dx and dy Getting

          if (((tmp.Y + dy <= _map.Height) && (tmp.Y + dy >= 0)) && ((tmp.X + dx <= _map.Width) && (tmp.X + dx >= 0))
            && (_map.GetMapElemStatus(tmp.X + dx, tmp.Y + dy) == MapElemStatus.CanMove))//Fast monsters are blocking slow monsters
            monster.Move(true);//Move
          else
            monster.Move(false);//Stop
          if (_currentLevelConf.Base.Invisible)
            if (Towers.Where(x => x.TrueSight).Any(tower => tower.InAttackRadius(monster.GetCanvaPos.X, monster.GetCanvaPos.Y)))
            {
              monster.MakeVisible();
            }
          if (monster.NewLap)//If the monster went around the whole map
          {
            monster.NewLap = false;
            NumberOfLives--;
            if (NumberOfLives <= 0)
            {
              Looser();
              return;
            }
          }
          tmp = monster.GetArrayPos;
          _map.SetMapElemStatus(tmp.X, tmp.Y, MapElemStatus.BusyByUnit);
        }

        #endregion Moving of the monsters + True sight

        #region Missiles adding

        foreach (Tower tower in _towers)
          _missels.AddRange(tower.GetAims(_monsters.Where(elem => elem.Visible)));

        #endregion Missiles adding

        #region Moving of the missiles

        foreach (Missle missle in Missels.Where(missle => !missle.DestroyMe))
          missle.Move(Monsters);

        #endregion

        #region New monster adding

        if ((_monstersCreated != _numberOfMonstersAtLevel[_currentLevelNumber - 1]) && (_map.GetMapElemStatus(_map.Way[0].X, _map.Way[0].Y) == MapElemStatus.CanMove))
        {
          AddMonster();
          _map.SetMapElemStatus(_map.Way[0].X, _map.Way[0].Y, MapElemStatus.BusyByUnit);
        }

        if ((_monstersCreated == _numberOfMonstersAtLevel[_currentLevelNumber - 1]) && (_monsters.Count == 0))
        {
          LevelStarted = false;
          _uiMenu.SetRenderState(Button.StartLevelEnabled, true);
          _uiMenu.SetRenderState(Button.StartLevelDisabled, false);
          Gold += _goldForSuccessfulLevelFinish[_currentLevelNumber - 1];
          if (_currentLevelNumber == _levelsNumber)
          {
            System.Windows.Forms.MessageBox.Show("Congratulations! You won this game.");
          }
        }

        #endregion New monster adding
      }

      //This code placed here for a smooth moving of visible map area, when it changing
      if (System.Windows.Forms.Control.MouseButtons == System.Windows.Forms.MouseButtons.Middle)
        if (MapAreaChanging(mousePos))
          _graphicEngine.RepaintConstImage = true;

      #region Useless objects removing (for example,missle killed the monster )

      Predicate<Monster> predicate = monster =>
                                        {
                                          if (monster.DestroyMe)
                                          {
                                            Gold += _goldForKillMonster[_currentLevelNumber - 1];
                                            Map.SetMapElemStatus(monster.GetArrayPos.X, monster.GetArrayPos.Y, MapElemStatus.CanMove);
                                            return true;
                                          }
                                          return false;
                                        };
      _monsters.RemoveAll(predicate);
      _missels.RemoveAll(missle => missle.DestroyMe);

      #endregion Useless objects removing (for example,missle killed the monster )
    }

    /// <summary>
    /// Renders the game window
    /// </summary>
    public void Render()
    {
      _graphicEngine.Show(this);
      _uiMenu.Show();
    }

    #endregion Game Logic

    /// <summary>
    /// Saves the game.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public void SaveGame(string fileName)
    {
      using (BinaryWriter saveStream = new BinaryWriter(new FileStream(Environment.CurrentDirectory + "\\Data\\SavedGames\\" + fileName + ".tdsg", FileMode.Create, FileAccess.Write)))
      {
        //Don't forget, that _pathToLevelConfigurations is a full path to levels configuration file
        string confFileNameWithExtension = _pathToLevelConfigurations.Substring(_pathToLevelConfigurations.LastIndexOf("\\", StringComparison.Ordinal));//Name of configuration file with extension
        saveStream.Write(confFileNameWithExtension.Substring(0, confFileNameWithExtension.Length - 5));//Remove extension and write to file
        saveStream.Write(_map.VisibleXStart);//Map position
        saveStream.Write(_map.VisibleYStart);
        saveStream.Write(_gameScale);//Scaling, saves for future
        saveStream.Write(_towerConfSelectedID);
        saveStream.Write(_towerMapSelectedID);
        saveStream.Write(_monstersCreated);
        saveStream.Write(_currentShopPage);
        saveStream.Write(_currentLevelNumber);
        saveStream.Write(Gold);
        saveStream.Write(NumberOfLives);
        saveStream.Write(LevelStarted);
        //saveStream.Write(true);//(Paused);
        //Hashes of tower configurations
        saveStream.Write(_towerConfigsHashes.Count);
        _towerConfigsHashes.ForEach(saveStream.Write);
        //Monster section
        //Count
        saveStream.Write(_monsters.Count(x => !x.DestroyMe));
        //Monsters list
        _monsters.ForEach(x => x.Save(saveStream));
        //Tower section
        //Count
        saveStream.Write(_towers.Count);
        //Towers list
        _towers.ForEach(x => x.Save(saveStream));
        //Missels section
        //Count
        saveStream.Write(_missels.Count(x => !x.DestroyMe));
        //Missels list
        _missels.ForEach(x => x.Save(saveStream));
      }
    }

    /// <summary>
    /// Loads saved game
    /// </summary>
    /// <param name="loadStream">The load stream.</param>
    public void Load(BinaryReader loadStream)
    {
      _map.ChangeVisibleArea(loadStream.ReadInt32(), loadStream.ReadInt32());
      _gameScale = loadStream.ReadSingle();//Scale
      _towerConfSelectedID = loadStream.ReadInt32();
      _towerMapSelectedID = loadStream.ReadInt32();
      _monstersCreated = loadStream.ReadInt32();
      _currentShopPage = loadStream.ReadInt32();
      _currentLevelNumber = loadStream.ReadInt32();
      Gold = loadStream.ReadInt32();
      NumberOfLives = loadStream.ReadInt32();
      LevelStarted = loadStream.ReadBoolean();
      Paused = true;//loadStream.ReadBoolean();
      int n = loadStream.ReadInt32();
      if (n != _towerConfigsHashes.Count)
        throw new Exception("Tower configration damadged");
      for (int i = 0; i < n; i++)
      {
        if (!_towerConfigsHashes[i].Equals(loadStream.ReadString(), StringComparison.InvariantCulture))
          throw new Exception("Tower configration damadged");
      }
      //Monster section
      n = loadStream.ReadInt32();
      LoadLevel(_currentLevelNumber);
      for (int i = 0; i < n; i++)
      {
        _monsters.Add(new Monster(_currentLevelConf, _map.Way, -1, _gameScale));
        _monsters[_monsters.Count - 1].Load(loadStream);
      }
      //Tower section
      n = loadStream.ReadInt32();
      for (int i = 0; i < n; i++)
      {
        string hash = loadStream.ReadString();
        _towers.Add(Tower.Factory(FactoryAct.Load, _towerParamsForBuilding[_towerConfigsHashes.IndexOf(hash)], new Point(loadStream.ReadInt32(), loadStream.ReadInt32()), hash, _gameScale, loadStream));
        SetSquareOnMapTo(_towers[_towers.Count - 1].ArrayPos, MapElemStatus.BusyByTower);
      }
      //Missels section
      n = loadStream.ReadInt32();
      for (int i = 0; i < n; i++)
        _missels.Add(Missle.Factory(FactoryAct.Load, loadStream));
    }

  }
}