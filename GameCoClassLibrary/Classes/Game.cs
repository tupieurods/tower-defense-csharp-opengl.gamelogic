#define Debug

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Loaders;
using GameCoClassLibrary.Structures;

namespace GameCoClassLibrary.Classes
{
  /// <summary>
  /// Главный класс TGame,
  /// </summary>
  public sealed class Game
  {
    #region Private Vars

    #region Lists

    private readonly List<int> _numberOfMonstersAtLevel;//Число монстров на каждом из уровней
    private readonly List<int> _goldForSuccessfulLevelFinish;//Число золота за успешное завершение уровня
    private readonly List<int> _goldForKillMonster;//Золото за убийство монстра на уровне
    private readonly List<TowerParam> _towerParamsForBuilding;//Параметры башен
    private readonly List<string> _towerConfigsHashes;
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    private List<Monster> _monsters;//Список с монстрами на текущем уровне(!НЕ КОНФИГУРАЦИИ ВСЕХ УРОВНЕЙ)
    private List<Tower> _towers;//Список башен(поставленных на карте)
    private List<Missle> _missels;
    // ReSharper restore FieldCanBeMadeReadOnly.Local

    #endregion Lists

    #region Static

    #endregion Static

    #region Graphics

    private readonly System.Windows.Forms.PictureBox _gameDrawingSpace;//Picture Box для отрисовки
    private float _gameScale = 1.0F;//Масштаб, используемый в игре
    private readonly GraphicEngine _graphicEngine;

    #endregion Graphics

    #region TowerShop

    private int _towerConfSelectedID = -1;//Номер выбраной конфигурации в магазине(!NOT AT THE MAP!)
    private Point _arrayPosForTowerStanding = new Point(-1, -1);//НЕ НАСТОЯЩАЯ ПОЗИЦИЯ В МАССИВЕ КАРТЫ!, нужно ещё пересчитывать с учётом смещения
    private int _currentShopPage = 1;//Текущая страница магазина
    private readonly int _pageCount = 1;//Сколько всего страниц

    #endregion TowerShop

    #region Monsters

    private int _monstersCreated;//Число созданых монстров
    private MonsterParam _currentLevelConf;//Текущая конфигурация монстров
    private long _position;//Позиция в файле конфигурации монстров
    private readonly string _pathToLevelConfigurations;//Путь к файлу конфигурации уровней

    #endregion Monsters

    #region Game Logic

    private int _currentLevelNumber;//Номер текущего уровня
    private readonly int _levelsNumber;//Число уровней
    private readonly Map _map;//Карта
    //private bool _paused;

    #endregion Game Logic

    //Tower on Map selection
    private int _towerMapSelectedID = -1;//Номер выбраной вышки на карте(для башен ID значит номер в массиве)

    #endregion Private Vars

    #region Public

    public float Scaling//В set лучше не заглядывать, надеюсь кто-нибудь опытный в будущем поможет разгрести эту кашу
    {
      get
      {
        return _gameScale;
      }
      set
      {
        //Если программист не догадывается что изображение не может содержать не целый пиксель
        //мы защитимся от такого тормоза
        //if (Convert.ToInt32((value*Settings.ElemSize) - Math.Floor(value*Settings.ElemSize)) != 0) return;
        _gameScale = value;
        _graphicEngine.RecreateConstantImage(this, value);
        Helpers.BlackPen = new Pen(Color.Black, Settings.PenWidth * value);
        Helpers.GreenPen = new Pen(Color.Green, Settings.PenWidth * value);
        //Создание буфера кадров
        if (_gameDrawingSpace != null)
        {
          _gameDrawingSpace.Width = Convert.ToInt32(Settings.WindowWidth * Scaling);
          _gameDrawingSpace.Height = Convert.ToInt32(Settings.WindowHeight * Scaling);
          _graphicEngine.SetNewGraphBuffer(BufferedGraphicsManager.Current.Allocate(_gameDrawingSpace.CreateGraphics(),
                                                                                    new Rectangle(new Point(0, 0),
                                                                                                  _gameDrawingSpace.Size)));
        }
        foreach (Monster monster in _monsters)
        {
          monster.Scaling = value;
        }
        foreach (Tower tower in _towers)
        {
          tower.Scaling = value;
        }
        Missle.Scaling = value;
        _map.Scaling = value;
      }
    }

