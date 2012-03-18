#define Debug

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Structures;
using GameCoClassLibrary.Loaders;

namespace GameCoClassLibrary.Classes
{
  /// <summary>
  /// Главный класс TGame, 
  /// </summary>
  public sealed class TGame
  {
    #region Private Vars

    #region Lists
    private List<int> NumberOfMonstersAtLevel;//Число монстров на каждом из уровней
    private List<int> GoldForSuccessfulLevelFinish;//Число золота за успешное завершение уровня
    private List<int> GoldForKillMonster;//Золото за убийство монстра на уровне
    private List<TowerParam> _TowerParamsForBuilding;//Параметры башен
    private List<TMonster> _Monsters;//Список с монстрами на текущем уровне(!НЕ КОНФИГУРАЦИИ ВСЕХ УРОВНЕЙ)
    private List<TTower> _Towers;//Список башен(поставленных на карте)
    private List<TMissle> _Missels;
    #endregion

    #region Static

    #endregion

    #region Graphics
    private System.Windows.Forms.PictureBox GameDrawingSpace;//Picture Box для отрисовки
    private float GameScale = 1.0F;//Масштаб, используемый в игре
    private TGraphicEngine GraphicEngine;
    #endregion

    #region TowerShop
    private int _TowerConfSelectedID = -1;//Номер выбраной конфигурации в магазине(!NOT AT THE MAP!)
    private Point _ArrayPosForTowerStanding = new Point(-1, -1);//НЕ НАСТОЯЩАЯ ПОЗИЦИЯ В МАССИВЕ КАРТЫ!, нужно ещё пересчитывать с учётом смещения
    private int _CurrentShopPage = 1;//Текущая страница магазина
    private int PageCount = 1;//Сколько всего страниц
    #endregion

    #region Monsters
    private int MonstersCreated = 0;//Число созданых монстров
    private MonsterParam CurrentLevelConf;//Текущая конфигурация монстров
    private long Position = 0;//Позиция в файле конфигурации монстров
    private string PathToLevelConfigurations;//Путь к файлу конфигурации уровней
    #endregion

    #region Game Logic
    private System.Windows.Forms.Timer GameTimer;
    private int CurrentLevelNumber = 0;//Номер текущего уровня
    private int LevelsNumber;//Число уровней
    private int _Gold;//Золото игрока
    private int _NumberOfLives;//Число монстров которых можно пропустить
    private bool _LevelStarted = false;//Начат уровень или нет
    private TMap _Map;//Карта
    #endregion

    //Tower on Map selection
    private int _TowerMapSelectedID = -1;//Номер выбраной вышки на карте(для башен ID значит номер в массиве)

    #endregion

    #region Public
    public float Scaling//В set лучше не заглядывать, надеюсь кто-нибудь опытный в будущем поможет разгрести эту кашу
    {
      get
      {
        return GameScale;
      }
      set
      {
        if /*(GameScale - value <= 0.00001) && */(Convert.ToInt32((value * Settings.ElemSize) - Math.Floor(value * Settings.ElemSize)) == 0)//Если программист не догадывается что изображение не может содержать
        //не целый пиксель мы защитимся от такого тормоза
        {
          GameScale = value;
          GraphicEngine.RecreateConstantImage(this, value);
          THelpers.BlackPen = new Pen(Color.Black, Settings.PenWidth * value);
          THelpers.GreenPen = new Pen(Color.Green, Settings.PenWidth * value);
          GameDrawingSpace.Width = Convert.ToInt32(/*GameDrawingSpace.Width*/ 730 * Scaling);
          GameDrawingSpace.Height = Convert.ToInt32(/*GameDrawingSpace.Height*/600 * Scaling);
          //Создание буфера кадров
          GraphicEngine.SetNewGraphBuffer(BufferedGraphicsManager.Current.Allocate(GameDrawingSpace.CreateGraphics(), new Rectangle(new Point(0, 0), GameDrawingSpace.Size)));
          foreach (TMonster Monster in _Monsters)
          {
            Monster.Scaling = value;
          }
          foreach (TTower Tower in _Towers)
          {
            Tower.Scaling = value;
          }
          TMissle.Scaling = value;
          _Map.Scaling = value;
        }
      }
    }
    public bool Lose
    {
      get;
      private set;
    }
    #endregion

