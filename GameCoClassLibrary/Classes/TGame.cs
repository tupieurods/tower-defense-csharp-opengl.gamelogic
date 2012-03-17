#define Debug

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using GameCoClassLibrary.Enums;

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
    private List<TowerParam> TowerParamsForBuilding;//Параметры башен
    private List<TMonster> Monsters;//Список с монстрами на текущем уровне(!НЕ КОНФИГУРАЦИИ ВСЕХ УРОВНЕЙ)
    private List<TTower> Towers;//Список башен(поставленных на карте)
    private List<TMissle> Missels;
    #endregion

    #region Static

    #endregion

    #region Graphics
    private System.Windows.Forms.PictureBox GameDrawingSpace;//Picture Box для отрисовки
    private BufferedGraphics GraphicalBuffer;
    private Bitmap ConstantMapImage;//Постоянное изображение карты для данного масштаба(+ к производительности и решение проблемы утечки памяти)
    private float GameScale = 1.0F;//Масштаб, используемый в игре
    private Color BackgroundColor = Color.Silver;//Цвет заднего фона
    #endregion

    #region TowerShop
    private int TowerConfSelectedID = -1;//Номер выбраной конфигурации в магазине(!NOT AT THE MAP!)
    private Point ArrayPosForTowerStanding = new Point(-1, -1);//НЕ НАСТОЯЩАЯ ПОЗИЦИЯ В МАССИВЕ КАРТЫ!, нужно ещё пересчитывать с учётом смещения
    private int CurrentShopPage = 1;//Текущая страница магазина
    private int PageCount = 1;//Сколько всего страниц
    private const int LinesInOnePage = 3;//Максимальное число строк в странице магазина
    private const int MaxTowersInLine = 5;//Максимально число башен в строке магазина
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
    private int Gold;//Золото игрока
    private int NumberOfLives;//Число монстров которых можно пропустить
    private bool LevelStarted = false;//Начат уровень или нет
    private TMap Map;//Карта
    #endregion

    //Tower on Map selection
    private int TowerMapSelectedID = -1;//Номер выбраной вышки на карте(для башен ID значит номер в массиве)

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
          ConstantMapImage = new Bitmap(Convert.ToInt32((Map.VisibleXFinish - Map.VisibleXStart) * Settings.ElemSize * GameScale),
            Convert.ToInt32((Map.VisibleYFinish - Map.VisibleYStart) * Settings.ElemSize * GameScale));//Единственное место где явно используется GameScale
          ConstantMapImage.Tag = 0;
          GameDrawingSpace.Width = Convert.ToInt32(/*GameDrawingSpace.Width*/ 730 * Scaling);
          GameDrawingSpace.Height = Convert.ToInt32(/*GameDrawingSpace.Height*/600 * Scaling);
          //Создание буфера кадров
          BufferedGraphicsContext CurrentContext;
          CurrentContext = BufferedGraphicsManager.Current;
          GraphicalBuffer = CurrentContext.Allocate(GameDrawingSpace.CreateGraphics(), new Rectangle(new Point(0, 0), GameDrawingSpace.Size));
          foreach (TMonster Monster in Monsters)
          {
            Monster.Scaling = value;
          }
          foreach (TTower Tower in Towers)
          {
            Tower.Scaling = value;
          }
          TMissle.Scaling = value;
          Map.Scaling = value;
        }
      }
    }
    public bool Lose
    {
      get;
      private set;
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
      Monsters = new List<TMonster>();
      Towers = new List<TTower>();
      TowerParamsForBuilding = new List<TowerParam>();
      Missels = new List<TMissle>();
      //дополнительные инициализации
      Lose = false;
      //Загрузили карту
      Map = new TMap(Environment.CurrentDirectory + "\\Data\\Maps\\" + Convert.ToString(GameSettings[0]).Substring(Convert.ToString(GameSettings[0]).LastIndexOf('\\')), true);
      //В будущем изменить масштабирование, чтобы не было лишней площади
      GameDrawingSpace = PBForDraw;
      #region Загрузка параметров башен
      DirectoryInfo DIForLoad = new DirectoryInfo(Environment.CurrentDirectory + "\\Data\\Towers\\" + Convert.ToString(GameSettings[1]));
      FileInfo[] TowerConfigs = DIForLoad.GetFiles();
      foreach (FileInfo i in TowerConfigs)
      {
        if (TowerParamsForBuilding.Count == 90)//Если будет больше 90 башен то у меня печальные новости для дизайнера
          break;
        if (i.Extension == ".tdtc")
        {
          using (FileStream TowerConfLoadStream = new FileStream(i.FullName, FileMode.Open, FileAccess.Read))
          {
            IFormatter Formatter = new BinaryFormatter();
            TowerParamsForBuilding.Add((TowerParam)Formatter.Deserialize(TowerConfLoadStream));
          }
        }
      }
      PageCount = (TowerParamsForBuilding.Count % Settings.ElemSize == 0) ? TowerParamsForBuilding.Count / Settings.ElemSize : (TowerParamsForBuilding.Count / Settings.ElemSize) + 1;
      #endregion
      //Число уровней, жизни
      LevelsNumber = (int)GameSettings[2];
      Gold = (int)GameSettings[4];
#if Debug
      Gold = 1000;
#endif
      NumberOfLives = (int)GameSettings[5];
      //Настройка и запуск таймера
      this.GameTimer = GameTimer;
      this.GameTimer.Tick += new System.EventHandler(Timer_Tick);
      this.GameTimer.Interval = 30;//1;
      Scaling = 1F;
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

    //По поводу графических констант, +40 допустим, после прекращения разработки я сам еле вспомню почему именно столько
    //необходимо на отдельно листе нарисовать(Уже нарисовано) и везде показать почему именно столько

    //Графическая часть полностью в это регионе
    //КРОМЕ ОДНОЙ!!!
    //Вывод Page Selector'а и самой страницы магазина башен производится в ShopPage*Action
    //Если кто предложит лучший вариант передалю
    #region Graphical Part
    /// <summary>
    /// Вызывает все процедуры вывода
    /// Основная процедура, перерисовывает весь игровой экран
    /// </summary>
    /// <param name="LinkToImage">Нужно ли делать постоянным для Picture Box'а или использовать более быстрый вывод</param>
    private void Show(bool LinkToImage = false)
    {
      Graphics Canva;
      Bitmap DrawingBitmap = null;
      if (LinkToImage)
      {
        DrawingBitmap = new Bitmap(Convert.ToInt32(730 * Scaling), Convert.ToInt32(600 * Scaling));
        Canva = Graphics.FromImage(DrawingBitmap);
      }
      else
      {
        Canva = GraphicalBuffer.Graphics;
      }
      //Залили одним цветом
      Canva.FillRectangle(new SolidBrush(BackgroundColor), 0, 0, Convert.ToInt32(730 * Scaling), Convert.ToInt32(600 * Scaling));
      //Вывели карту
      MapAreaShowing(Canva);
      //StartLevelButton
      BStartLevelShow(Canva);
      GraphicalBuffer.Graphics.DrawString(Monsters.Count.ToString(), new Font("Arial", Settings.ElemSize), new SolidBrush(Color.Black), new Point(0, 0));
      #region Вывод GUI
      {
        //Вывели линию разделения
        Canva.DrawLine(new Pen(new SolidBrush(Color.White), 3 * Scaling), new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * Scaling), 0),
              new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * Scaling), Convert.ToInt32(600 * Scaling)));
        ShowMoney(Canva);//Деньги
        ShowLives(Canva);//жизни
        ShowPageSelector(Canva);//Меню магазина
        ShowTowerShopPage(Canva);//Страница магазина
        if ((TowerConfSelectedID != -1) || (TowerMapSelectedID != -1))//Нужно ли выводить параметры
          ShowTowerParams(Canva);
      }
      #endregion
      if (LinkToImage)
      {
        GameDrawingSpace.Image = DrawingBitmap;
        GraphicalBuffer.Graphics.DrawImage(DrawingBitmap, 0, 0);
      }
      else
      {
        GraphicalBuffer.Render();
      }
    }

    /// <summary>
    /// Перерисовка области карты
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void MapAreaShowing(Graphics Canva)
    {
      //Если нужно изменить "неизменяемую" область карты
      if (Convert.ToInt32(ConstantMapImage.Tag) == 0)
      {
        Map.GetConstantBitmap(ConstantMapImage, (int)(Settings.MapAreaSize * Scaling), (int)(Settings.MapAreaSize * Scaling));
        ConstantMapImage.Tag = 1;
        if (!LevelStarted)//Сделано для того, чтобы избавиться от утечки памяти в 30+ мегабайт при перемещении видимой области карты
          //Если уровень начат, то сборщик сам довольно часто вызывается и память не замусоривается
          //Если уровень не начат неизвестно как долго память остаётся забитой мусором(проверено на практике)
          GC.Collect();
      }
      //Ограничиваем область для рисования
      Canva.Clip = new Region(new Rectangle(Settings.DeltaX, Settings.DeltaY, Convert.ToInt32((Map.VisibleXFinish - Map.VisibleXStart) * Settings.ElemSize * Scaling),
        Convert.ToInt32((Map.VisibleYFinish - Map.VisibleYStart) * Settings.ElemSize * Scaling)));
      //Выводим карту
      Canva.DrawImage(ConstantMapImage, Settings.DeltaX, Settings.DeltaY, ConstantMapImage.Width, ConstantMapImage.Height);
      Point VisibleStart = new Point(Map.VisibleXStart, Map.VisibleYStart);
      Point VisibleFinish = new Point(Map.VisibleXFinish, Map.VisibleYFinish);
      #region Вывод изображений башен
      foreach (TTower Tower in Towers)
      {
        Tower.ShowTower(Canva, VisibleStart, VisibleFinish, Settings.DeltaX, Settings.DeltaY);
      }
      #endregion
      #region Вывод изображений монстров
      foreach (TMonster Monster in Monsters)
      {
        if (!Monster.DestroyMe)
          Monster.ShowMonster(Canva, VisibleStart, VisibleFinish, Settings.DeltaX, Settings.DeltaY);
      }
      Predicate<TMonster> Predicate = (Monster) =>
      {
        if (Monster.DestroyMe)
        {
          Gold += GoldForKillMonster[CurrentLevelNumber - 1];
          Map.SetMapElemStatus(Monster.GetArrayPos.X, Monster.GetArrayPos.Y, MapElemStatus.CanMove);
          return true;
        }
        else
          return false;
      };
      Monsters.RemoveAll(Predicate);
      #endregion
      //Следующий далее region переделать, чтобы был один вариант вызова, просто от нескольких переменных(если получится)
      #region Вывод таких вещей как попытка постановки башни или выделение поставленой
      if (ArrayPosForTowerStanding.X != -1)
      {
        if (Check(ArrayPosForTowerStanding))
          ShowSquareAndCircleAtTower(Canva, ArrayPosForTowerStanding,
            TowerParamsForBuilding[TowerConfSelectedID].UpgradeParams[0].AttackRadius, Color.White);
        else
        {
          ShowSquareAndCircleAtTower(Canva, ArrayPosForTowerStanding,
            TowerParamsForBuilding[TowerConfSelectedID].UpgradeParams[0].AttackRadius, Color.Red);
        }
      }
      else if (TowerMapSelectedID != -1)
      {
        //if (Check(Towers[TowerMapSelectedID].ArrayPos, true))
        ShowSquareAndCircleAtTower(Canva, new Point(Towers[TowerMapSelectedID].ArrayPos.X - Map.VisibleXStart,
          Towers[TowerMapSelectedID].ArrayPos.Y - Map.VisibleYStart),
          Towers[TowerMapSelectedID].CurrentTowerParams.AttackRadius, Color.White);
        /*else
        {
          ShowSquareAndCircleAtTower(Canva, new Point(Towers[TowerMapSelectedID].ArrayPos.X - Map.VisibleXStart,
            Towers[TowerMapSelectedID].ArrayPos.Y - Map.VisibleYStart),
            Towers[TowerMapSelectedID].CurrentTowerParams.AttackRadius, Color.Red);
        }*/
      }
      #endregion
      #region Вывод снарядов
      foreach (TMissle Missle in Missels)
      {
        if (!Missle.DestroyMe)
        {
          Missle.Move(Monsters);
          Missle.Show(GraphicalBuffer.Graphics, VisibleStart, VisibleFinish, Monsters);
        }
      }
      Missels.RemoveAll((Missle) => Missle.DestroyMe);
      #endregion
      Canva.Clip = new Region();
    }

    #region Сектор башен
    /// <summary>
    /// Вывод Selector'а страниц в магазине башен
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void ShowPageSelector(Graphics Canva)
    {
      if (TowerParamsForBuilding.Count > Settings.ElemSize)
      {
        ShopPageSelectorAction(ProcAction.Show, Canva);
      }
    }

    /// <summary>
    /// Показ страницы магазина
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void ShowTowerShopPage(Graphics Canva)
    {
      ShopPageAction(ProcAction.Show, GraphicalBuffer.Graphics);
    }

    /// <summary>
    /// Вывод параметров выделенной(в магазине или на карте) пушки
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void ShowTowerParams(Graphics Canva)
    {
      string StrToShow = "";
      if (TowerConfSelectedID != -1)//Выводим информацию о покупаемой башне
      {
        StrToShow = TowerParamsForBuilding[TowerConfSelectedID].ToString() + TowerParamsForBuilding[TowerConfSelectedID].UpgradeParams[0].ToString();
        //Т.к эта башня ещё не куплена, то надо вывести ещё стоимость
        Canva.DrawString("Cost: " + TowerParamsForBuilding[TowerConfSelectedID].UpgradeParams[0].Cost,
          new Font("Arial", Settings.ElemSize * Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * Scaling) + 5, Convert.ToInt32(390 * Scaling)));
      }
      if (TowerMapSelectedID != -1)//Если выводим информацию о поставленной башне
      {
        StrToShow = Towers[TowerMapSelectedID].ToString();//Строка вывода
        //Иконка башни
        Canva.DrawImage(Towers[TowerMapSelectedID].Icon, Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * Scaling) + 5, Convert.ToInt32(375 * Scaling),
          Towers[TowerMapSelectedID].Icon.Width * Scaling, Towers[TowerMapSelectedID].Icon.Height * Scaling);
        //Выводим текущий уровень башни
        Canva.DrawString("Level: " + Towers[TowerMapSelectedID].Level.ToString(),
          new Font("Arial", Settings.ElemSize * Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Towers[TowerMapSelectedID].Icon.Width + Settings.DeltaX * 2) * Scaling) + 5,
            Convert.ToInt32((375 + Towers[TowerMapSelectedID].Icon.Height / 2) * Scaling)));
        //Кнопки Destroy и Upgrade
        BDestroyShow(Canva);
        BUpgradeShow(Canva);
      }
      //Характеристики
      Canva.DrawString(StrToShow,
          new Font("Arial", 10 * Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * Scaling) + 5, Convert.ToInt32(415 * Scaling)));
      //Рамка для красоты
      Canva.DrawRectangle(new Pen(Color.Black), Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * Scaling) + 5, Convert.ToInt32(415 * Scaling),
      Convert.ToInt32((200 - Settings.DeltaX * 2) * Scaling), Convert.ToInt32((184) * Scaling));
    }

    /// <summary>
    /// Вывод квадрата и радиуса атаки вокруг установленой/пытающейся установиться башни
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    /// <param name="Position">Левый верхний квадрат для башни</param>
    /// <param name="Radius">Радиус атаки</param>
    /// <param name="CircleColor">Цвет круга</param>
    private void ShowSquareAndCircleAtTower(Graphics Canva, Point Position, int Radius, Color CircleColor)
    {
      //Квадрат
      Canva.DrawRectangle(new Pen(Color.Black), ((Position.X) * Settings.ElemSize) * Scaling + Settings.DeltaX,
          ((Position.Y) * Settings.ElemSize) * Scaling + Settings.DeltaY, Settings.ElemSize * 2 * Scaling, Settings.ElemSize * 2 * Scaling);
      //Радиус атаки
      Canva.DrawEllipse(new Pen(CircleColor), ((Position.X + 1) * Settings.ElemSize - Radius) * Scaling + Settings.DeltaX,
          ((Position.Y + 1) * Settings.ElemSize - Radius) * Scaling + Settings.DeltaY, Radius * 2 * Scaling, Radius * 2 * Scaling);
    }
    #endregion

    #region Вывод кнопочек
    /// <summary>
    /// Показ кнопки начать новый уровень
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void BStartLevelShow(Graphics Canva)
    {
      if (LevelStarted)
      {
        Canva.DrawImage(Res.BStartLevelDisabled, THelpers.BuildRect(RectBuilder.NewLevelDisabled, Scaling));
      }
      else
      {
        Canva.DrawImage(Res.BStartLevelEnabled, THelpers.BuildRect(RectBuilder.NewLevelEnabled, Scaling));
      }
    }

    /// <summary>
    /// Уничтожить башню
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void BDestroyShow(Graphics Canva)
    {
      Canva.DrawImage(Res.BDestroyTower, THelpers.BuildRect(RectBuilder.Destroy, Scaling));
    }

    /// <summary>
    /// Показ кнопки улучшить башню
    /// </summary>
    /// <param name="Canva"></param>
    private void BUpgradeShow(Graphics Canva)
    {
      if (Towers[TowerMapSelectedID].CanUpgrade)
      {
        //Вводится Tmp, т.к этот прямоугольник будет использоваться три раза
        Rectangle Tmp = THelpers.BuildRect(RectBuilder.Upgrade, Scaling);
        Canva.DrawImage(Res.BUpgradeTower, Tmp);
        Canva.DrawString("Upgrade cost: " + Towers[TowerMapSelectedID].GetUpgradeCost,
          new Font("Arial", Settings.ElemSize * Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * Scaling) + 3, Tmp.Y - Convert.ToInt32(25 * Scaling)));
      }
    }
    #endregion

    #region Информация для игрока
    /// <summary>
    /// Вывод числа жизней
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void ShowLives(Graphics Canva)
    {
      Canva.DrawString("Lives: " + NumberOfLives.ToString(), new Font("Arial", 14 * Scaling), new SolidBrush(Color.Black),
        new Point(Convert.ToInt32((Settings.MapAreaSize + 10 + Settings.DeltaX * 2) * Scaling), Convert.ToInt32((Res.MoneyPict.Height + 10) * Scaling)));
    }
    /// <summary>
    /// Вывод количества денег 
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void ShowMoney(Graphics Canva)
    {
      //Изображение монеты
      Canva.DrawImage(Res.MoneyPict, Convert.ToInt32((Settings.MapAreaSize + 10 + Settings.DeltaX * 2) * Scaling), Convert.ToInt32(5 * Scaling),
        Res.MoneyPict.Width * Scaling, Res.MoneyPict.Height * Scaling);
      //Вывод числа денег
      Canva.DrawString(Gold.ToString(), new Font("Arial", 14 * Scaling), new SolidBrush(Color.Black),
        new Point(Convert.ToInt32((Settings.MapAreaSize + 10 + Res.MoneyPict.Width + Settings.DeltaX * 2) * Scaling), Convert.ToInt32(9 * Scaling)));
    }
    #endregion
    #endregion

    #region Обработка действий пользователя
    /// <summary>
    /// Обработка нажатия кнопки мыши
    /// </summary>
    /// <param name="e">System.Windows.Forms.MouseEventArgs</param>
    public void MouseUp(System.Windows.Forms.MouseEventArgs e)
    {
      bool Flag = false;
      #region Если уровень ещё не начат и игрок захотел начать
      if ((!LevelStarted) && (CurrentLevelNumber < LevelsNumber))
      {
        if (THelpers.BuildRect(RectBuilder.NewLevelEnabled, Scaling).Contains(e.X, e.Y))
        {
          LevelStarted = true;
          CurrentLevelNumber++;
          MonstersCreated = 0;
          Monsters.Clear();
          #region Загружаем конфигурацию уровня
          FileStream LevelLoadStream = new FileStream(PathToLevelConfigurations, FileMode.Open, FileAccess.Read);
          IFormatter Formatter = new BinaryFormatter();
          LevelLoadStream.Seek(Position, SeekOrigin.Begin);
          CurrentLevelConf = (MonsterParam)(Formatter.Deserialize(LevelLoadStream));
          Position = LevelLoadStream.Position;
          LevelLoadStream.Close();
          #endregion
          Flag = true;
        }
      }
      #endregion
      #region Tower Page Selection
      if ((!Flag) && (TowerParamsForBuilding.Count > Settings.ElemSize) && ((e.X >= Convert.ToInt32((Settings.MapAreaSize + 10 + Settings.DeltaX * 2) * Scaling))
        && (e.Y >= Convert.ToInt32((37 + Res.MoneyPict.Height) * Scaling))
        && (e.Y <= Convert.ToInt32((247 + Res.MoneyPict.Height) * Scaling))))
      {
        Flag = ShopPageSelectorAction(ProcAction.Select, GraphicalBuffer.Graphics, e.X, e.Y);
      }
      #endregion
      #region Tower Selected in Shop
      if ((!Flag) && (e.X >= (Convert.ToInt32((460 + Settings.DeltaX * 2) * Scaling))
        && (e.Y >= Convert.ToInt32((90 + Res.MoneyPict.Height) * Scaling))
        && (e.Y <= Convert.ToInt32((100 + Res.MoneyPict.Height + 42 * ((TowerParamsForBuilding.Count / 5) + 1)) * Scaling))))//Если в границах
      {
        Flag = ShopPageAction(ProcAction.Select, GraphicalBuffer.Graphics, e.X, e.Y);
      }
      #endregion
      #region Если пользователь хочет выделить вышку
      if ((!Flag) && (TowerConfSelectedID == -1)
        && ((e.X >= Settings.DeltaX) && (e.X <= (int)(Settings.MapAreaSize * Scaling) + Settings.DeltaX) && (e.Y >= Settings.DeltaY) && (e.Y <= (int)(Settings.MapAreaSize * Scaling) + Settings.DeltaY)))
      {
        switch (e.Button)
        {
          case System.Windows.Forms.MouseButtons.Left:
            Point ArrPos = new Point((e.X - Settings.DeltaX) / Convert.ToInt32(Settings.ElemSize * Scaling),
              (e.Y - Settings.DeltaY) / Convert.ToInt32(Settings.ElemSize * Scaling));
            if (!Check(ArrPos, true))
              break;
            if (Map.GetMapElemStatus(ArrPos.X + Map.VisibleXStart, ArrPos.Y + Map.VisibleYStart) == MapElemStatus.BusyByTower)
            {
              for (int i = 0; i < Towers.Count; i++)
              {
                if (Towers[i].Contain(new Point(ArrPos.X + Map.VisibleXStart, ArrPos.Y + Map.VisibleYStart)))
                {
                  TowerMapSelectedID = i;
                  Flag = true;
                  return;
                }
              }
            }
            break;
          case System.Windows.Forms.MouseButtons.Right:
            if (TowerMapSelectedID != -1)
            {
              FinishTowerMapSelectAct();
            }
            break;
        }
      }
      #endregion
      #region Если пользователь хочет поставить вышку
      if (TowerConfSelectedID != -1)//Если !=-1 значит в границах карты и Flag=false 100%
      {
        switch (e.Button)
        {
          case System.Windows.Forms.MouseButtons.Left:
            if (Check(ArrayPosForTowerStanding, false) && (Gold >= TowerParamsForBuilding[TowerConfSelectedID].UpgradeParams[0].Cost))
            {
              Towers.Add(new TTower(TowerParamsForBuilding[TowerConfSelectedID],
                new Point(ArrayPosForTowerStanding.X + Map.VisibleXStart, ArrayPosForTowerStanding.Y + Map.VisibleYStart), Scaling));
              Gold -= TowerParamsForBuilding[TowerConfSelectedID].UpgradeParams[0].Cost;
              for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                  Map.SetMapElemStatus(ArrayPosForTowerStanding.X + i + Map.VisibleXStart,
                    ArrayPosForTowerStanding.Y + j + Map.VisibleYStart, MapElemStatus.BusyByTower);
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
      if (TowerMapSelectedID != -1)
      {
        if (THelpers.BuildRect(RectBuilder.Destroy, Scaling).Contains(e.X, e.Y))
        {
          for (int i = 0; i < 2; i++)
          {
            for (int j = 0; j < 2; j++)
            {
              Map.SetMapElemStatus(Towers[TowerMapSelectedID].ArrayPos.X + i, Towers[TowerMapSelectedID].ArrayPos.Y + j, MapElemStatus.CanBuild);
            }
          }
          Towers.RemoveAt(TowerMapSelectedID);
          FinishTowerMapSelectAct();
        }
        else if ((THelpers.BuildRect(RectBuilder.Upgrade, Scaling).Contains(e.X, e.Y)) && (Towers[TowerMapSelectedID].CanUpgrade) &&
          Towers[TowerMapSelectedID].CurrentTowerParams.Cost <= Gold)
        {
          Gold -= Towers[TowerMapSelectedID].Upgrade();
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
      if ((Map.Width <= 30) || (Map.Height <= 30))
        return false;
      if (((Position.X > Settings.DeltaX) && (Position.X < (Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaX)))
        && ((Position.Y > Settings.DeltaY) && (Position.Y < (Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaY))))
      {
        if ((Position.X - Settings.DeltaX < Settings.ElemSize) && (Map.VisibleXStart != 0))
        {
          Map.ChangeVisibleArea(-1, 0);
          return true;
        }
        if ((Position.Y - Settings.DeltaY < Settings.ElemSize) && (Map.VisibleYStart != 0))
        {
          Map.ChangeVisibleArea(0, -1);
          return true;
        }
        if (((-Position.X + Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaX) < Settings.ElemSize) && (Map.VisibleXFinish != Map.Width))
        {
          Map.ChangeVisibleArea(1, 0);
          return true;
        }
        if (((-Position.Y + Convert.ToInt32(Settings.MapAreaSize * Scaling) + Settings.DeltaY) < Settings.ElemSize) && (Map.VisibleYFinish != Map.Height))
        {
          Map.ChangeVisibleArea(0, 1);
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
      if ((TowerConfSelectedID != -1) && (new Rectangle(Convert.ToInt32(Settings.DeltaX * Scaling), Convert.ToInt32(Settings.DeltaY * Scaling),
        Convert.ToInt32(Settings.MapAreaSize * Scaling), Convert.ToInt32(Settings.MapAreaSize * Scaling)).Contains(e.X, e.Y)))
      {
        ArrayPosForTowerStanding =
          new Point((e.X - Settings.DeltaX) / Convert.ToInt32(Settings.ElemSize * Scaling),
                    (e.Y - Settings.DeltaY) / Convert.ToInt32(Settings.ElemSize * Scaling));
        if (!Check(ArrayPosForTowerStanding, true))
          ArrayPosForTowerStanding = new Point(-1, -1);
      }
      else
        ArrayPosForTowerStanding = new Point(-1, -1);
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
      TowerMapSelectedID = -1;
    }

    /// <summary>
    /// Если поставили вышку или отменили её поставку
    /// </summary>
    private void FinishTowerShopAct()
    {
      TowerConfSelectedID = -1;
      ArrayPosForTowerStanding = new Point(-1, -1);
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
    private bool ShopPageSelectorAction(ProcAction Act, Graphics Canva, int XMouse = 0, int YMouse = 0)
    {
      int DY = 0;//Если больше одного ряда страниц будет изменена в процессе цикла
      Func<int, Rectangle> LambdaBuildRect = (x) =>
        new Rectangle(Convert.ToInt32((Settings.MapAreaSize + 10 + (x % 3) * ("Page " + (x + 1).ToString()).Length * 12 + Settings.DeltaX * 2) * Scaling),
             Convert.ToInt32((Res.MoneyPict.Height + 35 * (DY + 1)) * Scaling), Convert.ToInt32(("Page " + (x + 1).ToString()).Length * 11 * Scaling), Convert.ToInt32(24 * Scaling));
      for (int i = 0; i < PageCount; i++)
      {
        if ((i != 0) && (i % 3 == 0))
          DY++;
        switch (Act)
        {
          case ProcAction.Show:
            //Строка
            Canva.DrawString("Page " + (i + 1).ToString(), new Font("Arial", 14 * Scaling), new SolidBrush(Color.Black),
              new Point(Convert.ToInt32((Settings.MapAreaSize + 10 + (i % 3) * ("Page " + (i + 1).ToString()).Length * 12 + Settings.DeltaX * 2) * Scaling),
                Convert.ToInt32((Res.MoneyPict.Height + 35 * (DY + 1)) * Scaling)));//Эта часть с new Point сильно раздражает
            //но как убрать и сделать красивее пока не знаю
            //Вывод рамки
            Color PenColor = ((i + 1) == CurrentShopPage) ? Color.Red : Color.White;
            Canva.DrawRectangle(new Pen(PenColor, 2 * Scaling), LambdaBuildRect(i));
            break;
          case ProcAction.Select:
            if (LambdaBuildRect(i).Contains(XMouse, YMouse))
            {
              CurrentShopPage = i + 1;
              FinishTowerShopAct();
              return true;
            }
            break;
        }
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
    private bool ShopPageAction(ProcAction Act, Graphics Canva, int XMouse = 0, int YMouse = 0)
    {
      int TowersAtCurrentPage = GetNumberOfTowersAtPage(CurrentShopPage);
      int offset = 0;
      Func<int, int, Rectangle> LambdaBuildRect = (x, y) => new Rectangle(Convert.ToInt32((Settings.MapAreaSize + 10 + x * 42 + Settings.DeltaX * 2) * Scaling),
                    Convert.ToInt32((60 + Res.MoneyPict.Height + y * 42 + 40) * Scaling), Convert.ToInt32(32 * Scaling), Convert.ToInt32(32 * Scaling));
      for (int j = 0; j < LinesInOnePage; j++)
      {
        int TowersInThisLane = (TowersAtCurrentPage - j * MaxTowersInLine) >= MaxTowersInLine ? MaxTowersInLine : TowersAtCurrentPage - j * MaxTowersInLine;
        for (int i = 0; i < TowersInThisLane; i++)
        {
          switch (Act)
          {
            case ProcAction.Show:
              Canva.DrawImage(TowerParamsForBuilding[(CurrentShopPage - 1) * (LinesInOnePage * MaxTowersInLine) + offset].Icon, LambdaBuildRect(i, j));
              if (TowerConfSelectedID == (CurrentShopPage - 1) * (LinesInOnePage * MaxTowersInLine) + offset)//Если эта башня выбрана в магазине
                //обозначим это графически
                GraphicalBuffer.Graphics.DrawRectangle(new Pen(Color.Red, 3 * Scaling), LambdaBuildRect(i, j));
              break;
            case ProcAction.Select:
              if (LambdaBuildRect(i, j).Contains(XMouse, YMouse))//Если нашли выделенную башню
              {
                FinishTowerMapSelectAct();
                TowerConfSelectedID = (CurrentShopPage - 1) * (LinesInOnePage * MaxTowersInLine) + offset;
                return true;
              }
              break;
          }
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
    private bool Check(Point Pos, bool Simple = false)
    {
      Pos.X += Map.VisibleXStart;
      Pos.Y += Map.VisibleYStart;
      if (((Pos.X >= 0) && (Pos.X < Map.Width - 1)) && ((Pos.Y >= 0) && (Pos.Y < Map.Height - 1)))
      {
        if (Simple)
          return true;
        for (int Dx = 0; Dx <= 1; Dx++)
          for (int Dy = 0; Dy <= 1; Dy++)
          {
            if (Map.GetMapElemStatus(Pos.X + Dx, Pos.Y + Dy) != MapElemStatus.CanBuild)//Если не свободное для постановки место
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
      Monsters.Add(new TMonster(CurrentLevelConf, Map.Way, MonstersCreated, Scaling));
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
       ? (LinesInOnePage * MaxTowersInLine) : TowerParamsForBuilding.Count - (PageNumber - 1) * (LinesInOnePage * MaxTowersInLine);
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
      Show(true);
    }

    /// <summary>
    /// Игровой таймер
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">EventArgs</param>
    private void Timer_Tick(object sender, EventArgs e)
    {
      if (LevelStarted)
      {
        #region Действия башен(Выстрелы, подсветка невидимых юнитов)
        //Создание снарядов
        foreach (TTower Tower in Towers)
        {
          Missels.AddRange(Tower.GetAims(Monsters));
        }
        #endregion
        #region Движение монстров
        foreach (TMonster Monster in Monsters)
        {
          Point Tmp = Monster.GetArrayPos;
          Map.SetMapElemStatus(Tmp.X, Tmp.Y, MapElemStatus.CanMove);
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
          if (((Tmp.Y + Dy <= Map.Height) && (Tmp.Y + Dy >= 0)) && ((Tmp.X + Dx <= Map.Width) && (Tmp.X + Dx >= 0))
            && (Map.GetMapElemStatus(Tmp.X + Dx, Tmp.Y + Dy) == MapElemStatus.CanMove))//Блокировка более быстрых объектов более медленными
            Monster.Move(true);//Перемещается
          else
            Monster.Move(false);//Тормозится
          if (Monster.NewLap)//Если монстр прошёл полный круг
          {
            Monster.NewLap = false;
            NumberOfLives--;
            if (NumberOfLives == 0)
            {
              Looser();
              return;//выходим
            }
          }
          Tmp = Monster.GetArrayPos;
          Map.SetMapElemStatus(Tmp.X, Tmp.Y, MapElemStatus.BusyByUnit);
        }
        #endregion
        #region Добавление монстров(после движения, чтобы мы могли добавить монстра сразу же после освобождения начальной клетки)
        if ((MonstersCreated != NumberOfMonstersAtLevel[CurrentLevelNumber - 1]) && (Map.GetMapElemStatus(Map.Way[0].X, Map.Way[0].Y) == MapElemStatus.CanMove))
        {
          AddMonster();//Если слишком много монстров создаётся, но при этом ещё подкидываются новые монстры как создающиеся
          //Т.е игрок не убивает монстров за проход по кругу, причём они ещё создаются - это проблемы игрока, полчит наслаивающихся монстров
          //Ибо нефиг быть таким днищем(фича, а не баг)
          Map.SetMapElemStatus(Map.Way[0].X, Map.Way[0].Y, MapElemStatus.BusyByUnit);
        }
        #endregion
      }
      if (System.Windows.Forms.Control.MouseButtons == System.Windows.Forms.MouseButtons.Middle)
        if (MapAreaChanging(GameDrawingSpace.PointToClient(System.Windows.Forms.Control.MousePosition)))
          ConstantMapImage.Tag = 0;
      Show(false);
    }
    #endregion
  }
}