    public bool Lose
    {
      get;
      private set;
    }

    public bool Paused { get; set; }

    #endregion Public

    #region internal
    //Монстры
    internal IList<Monster> Monsters { get { return _monsters.AsReadOnly(); } }
    //Снаряды
    internal IList<Missle> Missels { get { return _missels.AsReadOnly(); } }
    //Башни
    internal IList<Tower> Towers { get { return _towers.AsReadOnly(); } }
    //Параметры башен для построения
    internal IList<TowerParam> TowerParamsForBuilding { get { return _towerParamsForBuilding.AsReadOnly(); } }
    //Карта
    internal Map Map { get { return _map; } }
    //ID конфигурации башни, которую хоятт поставить
    internal int TowerConfSelectedID { get { return _towerConfSelectedID; } }
    //ID выделенной на поле башни
    internal int TowerMapSelectedID { get { return _towerMapSelectedID; } }
    //Позция в которую хотят поставить башню
    internal Point ArrayPosForTowerStanding { get { return _arrayPosForTowerStanding; } }
    //Текущая страница магазина
    internal int CurrentShopPage { get { return _currentShopPage; } }
    //Число жизней
    internal int NumberOfLives { get; private set; }
    //Золото игрока
    internal int Gold { get; private set; }
    //Начат уровень или нет
    internal bool LevelStarted { get; private set; }

    #endregion internal

    #region Constructors

    /// <summary>
    /// Конструктор игры
    /// Предполагается что этот конструктор используется только в игре
    /// Соответсвенно должна иметься соостветсвующая структура папок
    /// </summary>
    /// <param name="graphicEngineType">Графичиский движок, которые будет использованн</param>
    /// <param name="pbForDraw">Picture Box на котором будет производиться отрисовка</param>
    /// <param name="filename">Имя конфигурации игры</param>
    private Game(string filename, GraphicEngineType graphicEngineType, System.Windows.Forms.PictureBox pbForDraw)
    {
      LevelStarted = false;
      Paused = false;
      object[] gameSettings;
      //Получили основную конфигурацию
      using (BinaryReader loader = new BinaryReader(new FileStream(Environment.CurrentDirectory + "\\Data\\GameConfigs\\" + filename + ".tdgc", FileMode.Open, FileAccess.Read)))
      {
        _pathToLevelConfigurations = Environment.CurrentDirectory + "\\Data\\GameConfigs\\" + filename + ".tdlc";
        SaveNLoad.LoadMainGameConf(loader, out _numberOfMonstersAtLevel, out _goldForSuccessfulLevelFinish, out _goldForKillMonster, out gameSettings);
        loader.Close();
      }
      //Создание оставшихся списков
      _monsters = new List<Monster>();
      _towers = new List<Tower>();
      _towerParamsForBuilding = new List<TowerParam>();
      _missels = new List<Missle>();
      //дополнительные инициализации
      Lose = false;
      //Загрузили карту
      _map = new Map(Environment.CurrentDirectory + "\\Data\\Maps\\" + Convert.ToString(gameSettings[0]).Substring(Convert.ToString(gameSettings[0]).LastIndexOf('\\')), true);
      //В будущем изменить масштабирование, чтобы не было лишней площади
      _gameDrawingSpace = pbForDraw;

      #region Загрузка параметров башен

      DirectoryInfo diForLoad = new DirectoryInfo(Environment.CurrentDirectory + "\\Data\\Towers\\" + Convert.ToString(gameSettings[1]));
      FileInfo[] towerConfigs = diForLoad.GetFiles();
      _towerConfigsHashes = new List<string>();
      foreach (FileInfo i in towerConfigs.Where(i => i.Extension == ".tdtc"))
      {
        if (_towerParamsForBuilding.Count == 90)//Если будет больше 90 башен то у меня печальные новости для дизайнера
          break;
        using (FileStream towerConfLoadStream = new FileStream(i.FullName, FileMode.Open, FileAccess.Read))
        {
          IFormatter formatter = new BinaryFormatter();
          _towerParamsForBuilding.Add((TowerParam)formatter.Deserialize(towerConfLoadStream));
        }
        _towerConfigsHashes.Add(Helpers.GetMD5ForFile(i.FullName));
      }
      _pageCount = (_towerParamsForBuilding.Count % Settings.ElemSize == 0) ? _towerParamsForBuilding.Count / Settings.ElemSize : (_towerParamsForBuilding.Count / Settings.ElemSize) + 1;

      #endregion Загрузка параметров башен

      //Число уровней, жизни
      _levelsNumber = (int)gameSettings[2];
      Gold = (int)gameSettings[4];
#if Debug
      Gold = 1000;
#endif
      NumberOfLives = (int)gameSettings[5];
      switch (graphicEngineType)
      {
        case GraphicEngineType.WinForms:
          _graphicEngine = new GraphicEngine(new WinFormsGraphic(null));
          break;
        case GraphicEngineType.OpenGL:
          break;
        case GraphicEngineType.SharpDX:
          break;
        default:
          throw new ArgumentOutOfRangeException("graphicEngineType");
      }
      /*if (pbForDraw != null)
        _graphicEngine = new GraphicEngine(new WinFormsGraphic(null));*/
      Scaling = 1F;
    }

