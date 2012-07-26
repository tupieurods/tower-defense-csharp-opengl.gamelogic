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
using GameCoClassLibrary.Forms;
using GameCoClassLibrary.Interfaces;
using GameCoClassLibrary.Loaders;
using GameCoClassLibrary.Properties;
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
    /// Position on the map, where player want install the tower. This value need + Visible{X|Y}Start
    /// </summary>
    private Point _arrayPosForTowerStanding = new Point(-1, -1);

    /// <summary>
    /// Tower shop object
    /// </summary>
    private readonly TowerShop _towerShop;

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
    /// Pause menu object
    /// </summary>
    private Menu _pauseMenu;

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
    /// Gets or sets a value indicating whether this <see cref="Game"/> is won.
    /// </summary>
    /// <value>
    ///   <c>true</c> if won; otherwise, <c>false</c>.
    /// </value>
    public bool Won { get; private set; }

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
      private set
      {
        _paused = value;
        if (_pauseMenu != null) return;
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
        if (_pauseMenu != null)
          _pauseMenu.Scaling = value;
        _map.Scaling = value;
        Shop.Scaling = value;
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
    internal int TowerConfSelectedID
    {
      get { return _towerShop.ConfSelectedID; }
      private set { _towerShop.ConfSelectedID = value; }
    }

    /// <summary>
    /// Gets the tower on map selected ID.
    /// </summary>
    internal int TowerMapSelectedID { get { return _towerMapSelectedID; } }

    /// <summary>
    /// Gets the array position for tower standing.(Tower need 4 array elements, this top left element)
    /// </summary>
    internal Point ArrayPosForTowerStanding { get { return _arrayPosForTowerStanding; } }

    /*/// <summary>
    /// Gets the current shop page.
    /// </summary>
    internal int CurrentShopPage { get { return _towerShop.CurrentShopPage; } }*/

    /// <summary>
    /// Gets the number of lives.
    /// </summary>
    internal int NumberOfLives { get; private set; }

    /// <summary>
    /// Money of player
    /// </summary>
    internal int Gold { get; private set; }

    /// <summary>
    /// Gets the get upgrade button pos.
    /// </summary>
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
      var iconsForShop = new List<Bitmap>();
      foreach (FileInfo i in towerConfigs.Where(i => i.Extension == ".tdtc"))
      {
        if (_towerParamsForBuilding.Count == 90)//if number of towers>90. Hmm. Bad news for designer
          break;
        using (FileStream towerConfLoadStream = new FileStream(i.FullName, FileMode.Open, FileAccess.Read))
        {
          IFormatter formatter = new BinaryFormatter();
          _towerParamsForBuilding.Add((TowerParam)formatter.Deserialize(towerConfLoadStream));
          iconsForShop.Add(_towerParamsForBuilding.Last().Icon);
        }
        _towerConfigsHashes.Add(Helpers.GetMD5ForFile(i.FullName));
      }
      /*_pageCount = (_towerParamsForBuilding.Count % Settings.ElemSize == 0) ? _towerParamsForBuilding.Count / Settings.ElemSize : (_towerParamsForBuilding.Count / Settings.ElemSize) + 1;*/
      _towerShop = new TowerShop(iconsForShop.AsReadOnly(), Settings.TowerShopPageSelectorPos, Settings.TowerShopPagePos);
      /*var lol = new List<Bitmap>(IconsForShop.AsReadOnly());
      lol.Clear();*/

      #endregion Loading of tower configurations
#if Debug
      Gold = 1000;
#endif
      _graphicEngine = new GraphicEngine(graphicObject);
      _uiMenu = new GameUIMenu(graphicObject);
      _pauseMenu = null;
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
      catch (Exception)
      {
        MessageBox.Show(Resources.Game_files_damadged, Resources.Fatal_error);
        throw;
      }
      return result;
    }

    #region Communication with player

    /// <summary>
    /// Mouses up event
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    /// <returns>Button whic was clicked. Button.Empty if click handeled in this function or this click doesn't bring inforamtion</returns>
    public Button MouseUp(MouseEventArgs e)
    {
      //Pause Checking
      if (Paused && _pauseMenu == null)
      {
        if ((e.Button == MouseButtons.Left) && (_uiMenu.MouseUpCheckOne(e, Button.Unpause)))
        {
          Paused = false;
        }
        if ((e.Button == MouseButtons.Left) && (_uiMenu.MouseUpCheckOne(e, Button.Menu)))
        {
          MenuButtonClick();
        }
        return Button.Empty;
      }

      //Menu Buttons click checking
      Button clickResult;
      if (MenuClickChecking(e, out clickResult))
        return clickResult;

      //Tower shop click
      if (TowerShopClickChecking(e))
        return Button.Empty;

      //Player wants to select the tower on the map
      if (MapTowerClickChecking(e))
        return Button.Empty;

      //Player wants to build the tower
      if (TowerBuildingClickChecking(e))
        return Button.Empty;

      return Button.Empty;
    }

    #region Checks
    /// <summary>
    /// Checks was user click on menu elemenbt or not
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    /// <param name="button">Clicked button</param>
    /// <returns>True if click was on menu button</returns>
    private bool MenuClickChecking(MouseEventArgs e, out Button button)
    {
      Button menuClickResult = _pauseMenu == null ? _uiMenu.MouseUp(e) : _pauseMenu.MouseUp(e);
      switch (menuClickResult)
      {
        case Button.Empty:
          break;
        case Button.StartLevelEnabled:
          {
            NewLevelButtonClick();
            button = Button.Empty;
            return true;
          }
        case Button.StartLevelDisabled:
          {
            button = Button.Empty;
            return true;
          }
        case Button.DestroyTower:
          {
            DestroyButtonClick();
            button = Button.Empty;
            return true;
          }
        case Button.UpgradeTower:
          {
            UpgdareButtonClick();
            button = Button.Empty;
            return true;
          }
        case Button.BigScale:
        case Button.NormalScale:
        case Button.SmallScale:
          {
            button = menuClickResult;
            return true;
          }
        case Button.Menu:
          {
            MenuButtonClick();
            button = Button.Empty;
            return true;
          }
        case Button.Pause:
          {
            Paused = true;
            break;
          }
        //Next for pause menu only
        case Button.SaveGame:
          {
            SaveButtonClick();
            break;
          }
        case Button.NewGame:
        case Button.LoadGame:
        case Button.Exit:
          {
            button = menuClickResult;
            return true;
          }
        case Button.Back:
          {
            _pauseMenu = null;
            Paused = false;
            break;
          }
      }
      button = Button.Empty;
      return false;
    }

    /// <summary>
    /// Checks user click coords when tower selected on shop and build towert if needed
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    /// <returns>True if tower build or building canceled</returns>
    private bool TowerBuildingClickChecking(MouseEventArgs e)
    {
      if ((TowerConfSelectedID == -1) || (_arrayPosForTowerStanding.X == -1))
        return false;
      switch (e.Button)
      {
        case MouseButtons.Left:
          if (Check(_arrayPosForTowerStanding)
              && (Gold >= _towerParamsForBuilding[TowerConfSelectedID].UpgradeParams[0].Cost))
          {
            _towers.Add(Tower.Factory(FactoryAct.Create, _towerParamsForBuilding[TowerConfSelectedID],
                                      new Point(_arrayPosForTowerStanding.X + _map.VisibleXStart,
                                                _arrayPosForTowerStanding.Y + _map.VisibleYStart),
                                      _towerConfigsHashes[TowerConfSelectedID], Scaling));
            Gold -= _towerParamsForBuilding[TowerConfSelectedID].UpgradeParams[0].Cost;
            ChangeMapElementStatus(_arrayPosForTowerStanding, MapElemStatus.BusyByTower);
            FinishTowerShopAct();
          }
          return true;
        case MouseButtons.Right:
          FinishTowerShopAct();
          return true;
      }
      return false;
    }

    /// <summary>
    /// Checks for tower under click coords
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    /// <returns>True if click was on tower or selection removed</returns>
    private bool MapTowerClickChecking(MouseEventArgs e)
    {
      if (TowerConfSelectedID == -1
          && ((e.X >= Convert.ToInt32(Settings.DeltaX * Scaling))
              && (e.X <= Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX) * Scaling))
              && (e.Y >= Convert.ToInt32(Settings.DeltaY * Scaling))
              && e.Y <= Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaY) * Scaling)))
      {
        switch (e.Button)
        {
          case MouseButtons.Left:
            Point arrPos =
              new Point((e.X - Convert.ToInt32(Settings.DeltaX * Scaling)) / Convert.ToInt32(Settings.ElemSize * Scaling),
                        (e.Y - Convert.ToInt32(Settings.DeltaY * Scaling)) / Convert.ToInt32(Settings.ElemSize * Scaling));
            if (!Check(arrPos, true))
              break;
            if (_map.GetMapElemStatus(arrPos.X + _map.VisibleXStart, arrPos.Y + _map.VisibleYStart) ==
                MapElemStatus.BusyByTower)
            {
              for (int i = 0; i < _towers.Count; i++)
              {
                if (!_towers[i].Contain(new Point(arrPos.X + _map.VisibleXStart, arrPos.Y + _map.VisibleYStart)))
                  continue;
                _towerMapSelectedID = i;
                _uiMenu.SetRenderState(Button.DestroyTower, true);
                if (Towers[TowerMapSelectedID].CanUpgrade)
                  _uiMenu.SetRenderState(Button.UpgradeTower, true);
                return true;
              }
            }
            break;
          case MouseButtons.Right:
            if (_towerMapSelectedID != -1)
            {
              FinishTowerMapSelectAct();
              return true;
            }
            break;
        }
      }
      return false;
    }

    /// <summary>
    /// Tower shop click event
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    /// <returns>true if click changing tower shop statement</returns>
    private bool TowerShopClickChecking(MouseEventArgs e)
    {
      ShopActStatus status;
      _towerShop.MouseUp(e, out status);
      switch (status)
      {
        case ShopActStatus.Normal:
          return false;
        case ShopActStatus.ShopActFinish:
          FinishTowerShopAct();
          return true;
        case ShopActStatus.MapActFinish:
          FinishTowerMapSelectAct();
          return true;
        default:
          throw new ArgumentOutOfRangeException();
      }
      //return false;
    }
    #endregion

    #region Actions
    /// <summary>
    /// Save button click handler
    /// </summary>
    private void SaveButtonClick()
    {
      //Paused = true;
      FormForSave saveNameForm = new FormForSave();
      if (saveNameForm.ShowDialog() == DialogResult.OK)
      {
        SaveGame(saveNameForm.ReturnSaveFileName());
        MessageBox.Show(Resources.SaveStatusSuccess);
      }
      //Paused = false;
    }

    /// <summary>
    /// Menu button click handler
    /// </summary>
    private void MenuButtonClick()
    {
      Paused = true;
      _pauseMenu = new PauseMenu(_graphicEngine.GetGraphObject()) { Scaling = Scaling };
    }

    /// <summary>
    /// New level button was clicked.
    /// </summary>
    private void NewLevelButtonClick()
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
    private void UpgdareButtonClick()
    {
      if (Paused || _towerMapSelectedID == -1
        || (!_towers[_towerMapSelectedID].CanUpgrade
        || _towers[_towerMapSelectedID].CurrentTowerParams.Cost >= Gold)) return;
      Gold -= _towers[_towerMapSelectedID].Upgrade();
    }

    /// <summary>
    /// Destroy button was clicked.
    /// </summary>
    private void DestroyButtonClick()
    {
      if (Paused || _towerMapSelectedID == -1) return;
      ChangeMapElementStatus(_towers[_towerMapSelectedID].ArrayPos, MapElemStatus.CanBuild, false);
      _towers.RemoveAt(_towerMapSelectedID);
      FinishTowerMapSelectAct();
    }
    #endregion

    /// <summary>
    /// Visible map area changing.
    /// </summary>
    /// <param name="position">The position of cursor</param>
    /// <returns>true, if visible area has been changed</returns>
    private bool MapAreaChanging(Point position)
    {
      if (Paused)
        return false;
      if ((_map.Width <= 30) || (_map.Height <= 30))
        return false;
      if (position.X > Convert.ToInt32(Settings.DeltaX * Scaling)
          && position.X < Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX) * Scaling)
          && position.Y > Convert.ToInt32(Settings.DeltaY * Scaling)
          && position.Y < Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaY) * Scaling))
      {
        if ((position.X - Convert.ToInt32(Settings.DeltaX * Scaling) < Settings.ElemSize) && (_map.VisibleXStart != 0))
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
    public void MouseMove(MouseEventArgs e)
    {
      if (Paused)
        return;

      #region Player trying to stand the tower(Player searching the place, where he can stand the tower)

      if ((TowerConfSelectedID != -1)
        && (
        new Rectangle(
          Convert.ToInt32(Settings.DeltaX * Scaling),
          Convert.ToInt32(Settings.DeltaY * Scaling),
          Convert.ToInt32(Settings.MapAreaSize * Scaling),
          Convert.ToInt32(Settings.MapAreaSize * Scaling)).Contains(e.X, e.Y)))
      {
        _arrayPosForTowerStanding = new Point(
            (e.X - Convert.ToInt32(Settings.DeltaX * Scaling)) / Convert.ToInt32(Settings.ElemSize * Scaling),
            (e.Y - Convert.ToInt32(Settings.DeltaY * Scaling)) / Convert.ToInt32(Settings.ElemSize * Scaling));
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
      TowerConfSelectedID = -1;
      _arrayPosForTowerStanding = new Point(-1, -1);
    }

    #endregion Action finalizers

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
    /// Sets the square on map to status.
    /// </summary>
    /// <param name="leftTopSquarePos">The left top square pos.</param>
    /// <param name="status">The status of map elem.</param>
    /// <param name="addVisibleStart">if set to <c>true</c> add to coords _map.Visible{X|Y}Start.</param>
    private void ChangeMapElementStatus(Point leftTopSquarePos, MapElemStatus status, bool addVisibleStart = true)
    {
      Helpers.TowerSquareCycle(
        (dx, dy) =>
        {
          _map.SetMapElemStatus(
            leftTopSquarePos.X + dx + (addVisibleStart ? _map.VisibleXStart : 0),
            leftTopSquarePos.Y + dy + (addVisibleStart ? _map.VisibleYStart : 0), status);
          return true;
        }, 0);
    }

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
        return
          simple
          || Helpers.TowerSquareCycle((dx, dy) => _map.GetMapElemStatus(pos.X + dx, pos.Y + dy) == MapElemStatus.CanBuild, 4);
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
    /// Player lose
    /// </summary>
    private void Looser()
    {
      //Currently small method, will be bigger later
      Lose = true;
    }

    /// <summary>
    /// Player win
    /// </summary>
    private void Winner()
    {
      //Currently small method, will be bigger later
      Won = true;
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
            Winner();
        }

        #endregion New monster adding
      }

      //This code placed here for a smooth moving of visible map area, when it changing
      if (Control.MouseButtons == MouseButtons.Middle)
        if (MapAreaChanging(mousePos))
          _graphicEngine.RepaintConstImage = true;

      #region Useless objects removing (for example: dead monsters )

      Predicate<Monster> predicate =
        monster =>
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
      _towerShop.Show(_graphicEngine.GetGraphObject());
      if (_pauseMenu == null)
        _uiMenu.Show();
      else
        _pauseMenu.Show();
    }

    #endregion Game Logic

    /// <summary>
    /// Saves the game.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    private void SaveGame(string fileName)
    {
      using (BinaryWriter saveStream = new BinaryWriter(new FileStream(Environment.CurrentDirectory + "\\Data\\SavedGames\\" + fileName + ".tdsg", FileMode.Create, FileAccess.Write)))
      {
        //Don't forget, that _pathToLevelConfigurations is a full path to levels configuration file
        string confFileNameWithExtension = _pathToLevelConfigurations.Substring(_pathToLevelConfigurations.LastIndexOf("\\", StringComparison.Ordinal));//Name of configuration file with extension
        saveStream.Write(confFileNameWithExtension.Substring(0, confFileNameWithExtension.Length - 5));//Remove extension and write to file
        saveStream.Write(_map.VisibleXStart);//Map position
        saveStream.Write(_map.VisibleYStart);
        saveStream.Write(_gameScale);//Scaling, saves for future
        saveStream.Write(TowerConfSelectedID);
        saveStream.Write(_towerMapSelectedID);
        saveStream.Write(_monstersCreated);
        saveStream.Write(_towerShop.CurrentShopPage);
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
    private void Load(BinaryReader loadStream)
    {
      _map.ChangeVisibleArea(loadStream.ReadInt32(), loadStream.ReadInt32());
      _gameScale = loadStream.ReadSingle();//Scale
      TowerConfSelectedID = loadStream.ReadInt32();
      //_towerConfSelectedID = loadStream.ReadInt32();
      _towerMapSelectedID = loadStream.ReadInt32();
      _monstersCreated = loadStream.ReadInt32();
      _towerShop.CurrentShopPage = loadStream.ReadInt32();
      //int currentShopPage = loadStream.ReadInt32();
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
        ChangeMapElementStatus(_towers[_towers.Count - 1].ArrayPos, MapElemStatus.BusyByTower);
      }
      //Missels section
      n = loadStream.ReadInt32();
      for (int i = 0; i < n; i++)
        _missels.Add(Missle.Factory(FactoryAct.Load, loadStream));
    }

  }
}