    #region internal
    internal List<TMonster> Monsters
    {
      get
      {
        return _Monsters;
      }
    }
    internal List<TMissle> Missels
    {
      get
      {
        return _Missels;
      }
    }
    internal List<TTower> Towers
    {
      get
      {
        return _Towers;
      }
    }
    internal List<TowerParam> TowerParamsForBuilding
    {
      get
      {
        return _TowerParamsForBuilding;
      }
    }
    internal TMap Map
    {
      get
      {
        return _Map;
      }
    }
    internal int TowerConfSelectedID
    {
      get
      {
        return _TowerConfSelectedID;
      }
    }
    internal int TowerMapSelectedID
    {
      get
      {
        return _TowerMapSelectedID;
      }
    }
    internal Point ArrayPosForTowerStanding
    {
      get
      {
        return _ArrayPosForTowerStanding;
      }
    }
    internal int CurrentShopPage
    {
      get
      {
        return _CurrentShopPage;
      }
    }
    internal int NumberOfLives
    {
      get
      {
        return _NumberOfLives;
      }
    }
    internal int Gold
    {
      get
      {
        return _Gold;
      }
    }
    internal bool LevelStarted
    {
      get
      {
        return _LevelStarted;
      }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Конструктор игры
    /// Предполагается что этот конструктор используется только в игре
    /// Соответсвенно должна иметься соостветсвующая структура папок
    /// </summary>
    /// <param name="PBForDraw">Picture Box на котором будет производиться отрисовка</param>
    /// <param name="GameTimer">Игровой таймер</param>
    /// <param name="ConfigurationName">Имя конфигурации игры</param>
    private TGame(System.Windows.Forms.PictureBox PBForDraw, System.Windows.Forms.Timer GameTimer, string ConfigurationName)
    {
      //Получили основную конфигурацию
      BinaryReader Loader = new BinaryReader(new FileStream(Environment.CurrentDirectory + "\\Data\\GameConfigs\\" + ConfigurationName + ".tdgc",
                                                              FileMode.Open, FileAccess.Read));
      PathToLevelConfigurations = Environment.CurrentDirectory + "\\Data\\GameConfigs\\" + ConfigurationName + ".tdlc";
      object[] GameSettings;
      SaveNLoad.LoadMainGameConf(Loader, out NumberOfMonstersAtLevel, out GoldForSuccessfulLevelFinish, out GoldForKillMonster, out GameSettings);
      Loader.Close();
      //Создание оставшихся списков
      _Monsters = new List<TMonster>();
      _Towers = new List<TTower>();
      _TowerParamsForBuilding = new List<TowerParam>();
      _Missels = new List<TMissle>();
      //дополнительные инициализации
      Lose = false;
      //Загрузили карту
      _Map = new TMap(Environment.CurrentDirectory + "\\Data\\Maps\\" + Convert.ToString(GameSettings[0]).Substring(Convert.ToString(GameSettings[0]).LastIndexOf('\\')), true);
      //В будущем изменить масштабирование, чтобы не было лишней площади
      GameDrawingSpace = PBForDraw;
      #region Загрузка параметров башен
      DirectoryInfo DIForLoad = new DirectoryInfo(Environment.CurrentDirectory + "\\Data\\Towers\\" + Convert.ToString(GameSettings[1]));
      FileInfo[] TowerConfigs = DIForLoad.GetFiles();
      foreach (FileInfo i in TowerConfigs)
      {
        if (_TowerParamsForBuilding.Count == 90)//Если будет больше 90 башен то у меня печальные новости для дизайнера
          break;
        if (i.Extension == ".tdtc")
        {
          using (FileStream TowerConfLoadStream = new FileStream(i.FullName, FileMode.Open, FileAccess.Read))
          {
            IFormatter Formatter = new BinaryFormatter();
            _TowerParamsForBuilding.Add((TowerParam)Formatter.Deserialize(TowerConfLoadStream));
          }
        }
      }
      PageCount = (_TowerParamsForBuilding.Count % Settings.ElemSize == 0) ? _TowerParamsForBuilding.Count / Settings.ElemSize : (_TowerParamsForBuilding.Count / Settings.ElemSize) + 1;
      #endregion
      //Число уровней, жизни
      LevelsNumber = (int)GameSettings[2];
      _Gold = (int)GameSettings[4];
#if Debug
      _Gold = 1000;
#endif
      _NumberOfLives = (int)GameSettings[5];
      GraphicEngine = new TGraphicEngine();
      Scaling = 1F;
      //Настройка и запуск таймера
      this.GameTimer = GameTimer;
      this.GameTimer.Tick += new System.EventHandler(Timer_Tick);
      this.GameTimer.Interval = 30;//1;
      this.GameTimer.Start();
    }
    #endregion

    /// <summary>
    /// Используется фабрика, если произойдёт ошибка мы просто вернём null, а не получим франкинштейна
    /// </summary>
    /// <param name="PBForDraw">Picture Box на котором будет производиться отрисовка</param>
    /// <param name="GameTimer">Игровой таймер</param>
    /// <param name="ConfigurationName">Имя конфигурации игры</param>
    /// <returns>Возвращает объект при успешной генерации</returns>
    public static TGame Factory(System.Windows.Forms.PictureBox PBForDraw, System.Windows.Forms.Timer GameTimer, string ConfigurationName)
    {
      TGame Result = null;
      try
      {
        Result = new TGame(PBForDraw, GameTimer, ConfigurationName);
      }
      catch (Exception exc)
      {
        System.Windows.Forms.MessageBox.Show("Game files damadged: " + exc.Message + "\n" + exc.StackTrace, "Fatal error");
      }
      return Result;
    }

    #region Обработка действий пользователя
    /// <summary>
    /// Обработка нажатия кнопки мыши
    /// </summary>
    /// <param name="e">System.Windows.Forms.MouseEventArgs</param>
    public void MouseUp(System.Windows.Forms.MouseEventArgs e)
    {
      bool Flag = false;
      #region Если уровень ещё не начат и игрок захотел начать
      if ((!_LevelStarted) && (CurrentLevelNumber < LevelsNumber))
      {
        if (THelpers.BuildRect(RectBuilder.NewLevelEnabled, Scaling).Contains(e.X, e.Y))
        {
          _LevelStarted = true;
          CurrentLevelNumber++;
          MonstersCreated = 0;
          _Monsters.Clear();
          #region Загружаем конфигурацию уровня
          FileStream LevelLoadStream = new FileStream(PathToLevelConfigurations, FileMode.Open, FileAccess.Read);
          IFormatter Formatter = new BinaryFormatter();
          LevelLoadStream.Seek(Position, SeekOrigin.Begin);
          CurrentLevelConf = (MonsterParam)(Formatter.Deserialize(LevelLoadStream));
          Position = LevelLoadStream.Position;
          LevelLoadStream.Close();
          #endregion
          //Оптимизируем проверки на вхождение в видимую область карты
          TMonster.HalfSizes = new int[]{
            CurrentLevelConf[MonsterDirection.Up,0].Height/2,
            CurrentLevelConf[MonsterDirection.Right,0].Width/2,
            CurrentLevelConf[MonsterDirection.Down,0].Height/2,
            CurrentLevelConf[MonsterDirection.Left,0].Width/2,
          };
          Flag = true;
        }
      }
      #endregion
      #region Tower Page Selection
      if ((!Flag) && (_TowerParamsForBuilding.Count > Settings.ElemSize) && ((e.X >= Convert.ToInt32((Settings.MapAreaSize + 10 + Settings.DeltaX * 2) * Scaling))
        && (e.Y >= Convert.ToInt32((37 + Res.MoneyPict.Height) * Scaling))
        && (e.Y <= Convert.ToInt32((247 + Res.MoneyPict.Height) * Scaling))))
      {
        Flag = ShopPageSelectorAction((int i, int DY, int XMouse, int YMouse) =>
        {
          if (THelpers.LambdaBuildRectPageSelector(this, i, DY).Contains(XMouse, YMouse))
          {
            _CurrentShopPage = i + 1;
            FinishTowerShopAct();
            return true;
          }
          return false;
        }, null, e.X, e.Y);
      }
      #endregion
      #region Tower Selected in Shop
      if ((!Flag) && (e.X >= (Convert.ToInt32((460 + Settings.DeltaX * 2) * Scaling))
        && (e.Y >= Convert.ToInt32((90 + Res.MoneyPict.Height) * Scaling))
        && (e.Y <= Convert.ToInt32((100 + Res.MoneyPict.Height + 42 * ((_TowerParamsForBuilding.Count / 5) + 1)) * Scaling))))//Если в границах
      {
        Flag = ShopPageAction((int i, int j, int offset, int XMouse, int YMouse) =>
        {
          if (THelpers.LambdaBuildRectPage(this, i, j).Contains(XMouse, YMouse))//Если нашли выделенную башню
          {
            FinishTowerMapSelectAct();
            _TowerConfSelectedID = (_CurrentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset;
            return true;
          }
          return false;
        }, null, e.X, e.Y);
      }
      #endregion
      #region Если пользователь хочет выделить вышку
      if ((!Flag) && (_TowerConfSelectedID == -1)
        && ((e.X >= Settings.DeltaX) && (e.X <= (int)(Settings.MapAreaSize * Scaling) + Settings.DeltaX) && (e.Y >= Settings.DeltaY) && (e.Y <= (int)(Settings.MapAreaSize * Scaling) + Settings.DeltaY)))
      {
        switch (e.Button)
        {
          case System.Windows.Forms.MouseButtons.Left:
            Point ArrPos = new Point((e.X - Settings.DeltaX) / Convert.ToInt32(Settings.ElemSize * Scaling),
              (e.Y - Settings.DeltaY) / Convert.ToInt32(Settings.ElemSize * Scaling));
            if (!Check(ArrPos, true))
              break;
            if (_Map.GetMapElemStatus(ArrPos.X + _Map.VisibleXStart, ArrPos.Y + _Map.VisibleYStart) == MapElemStatus.BusyByTower)
            {
              for (int i = 0; i < _Towers.Count; i++)
              {
                if (_Towers[i].Contain(new Point(ArrPos.X + _Map.VisibleXStart, ArrPos.Y + _Map.VisibleYStart)))
                {
                  _TowerMapSelectedID = i;
                  Flag = true;
                  return;
                }
              }
            }
            break;
          case System.Windows.Forms.MouseButtons.Right:
            if (_TowerMapSelectedID != -1)
            {
              FinishTowerMapSelectAct();
            }
            break;
        }
      }
      #endregion
      #region Если пользователь хочет поставить вышку
      if (_TowerConfSelectedID != -1)//Если !=-1 значит в границах карты и Flag=false 100%
      {
        switch (e.Button)
        {
          case System.Windows.Forms.MouseButtons.Left:
            if (Check(_ArrayPosForTowerStanding, false) && (_Gold >= _TowerParamsForBuilding[_TowerConfSelectedID].UpgradeParams[0].Cost))
            {
              _Towers.Add(new TTower(_TowerParamsForBuilding[_TowerConfSelectedID],
                new Point(_ArrayPosForTowerStanding.X + _Map.VisibleXStart, _ArrayPosForTowerStanding.Y + _Map.VisibleYStart), Scaling));
              _Gold -= _TowerParamsForBuilding[_TowerConfSelectedID].UpgradeParams[0].Cost;
              for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                  _Map.SetMapElemStatus(_ArrayPosForTowerStanding.X + i + _Map.VisibleXStart,
                    _ArrayPosForTowerStanding.Y + j + _Map.VisibleYStart, MapElemStatus.BusyByTower);
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
      #endregion
      #region Пользователь захотел уничтожить вышку или улучшить
      if (_TowerMapSelectedID != -1)
      {
        if (THelpers.BuildRect(RectBuilder.Destroy, Scaling).Contains(e.X, e.Y))
        {
          for (int i = 0; i < 2; i++)
          {
            for (int j = 0; j < 2; j++)
            {
              _Map.SetMapElemStatus(_Towers[_TowerMapSelectedID].ArrayPos.X + i, _Towers[_TowerMapSelectedID].ArrayPos.Y + j, MapElemStatus.CanBuild);
            }
          }
          _Towers.RemoveAt(_TowerMapSelectedID);
          FinishTowerMapSelectAct();
        }
        else if ((THelpers.BuildRect(RectBuilder.Upgrade, Scaling).Contains(e.X, e.Y)) && (_Towers[_TowerMapSelectedID].CanUpgrade) &&
          _Towers[_TowerMapSelectedID].CurrentTowerParams.Cost <= _Gold)
        {
          _Gold -= _Towers[_TowerMapSelectedID].Upgrade();
        }
      }
      #endregion
    }

    /// <summary>
    /// Вызывается при попытке смены показываемой области карты
    /// </summary>
    /// <param name="Position">Позиция мыши</param>
    /// <returns>Произведена ли смена области</returns>
    public bool MapAreaChanging(Point Position)
    {
      if ((_Map.Width <= 30) || (_Map.Height <= 30))
        return false;
      if (((Position.X > Settings.DeltaX) && (Position.X < (Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaX)))
        && ((Position.Y > Settings.DeltaY) && (Position.Y < (Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaY))))
      {
        if ((Position.X - Settings.DeltaX < Settings.ElemSize) && (_Map.VisibleXStart != 0))
        {
          _Map.ChangeVisibleArea(-1, 0);
          return true;
        }
        if ((Position.Y - Settings.DeltaY < Settings.ElemSize) && (_Map.VisibleYStart != 0))
        {
          _Map.ChangeVisibleArea(0, -1);
          return true;
        }
        if (((-Position.X + Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaX) < Settings.ElemSize) && (_Map.VisibleXFinish != _Map.Width))
        {
          _Map.ChangeVisibleArea(1, 0);
          return true;
        }
        if (((-Position.Y + Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaY) < Settings.ElemSize) && (_Map.VisibleYFinish != _Map.Height))
        {
          _Map.ChangeVisibleArea(0, 1);
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
      #region Обработка перемещения при попытке постановки башни
      if ((_TowerConfSelectedID != -1) && (new Rectangle(Convert.ToInt32(Settings.DeltaX * Scaling), Convert.ToInt32(Settings.DeltaY * Scaling),
        Convert.ToInt32(Settings.MapAreaSize * Scaling), Convert.ToInt32(Settings.MapAreaSize * Scaling)).Contains(e.X, e.Y)))
      {
        _ArrayPosForTowerStanding =
          new Point((e.X - Settings.DeltaX) / Convert.ToInt32(Settings.ElemSize * Scaling),
                    (e.Y - Settings.DeltaY) / Convert.ToInt32(Settings.ElemSize * Scaling));
        if (!Check(_ArrayPosForTowerStanding, true))
          _ArrayPosForTowerStanding = new Point(-1, -1);
      }
      else
        _ArrayPosForTowerStanding = new Point(-1, -1);
      #endregion
    }
    #endregion

    #region Game Logic

    #region "Финализаторы" действий
    /// <summary>
    /// Если была выделена вышка и необходимо снять выделение
    /// </summary>
    private void FinishTowerMapSelectAct()
    {
      _TowerMapSelectedID = -1;
    }

    /// <summary>
    /// Если поставили вышку или отменили её поставку
    /// </summary>
    private void FinishTowerShopAct()
    {
      _TowerConfSelectedID = -1;
      _ArrayPosForTowerStanding = new Point(-1, -1);
    }
    #endregion

    #region Действия с магазином башен
    /*
     * Пояснение того зачем вообще сделаны ShopPageSelectorAction и ShopPageAction
     * Если изменится структура магазина, цикл будет изменяться в одном месте
     */

    /// <summary>
    /// Действие с Page Selector'ом магазина(Вывод или выбор)
    /// </summary>
    /// <param name="Act">Отображение селектора или проверка нажатия по нему</param>
    /// <param name="Canva">Graphics для отрисовки</param>
    /// <param name="XMouse">Позиция мыши для проверки</param>
    /// <param name="YMouse">Позиция мыши для проверки</param>
    /// <returns>Если вызвано для проверки на попадание мышью, возвращает результат проверки</returns>
    internal bool ShopPageSelectorAction(Func<int, int, int, int, bool> Act, Graphics Canva = null, int XMouse = 0, int YMouse = 0)
    {
      int DY = 0;//Если больше одного ряда страниц будет изменена в процессе цикла
      for (int i = 0; i < PageCount; i++)
      {
        if ((i != 0) && (i % 3 == 0))
          DY++;
        if (Act(i, DY, XMouse, YMouse))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Действие со страницей магазина(Вывод или выбор)
    /// </summary>
    /// <param name="Act">Отображение страницы магазина или проверка нажатия по ней</param>
    /// <param name="Canva">Graphics для отрисовки</param>
    /// <param name="XMouse">Позиция мыши для проверки</param>
    /// <param name="YMouse">Позиция мыши для проверки</param>
    /// <returns>Если вызвано для проверки на попадание мышью, возвращает результат проверки</returns>
    internal bool ShopPageAction(Func<int, int, int, int, int, bool> Act, Graphics Canva = null, int XMouse = 0, int YMouse = 0)
    {
      int TowersAtCurrentPage = GetNumberOfTowersAtPage(_CurrentShopPage);
      int offset = 0;
      for (int j = 0; j < Settings.LinesInOnePage; j++)
      {
        int TowersInThisLane = (TowersAtCurrentPage - j * Settings.MaxTowersInLine) >= Settings.MaxTowersInLine ?
          Settings.MaxTowersInLine :
          TowersAtCurrentPage - j * Settings.MaxTowersInLine;
        for (int i = 0; i < TowersInThisLane; i++)
        {
          if (Act(i, j, offset, XMouse, YMouse))
            return true;
          offset++;
        }
      }
      return false;
    }
    #endregion

    /// <summary>
    /// Проверка при попытке постановки башни, входит ли в границы массива
    /// </summary>
    /// <param name="Pos">Проверяемый элемент карты</param>
    /// <param name="Simple">Если True, то проверять три клетки справа и внизу не нужно</param>
    /// <returns>Результат проверки</returns>
    internal bool Check(Point Pos, bool Simple = false)
    {
      Pos.X += _Map.VisibleXStart;
      Pos.Y += _Map.VisibleYStart;
      if (((Pos.X >= 0) && (Pos.X < _Map.Width - 1)) && ((Pos.Y >= 0) && (Pos.Y < _Map.Height - 1)))
      {
        if (Simple)
          return true;
        for (int Dx = 0; Dx <= 1; Dx++)
          for (int Dy = 0; Dy <= 1; Dy++)
          {
            if (_Map.GetMapElemStatus(Pos.X + Dx, Pos.Y + Dy) != MapElemStatus.CanBuild)//Если не свободное для постановки место
              return false;
          }
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Добавление врага
    /// </summary>
    private void AddMonster()
    {
      _Monsters.Add(new TMonster(CurrentLevelConf, _Map.Way, MonstersCreated, Scaling));
      MonstersCreated++;
    }

    /// <summary>
    /// Число вышек на выбраной странице магазина
    /// </summary>
    /// <param name="PageNumber">Номер страницы магазина</param>
    /// <returns>Число вышек на странице</returns>
    private int GetNumberOfTowersAtPage(int PageNumber = 1)
    {
      return (PageCount != PageNumber)
       ? (Settings.LinesInOnePage * Settings.MaxTowersInLine) :
       _TowerParamsForBuilding.Count - (PageNumber - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine);
    }

    /// <summary>
    /// Освобождение таймера
    /// </summary>
    public void GetFreedomToTimer()
    {
      GameTimer.Tick -= Timer_Tick;
      this.GameTimer.Stop();
    }

    /// <summary>
    /// Обработка проигрыша
    /// </summary>
    private void Looser()
    {
      GetFreedomToTimer();
      Lose = true;
      GraphicEngine.Show(this, GameDrawingSpace);
    }

    /// <summary>
    /// Игровой таймер
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">EventArgs</param>
    private void Timer_Tick(object sender, EventArgs e)
    {
      if (_LevelStarted)
      {
        #region Действия башен(Выстрелы, подсветка невидимых юнитов)
        //Создание снарядов
        foreach (TTower Tower in _Towers)
        {
          _Missels.AddRange(Tower.GetAims(_Monsters));
        }
        #endregion
        #region Движение монстров
        foreach (TMonster Monster in _Monsters)
        {
          Point Tmp = Monster.GetArrayPos;
          _Map.SetMapElemStatus(Tmp.X, Tmp.Y, MapElemStatus.CanMove);
          int Dx = 0;//Для определения, можно ли двигаться далее(т.е нет ли впереди дургого монстра)
          int Dy = 0;
          #region Определение перемещения
          switch (Monster.GetDirection)
          {
            case MonsterDirection.Up:
              Dy = -1;
              break;
            case MonsterDirection.Right:
              Dx = 1;
              break;
            case MonsterDirection.Down:
              Dy = 1;
              break;
            case MonsterDirection.Left:
              Dx = -1;
              break;
          }
          #endregion
          if (((Tmp.Y + Dy <= _Map.Height) && (Tmp.Y + Dy >= 0)) && ((Tmp.X + Dx <= _Map.Width) && (Tmp.X + Dx >= 0))
            && (_Map.GetMapElemStatus(Tmp.X + Dx, Tmp.Y + Dy) == MapElemStatus.CanMove))//Блокировка более быстрых объектов более медленными
            Monster.Move(true);//Перемещается
          else
            Monster.Move(false);//Тормозится
          if (Monster.NewLap)//Если монстр прошёл полный круг
          {
            Monster.NewLap = false;
            _NumberOfLives--;
            if (_NumberOfLives == 0)
            {
              Looser();
              return;//выходим
            }
          }
          Tmp = Monster.GetArrayPos;
          _Map.SetMapElemStatus(Tmp.X, Tmp.Y, MapElemStatus.BusyByUnit);
        }
        #endregion
        #region Добавление монстров(после движения, чтобы мы могли добавить монстра сразу же после освобождения начальной клетки)
        if ((MonstersCreated != NumberOfMonstersAtLevel[CurrentLevelNumber - 1]) && (_Map.GetMapElemStatus(_Map.Way[0].X, _Map.Way[0].Y) == MapElemStatus.CanMove))
        {
          AddMonster();//Если слишком много монстров создаётся, но при этом ещё подкидываются новые монстры как создающиеся
          //Т.е игрок не убивает монстров за проход по кругу, причём они ещё создаются - это проблемы игрока, полчит наслаивающихся монстров
          //Ибо нефиг быть таким днищем(фича, а не баг)
          _Map.SetMapElemStatus(_Map.Way[0].X, _Map.Way[0].Y, MapElemStatus.BusyByUnit);
        }
        #endregion
      }
      if (System.Windows.Forms.Control.MouseButtons == System.Windows.Forms.MouseButtons.Middle)
        if (MapAreaChanging(GameDrawingSpace.PointToClient(System.Windows.Forms.Control.MousePosition)))
          GraphicEngine.RepaintConstImage = true;
      GraphicEngine.Show(this, null);
      #region Удаление объектов, которые больше не нужны(например снаряд добил монстра)
      Predicate<TMonster> Predicate = (Monster) =>
      {
        if (Monster.DestroyMe)
        {
          _Gold += GoldForKillMonster[CurrentLevelNumber - 1];
          Map.SetMapElemStatus(Monster.GetArrayPos.X, Monster.GetArrayPos.Y, MapElemStatus.CanMove);
          return true;
        }
        else
          return false;
      };
      Monsters.RemoveAll(Predicate);
      Missels.RemoveAll((Missle) => Missle.DestroyMe);
      #endregion
    }
    #endregion
  }
}