    #endregion Constructors

    /// <summary>
    /// Используется фабрика, если произойдёт ошибка мы просто вернём null, а не получим франкинштейна
    /// </summary>
    /// <param name="act">Действие которое совершит фабрика</param>
    /// <param name="graphicEngineType">Тип графического движка</param>
    /// <param name="pbForDraw">Picture Box на котором будет производиться отрисовка</param>
    /// <param name="filename">Имя файла, который будет использованн для загрузки</param>
    /// <returns>Возвращает объект при успешной генерации</returns>
    public static Game Factory(string filename, FactoryAct act, GraphicEngineType graphicEngineType, System.Windows.Forms.PictureBox pbForDraw)
    {
      Game result = null;
      /*try
      {*/
      switch (act)
      {
        case FactoryAct.Create:
          result = new Game(filename, graphicEngineType, pbForDraw);
          break;
        case FactoryAct.Load:
          using (BinaryReader loadGameInfo = new BinaryReader(new FileStream(Environment.CurrentDirectory + "\\Data\\SavedGames\\" + filename + ".tdsg", FileMode.Open, FileAccess.Read)))
          {
            result = new Game(loadGameInfo.ReadString(), graphicEngineType, pbForDraw);
            result.Load(loadGameInfo);
          }
          break;
        default:
          throw new ArgumentOutOfRangeException("act");
      }
      //}
      /*catch (Exception exc)
      {
        System.Windows.Forms.MessageBox.Show("Game files damadged: " + exc.Message + "\n" + exc.StackTrace, "Fatal error");
      }*/
      return result;
    }

    #region Обработка действий пользователя

    /// <summary>
    /// Обработка нажатия кнопки мыши
    /// </summary>
    /// <param name="e">System.Windows.Forms.MouseEventArgs</param>
    public void MouseUp(System.Windows.Forms.MouseEventArgs e)
    {
      if (Paused)
        return;

      bool flag = false;

      #region Если уровень ещё не начат и игрок захотел начать

      if ((!LevelStarted) && (_currentLevelNumber < _levelsNumber))
      {
        if (Helpers.BuildRect(RectBuilder.NewLevelEnabled, Scaling).Contains(e.X, e.Y))
        {
          LevelStarted = true;
          _currentLevelNumber++;
          _monstersCreated = 0;
          //_monsters.Clear();//Не нужно, т.к монстры в случае начала нового уровня и так будут мертвы
          LoadLevel();
          flag = true;
        }
      }

      #endregion Если уровень ещё не начат и игрок захотел начать

      #region Tower Page Selection

      if ((!flag) && (_towerParamsForBuilding.Count > Settings.ElemSize) && ((e.X >= Convert.ToInt32((Settings.MapAreaSize + 10 + Settings.DeltaX * 2) * Scaling))
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

      #region Если пользователь хочет выделить вышку

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
                if (_towers[i].Contain(new Point(arrPos.X + _map.VisibleXStart, arrPos.Y + _map.VisibleYStart)))
                {
                  _towerMapSelectedID = i;
                  //flag = true;
                  return;
                }
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

      #endregion Если пользователь хочет выделить вышку

      #region Если пользователь хочет поставить вышку

      if (_towerConfSelectedID != -1)//Если !=-1 значит в границах карты и Flag=false 100%
      {
        switch (e.Button)
        {
          case System.Windows.Forms.MouseButtons.Left:
            if (Check(_arrayPosForTowerStanding) && (Gold >= _towerParamsForBuilding[_towerConfSelectedID].UpgradeParams[0].Cost))
            {
              _towers.Add(Tower.Factory(FactoryAct.Create, _towerParamsForBuilding[_towerConfSelectedID],
                new Point(_arrayPosForTowerStanding.X + _map.VisibleXStart, _arrayPosForTowerStanding.Y + _map.VisibleYStart), _towerConfigsHashes[_towerConfSelectedID], Scaling));
              Gold -= _towerParamsForBuilding[_towerConfSelectedID].UpgradeParams[0].Cost;
              for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                  _map.SetMapElemStatus(_arrayPosForTowerStanding.X + i + _map.VisibleXStart,
                    _arrayPosForTowerStanding.Y + j + _map.VisibleYStart, MapElemStatus.BusyByTower);
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

      #endregion Если пользователь хочет поставить вышку

      #region Пользователь захотел уничтожить вышку или улучшить

      if (_towerMapSelectedID != -1)
      {
        if (Helpers.BuildRect(RectBuilder.Destroy, Scaling).Contains(e.X, e.Y))
        {
          for (int i = 0; i < 2; i++)
          {
            for (int j = 0; j < 2; j++)
            {
              _map.SetMapElemStatus(_towers[_towerMapSelectedID].ArrayPos.X + i, _towers[_towerMapSelectedID].ArrayPos.Y + j, MapElemStatus.CanBuild);
            }
          }
          _towers.RemoveAt(_towerMapSelectedID);
          FinishTowerMapSelectAct();
        }
        else if ((Helpers.BuildRect(RectBuilder.Upgrade, Scaling).Contains(e.X, e.Y)) && (_towers[_towerMapSelectedID].CanUpgrade) &&
          _towers[_towerMapSelectedID].CurrentTowerParams.Cost <= Gold)
        {
          Gold -= _towers[_towerMapSelectedID].Upgrade();
        }
      }

      #endregion Пользователь захотел уничтожить вышку или улучшить
    }

    /// <summary>
    /// Загрузка уровня из файла конфигурации уровней
    /// </summary>
    /// <param name="level">Если -1, то загружается из файла с позиции Position, в ином случае прокручиваются все уровни до необходимого</param>
    private void LoadLevel(int level = -1)
    {
      #region Загружаем конфигурацию уровня

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

      #endregion Загружаем конфигурацию уровня

      //Оптимизируем проверки на вхождение в видимую область карты
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
    /// Вызывается при попытке смены показываемой области карты
    /// </summary>
    /// <param name="position">Позиция мыши</param>
    /// <returns>Произведена ли смена области</returns>
    public bool MapAreaChanging(Point position)
    {
      if (Paused)
        return false;
      if ((_map.Width <= 30) || (_map.Height <= 30))
        return false;
      if (((position.X > Settings.DeltaX) && (position.X < (Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaX)))
        && ((position.Y > Settings.DeltaY) && (position.Y < (Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaY))))
      {
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
    /// Перемещение мыши
    /// </summary>
    /// <param name="e">System.Windows.Forms.MouseEventArgs</param>
    public void MouseMove(System.Windows.Forms.MouseEventArgs e)
    {
      if (Paused)
        return;
      #region Обработка перемещения при попытке постановки башни

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

      #endregion Обработка перемещения при попытке постановки башни
    }

    #endregion Обработка действий пользователя

    #region Game Logic

    #region "Финализаторы" действий

    /// <summary>
    /// Если была выделена вышка и необходимо снять выделение
    /// </summary>
    private void FinishTowerMapSelectAct()
    {
      _towerMapSelectedID = -1;
    }

    /// <summary>
    /// Если поставили вышку или отменили её поставку
    /// </summary>
    private void FinishTowerShopAct()
    {
      _towerConfSelectedID = -1;
      _arrayPosForTowerStanding = new Point(-1, -1);
    }

    #endregion "Финализаторы" действий

    #region Действия с магазином башен

    /*
     * Пояснение того зачем вообще сделаны ShopPageSelectorAction и ShopPageAction
     * Если изменится структура магазина, цикл будет изменяться в одном месте
     */

    /// <summary>
    /// Действие с Page Selector'ом магазина(Вывод или выбор)
    /// </summary>
    /// <param name="act">Отображение селектора или проверка нажатия по нему</param>
    /// <param name="XMouse">Позиция мыши для проверки</param>
    /// <param name="YMouse">Позиция мыши для проверки</param>
    /// <returns>Если вызвано для проверки на попадание мышью, возвращает результат проверки</returns>
    // ReSharper disable InconsistentNaming
    internal bool ShopPageSelectorAction(Func<int, int, int, int, bool> act, int XMouse = 0, int YMouse = 0)
    // ReSharper restore InconsistentNaming
    {
      int dy = 0;//Если больше одного ряда страниц будет изменена в процессе цикла
      for (int i = 0; i < _pageCount; i++)
      {
        if ((i != 0) && (i % 3 == 0))
          dy++;
        if (act(i, dy, XMouse, YMouse))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Действие со страницей магазина(Вывод или выбор)
    /// </summary>
    /// <param name="act">Отображение страницы магазина или проверка нажатия по ней</param>
    /// <param name="XMouse">Позиция мыши для проверки</param>
    /// <param name="YMouse">Позиция мыши для проверки</param>
    /// <returns>Если вызвано для проверки на попадание мышью, возвращает результат проверки</returns>
    // ReSharper disable InconsistentNaming
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

    #endregion Действия с магазином башен

    /// <summary>
    /// Проверка при попытке постановки башни, входит ли в границы массива
    /// </summary>
    /// <param name="pos">Проверяемый элемент карты</param>
    /// <param name="simple">Если True, то проверять три клетки справа и внизу не нужно</param>
    /// <returns>Результат проверки</returns>
    internal bool Check(Point pos, bool simple = false)
    {
      pos.X += _map.VisibleXStart;
      pos.Y += _map.VisibleYStart;
      if (((pos.X >= 0) && (pos.X < _map.Width - 1)) && ((pos.Y >= 0) && (pos.Y < _map.Height - 1)))
      {
        if (simple)
          return true;
        for (int dx = 0; dx <= 1; dx++)
          for (int dy = 0; dy <= 1; dy++)
          {
            if (_map.GetMapElemStatus(pos.X + dx, pos.Y + dy) != MapElemStatus.CanBuild)//Если не свободное для постановки место
              return false;
          }
        return true;
      }
      return false;
    }

    /// <summary>
    /// Добавление врага
    /// </summary>
    private void AddMonster()
    {
      _monsters.Add(new Monster(_currentLevelConf, _map.Way, _monstersCreated, Scaling));
      _monstersCreated++;
    }

    /// <summary>
    /// Число вышек на выбраной странице магазина
    /// </summary>
    /// <param name="pageNumber">Номер страницы магазина</param>
    /// <returns>Число вышек на странице</returns>
    private int GetNumberOfTowersAtPage(int pageNumber = 1)
    {
      return (_pageCount != pageNumber)
       ? (Settings.LinesInOnePage * Settings.MaxTowersInLine) :
       _towerParamsForBuilding.Count - (pageNumber - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine);
    }

    /// <summary>
    /// Обработка проигрыша
    /// </summary>
    private void Looser()
    {
      Lose = true;
      //_graphicEngine.Show(this);
    }

    /// <summary>
    /// Процедура, обрабатываемая в таймере
    /// </summary>
    public void Tick()
    {
      if (LevelStarted)
      {
        #region Движение монстров + Подсветка невидимых юнитов

        var allTrueSightTowers = Towers.Where(x => x.TrueSight);

        foreach (Monster monster in _monsters)
        {
          Point tmp = monster.GetArrayPos;
          _map.SetMapElemStatus(tmp.X, tmp.Y, MapElemStatus.CanMove);
          int dx = 0;//Для определения, можно ли двигаться далее(т.е нет ли впереди дургого монстра)
          int dy = 0;

          #region Определение перемещения

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

          #endregion Определение перемещения

          if (((tmp.Y + dy <= _map.Height) && (tmp.Y + dy >= 0)) && ((tmp.X + dx <= _map.Width) && (tmp.X + dx >= 0))
            && (_map.GetMapElemStatus(tmp.X + dx, tmp.Y + dy) == MapElemStatus.CanMove))//Блокировка более быстрых объектов более медленными
            monster.Move(true);//Перемещается
          else
            monster.Move(false);//Тормозится
          // ReSharper disable PossibleMultipleEnumeration
          if (_currentLevelConf.Base.Invisible)
            if (allTrueSightTowers.Any(tower => tower.InAttackRadius(monster.GetCanvaPos.X, monster.GetCanvaPos.Y)))
            // ReSharper restore PossibleMultipleEnumeration
            {
              monster.MakeVisible();
            }
          if (monster.NewLap)//Если монстр прошёл полный круг
          {
            monster.NewLap = false;
            NumberOfLives--;
            if (NumberOfLives == 0)
            {
              Looser();
              return;//выходим
            }
          }
          tmp = monster.GetArrayPos;
          _map.SetMapElemStatus(tmp.X, tmp.Y, MapElemStatus.BusyByUnit);
        }

        #endregion Движение монстров

        #region Действия башен(Выстрелы)

        //Создание снарядов
        foreach (Tower tower in _towers)
        {
          _missels.AddRange(tower.GetAims(_monsters.Where(elem => elem.Visible)));
        }

        #endregion Действия башен(Выстрелы)

        #region Перемещение снаряда
        foreach (Missle missle in Missels.Where(missle => !missle.DestroyMe))
        {
          missle.Move(Monsters);
        }
        #endregion

        #region Добавление монстров(после движения, чтобы мы могли добавить монстра сразу же после освобождения начальной клетки)

        if ((_monstersCreated != _numberOfMonstersAtLevel[_currentLevelNumber - 1]) && (_map.GetMapElemStatus(_map.Way[0].X, _map.Way[0].Y) == MapElemStatus.CanMove))
        {
          AddMonster();//Если слишком много монстров создаётся, но при этом ещё подкидываются новые монстры как создающиеся
          //Т.е игрок не убивает монстров за проход по кругу, причём они ещё создаются - это проблемы игрока, полчит наслаивающихся монстров
          //Ибо нефиг быть таким днищем(фича, а не баг)
          _map.SetMapElemStatus(_map.Way[0].X, _map.Way[0].Y, MapElemStatus.BusyByUnit);
        }

        if ((_monstersCreated == _numberOfMonstersAtLevel[_currentLevelNumber - 1]) && (_monsters.Count == 0))
        {
          LevelStarted = false;
        }

        #endregion Добавление монстров(после движения, чтобы мы могли добавить монстра сразу же после освобождения начальной клетки)
      }
      if (System.Windows.Forms.Control.MouseButtons == System.Windows.Forms.MouseButtons.Middle)
        if (MapAreaChanging(_gameDrawingSpace.PointToClient(System.Windows.Forms.Control.MousePosition)))
          _graphicEngine.RepaintConstImage = true;

      #region Удаление объектов, которые больше не нужны(например снаряд добил монстра)

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

      #endregion Удаление объектов, которые больше не нужны(например снаряд добил монстра)
    }

    /// <summary>
    /// Визуализация
    /// </summary>
    public void Render()
    {
      _graphicEngine.Show(this);
    }

    #endregion Game Logic

    public void SaveGame(string fileName)
    {
      using (BinaryWriter saveStream = new BinaryWriter(new FileStream(Environment.CurrentDirectory + "\\Data\\SavedGames\\" + fileName + ".tdsg", FileMode.CreateNew, FileAccess.Write)))
      {
        System.Windows.Forms.MessageBox.Show(fileName + ".tdsg");
        //Не забываем, что _pathToLevelConfigurations содержит полный путь к файлу конфигурации монстров
        string confFileNameWithExtension = _pathToLevelConfigurations.Substring(_pathToLevelConfigurations.LastIndexOf("\\", StringComparison.Ordinal));//Получили имя файла с расширением
        saveStream.Write(confFileNameWithExtension.Substring(0, confFileNameWithExtension.Length - 5));//Убрали расширение и записали
        saveStream.Write(_gameScale);//Запомнили масштаб
        saveStream.Write(_towerConfSelectedID);//Выбраный ID башни в магазине
        saveStream.Write(_towerMapSelectedID);
        saveStream.Write(_monstersCreated);
        saveStream.Write(_currentShopPage);
        saveStream.Write(_currentLevelNumber);
        saveStream.Write(Gold);
        saveStream.Write(NumberOfLives);
        saveStream.Write(LevelStarted);
        //saveStream.Write(true);//(Paused);
        //Хеши файлов башен
        saveStream.Write(_towerConfigsHashes.Count);
        _towerConfigsHashes.ForEach(saveStream.Write);
        //Секция монстров
        //Число монстров
        saveStream.Write(_monsters.Count(x => !x.DestroyMe));
        //Сам список монстров
        _monsters.ForEach(x => x.Save(saveStream));
        //Секция башен
        //Число башен
        saveStream.Write(_towers.Count);
        //Сам список башен
        _towers.ForEach(x => x.Save(saveStream));
        //Секция снарядов
        //Число снарядов
        saveStream.Write(_missels.Count(x => !x.DestroyMe));
        //Сам список снарядов
        _missels.ForEach(x => x.Save(saveStream));
      }
    }

    public void Load(BinaryReader loadStream)
    {
      _gameScale = loadStream.ReadSingle();//Масштаб
      _towerConfSelectedID = loadStream.ReadInt32();//Выбраный ID башни в магазине
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
      //Секция монстров
      n = loadStream.ReadInt32();
      LoadLevel(_currentLevelNumber);
      for (int i = 0; i < n; i++)
      {
        _monsters.Add(new Monster(_currentLevelConf, _map.Way, -1, _gameScale));
        _monsters[_monsters.Count - 1].Load(loadStream);
      }
      //Секция башен
      n = loadStream.ReadInt32();
      for (int i = 0; i < n; i++)
      {
        string hash = loadStream.ReadString();
        _towers.Add(Tower.Factory(FactoryAct.Load, _towerParamsForBuilding[_towerConfigsHashes.IndexOf(hash)], new Point(loadStream.ReadInt32(), loadStream.ReadInt32()), hash, _gameScale, loadStream));
      }
      //Секция снарядов
      n = loadStream.ReadInt32();
      for (int i = 0; i < n; i++)
      {
        _missels.Add(Missle.Factory(FactoryAct.Load, loadStream));
      }
    }
  }
}