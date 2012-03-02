﻿#define Debug

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameCoClassLibrary
{
  enum ProcAction { Show, Select };
  enum RectBuilder { NewLevelEnabled, NewLevelDisabled, Destroy, Upgrade };

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
    //Если кто-то читает эти исходники кроме меня и не понимает где какая картинка, немедленно прекратите его чтение
    //B в начале названия переменной- означает Button
    static private Bitmap MoneyPict, BStartLevelEnabled, BStartLevelDisabled, BDestroyTower, BUpgradeTower;
    #endregion

    #region Graphics
    private System.Windows.Forms.PictureBox GameDrawingSpace;//Picture Box для отрисовки
    private BufferedGraphics GraphicalBuffer;
    private Bitmap ConstantMapImage;//Постоянное изображение карты для данного масштаба(+ к производительности и решение проблемы утечки памяти)
    private float GameScale = 1.0F;//Масштаб, используемый в игре
    private int DeltaX = 10;//Отступы от левого верхнего края для карты
    private int DeltaY = 10;
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
        if /*(GameScale - value <= 0.00001) && */(Convert.ToInt32((value * 15) - Math.Floor(value * 15)) == 0)//Если программист не догадывается что изображение не может содержать
        //не целый пиксель мы защитимся от такого тормоза
        {
          GameScale = value;
          ConstantMapImage = new Bitmap(Convert.ToInt32((Map.VisibleXFinish - Map.VisibleXStart) * 15 * GameScale),
            Convert.ToInt32((Map.VisibleYFinish - Map.VisibleYStart) * 15 * GameScale));//Единственное место где явно используется GameScale
          ConstantMapImage.Tag = 0;
          GameDrawingSpace.Width = Convert.ToInt32(/*GameDrawingSpace.Width*/ 700 * Scaling);
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
    //Предполагается что этот конструктор используется только в игре
    //Соответсвенно должна иметься соостветсвующая структура папок
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
      PageCount = (TowerParamsForBuilding.Count % 15 == 0) ? TowerParamsForBuilding.Count / 15 : (TowerParamsForBuilding.Count / 15) + 1;
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
      this.GameTimer.Interval = 30;
      Scaling = 1F;
      this.GameTimer.Start();
    }

    static TGame()
    {
      MoneyPict = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\Money.png");
      MoneyPict.MakeTransparent();
      BStartLevelEnabled = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\StartLevelEnabled.png");
      BStartLevelEnabled.MakeTransparent();
      BStartLevelDisabled = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\StartLevelDisabled.png");
      BStartLevelDisabled.MakeTransparent();
      BDestroyTower = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\Destroy.png");
      BDestroyTower.MakeTransparent();
      BUpgradeTower = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\Up.png");
      BUpgradeTower.MakeTransparent();
    }
    #endregion

    //Используется фабрика, если произойдёт ошибка мы просто вернём null, а не получим франкинштейна
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
    //Вызывает все процедуры вывода
    //Основная процедура, перерисовывает весь игровой экран
    private void Show(bool LinkToImage = false)
    {
      Graphics Canva;
      Bitmap DrawingBitmap = null;
      if (LinkToImage)
      {
        DrawingBitmap = new Bitmap(Convert.ToInt32(700 * Scaling), Convert.ToInt32(600 * Scaling));
        Canva = Graphics.FromImage(DrawingBitmap);
      }
      else
      {
        Canva = GraphicalBuffer.Graphics;
      }
      //Залили одним цветом область карты
      Canva.FillRectangle(new SolidBrush(BackgroundColor), 0, 0, Convert.ToInt32(700 * Scaling), Convert.ToInt32(600 * Scaling));
      //Вывели карту
      MapAreaShowing(Canva);
      //StartLevelButton
      BStartLevelShow(Canva);
      GraphicalBuffer.Graphics.DrawString(Monsters.Count.ToString(), new Font("Arial", 15), new SolidBrush(Color.Black), new Point(0, 0));
      #region Вывод GUI
      {
        //Вывели линию разделения
        Canva.DrawLine(new Pen(new SolidBrush(Color.White), 3 * Scaling), new Point(Convert.ToInt32((450 + DeltaX * 2) * Scaling), 0),
              new Point(Convert.ToInt32((450 + DeltaX * 2) * Scaling), Convert.ToInt32(700 * Scaling)));
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

    //Перерисовка области карты
    private void MapAreaShowing(Graphics Canva)
    {
      //Если нужно изменить "неизменяемую" область карты
      if (Convert.ToInt32(ConstantMapImage.Tag) == 0)
      {
        Map.GetConstantBitmap(ConstantMapImage, (int)(450 * Scaling), (int)(450 * Scaling));
        ConstantMapImage.Tag = 1;
      }
      //Ограничиваем область для рисования
      Canva.Clip = new Region(new Rectangle(DeltaX, DeltaY, Convert.ToInt32((Map.VisibleXFinish - Map.VisibleXStart) * 15 * Scaling),
        Convert.ToInt32((Map.VisibleYFinish - Map.VisibleYStart) * 15 * Scaling)));
      //Выводим карту
      Canva.DrawImage(ConstantMapImage, DeltaX, DeltaY, ConstantMapImage.Width, ConstantMapImage.Height);
      Point VisibleStart = new Point(Map.VisibleXStart, Map.VisibleYStart);
      Point VisibleFinish = new Point(Map.VisibleXFinish, Map.VisibleYFinish);
      #region Вывод изображений башен
      foreach (TTower Tower in Towers)
      {
        Tower.ShowTower(Canva, VisibleStart, VisibleFinish, DeltaX, DeltaY);
      }
      #endregion
      #region Вывод изображений монстров
      foreach (TMonster Monster in Monsters)
      {
        if (!Monster.DestroyMe)
          Monster.ShowMonster(Canva, VisibleStart, VisibleFinish, DeltaX, DeltaY);
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
    //Вывод Selector'а страниц в магазине башен
    private void ShowPageSelector(Graphics Canva)
    {
      if (TowerParamsForBuilding.Count > 15)
      {
        ShopPageSelectorAction(ProcAction.Show, Canva);
      }
    }

    //Показ страницы магазина
    private void ShowTowerShopPage(Graphics Canva)
    {
      ShopPageAction(ProcAction.Show, GraphicalBuffer.Graphics);
    }

    //Вывод параметров выделенной(в магазине или на карте) пушки
    private void ShowTowerParams(Graphics Canva)
    {
      string StrToShow = "";
      if (TowerConfSelectedID != -1)//Выводим информацию о покупаемой башне
      {
        StrToShow = TowerParamsForBuilding[TowerConfSelectedID].ToString() + TowerParamsForBuilding[TowerConfSelectedID].UpgradeParams[0].ToString();
        //Т.к эта башня ещё не куплена, то надо вывести ещё стоимость
        Canva.DrawString("Cost: " + TowerParamsForBuilding[TowerConfSelectedID].UpgradeParams[0].Cost,
          new Font("Arial", 15 * Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((450 + DeltaX * 2) * Scaling) + 5, Convert.ToInt32(405 * Scaling)));
      }
      if (TowerMapSelectedID != -1)//Если выводим информацию о поставленной башне
      {
        StrToShow = Towers[TowerMapSelectedID].ToString();//Строка вывода
        //Иконка башни
        Canva.DrawImage(Towers[TowerMapSelectedID].Icon, Convert.ToInt32((450 + DeltaX * 2) * Scaling) + 5, Convert.ToInt32(390 * Scaling),
          Towers[TowerMapSelectedID].Icon.Width * Scaling, Towers[TowerMapSelectedID].Icon.Height * Scaling);
        //Выводим текущий уровень башни
        Canva.DrawString("Level: " + Towers[TowerMapSelectedID].Level.ToString(),
          new Font("Arial", 15 * Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((450 + Towers[TowerMapSelectedID].Icon.Width + DeltaX * 2) * Scaling) + 5,
            Convert.ToInt32((390 + Towers[TowerMapSelectedID].Icon.Height / 2) * Scaling)));
        //Кнопки Destroy и Upgrade
        BDestroyShow(Canva);
        BUpgradeShow(Canva);
      }
      //Характеристики
      Canva.DrawString(StrToShow,
          new Font("Arial", 10 * Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((450 + DeltaX * 2) * Scaling) + 5, Convert.ToInt32(430 * Scaling)));
      //Рамка для красоты
      Canva.DrawRectangle(new Pen(Color.Black), Convert.ToInt32((450 + DeltaX * 2) * Scaling) + 5, Convert.ToInt32(430 * Scaling),
      Convert.ToInt32((200 - DeltaX * 2) * Scaling), Convert.ToInt32((169) * Scaling));
    }

    //Вывод квадрата и радиуса атаки вокруг установленой/пытающейся установиться башни
    private void ShowSquareAndCircleAtTower(Graphics Canva, Point Position, int Radius, Color CircleColor)
    {
      //Квадрат
      Canva.DrawRectangle(new Pen(Color.Black), ((Position.X) * 15) * Scaling + DeltaX,
          ((Position.Y) * 15) * Scaling + DeltaY, 30 * Scaling, 30 * Scaling);
      //Радиус атаки
      Canva.DrawEllipse(new Pen(CircleColor), ((Position.X + 1) * 15 - Radius) * Scaling + DeltaX,
          ((Position.Y + 1) * 15 - Radius) * Scaling + DeltaY, Radius * 2 * Scaling, Radius * 2 * Scaling);
    }
    #endregion

    #region Вывод кнопочек
    //Показ кнопки начать новый уровень
    private void BStartLevelShow(Graphics Canva)
    {
      if (LevelStarted)
      {
        Canva.DrawImage(BStartLevelDisabled, BuildRect(RectBuilder.NewLevelDisabled));
      }
      else
      {
        Canva.DrawImage(BStartLevelEnabled, BuildRect(RectBuilder.NewLevelEnabled));
      }
    }

    //Уничтожить башню
    private void BDestroyShow(Graphics Canva)
    {
      Canva.DrawImage(BDestroyTower, BuildRect(RectBuilder.Destroy));
    }

    //Улучшить
    private void BUpgradeShow(Graphics Canva)
    {
      if (Towers[TowerMapSelectedID].CanUpgrade)
      {
        //Вводится Tmp, т.к этот прямоугольник будет использоваться три раза
        Rectangle Tmp = BuildRect(RectBuilder.Upgrade);
        Canva.DrawImage(BUpgradeTower, Tmp);
        Canva.DrawString("Upgrade cost: " + Towers[TowerMapSelectedID].GetUpgradeCost,
          new Font("Arial", 15 * Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((450 + DeltaX * 2) * Scaling) + 3, Tmp.Y - Convert.ToInt32(25 * Scaling)));
      }
    }
    #endregion

    #region Информация для игрока
    //Вывод числа жизней
    private void ShowLives(Graphics Canva)
    {
      Canva.DrawString("Lives: " + NumberOfLives.ToString(), new Font("Arial", 14 * Scaling), new SolidBrush(Color.Black),
        new Point(Convert.ToInt32((460 + DeltaX * 2) * Scaling), Convert.ToInt32((MoneyPict.Height + 10) * Scaling)));
    }
    //Вывод количества денег 
    private void ShowMoney(Graphics Canva)
    {
      //Изображение монеты
      Canva.DrawImage(MoneyPict, Convert.ToInt32((460 + DeltaX * 2) * Scaling), Convert.ToInt32(5 * Scaling),
        MoneyPict.Width * Scaling, MoneyPict.Height * Scaling);
      //Вывод числа денег
      Canva.DrawString(Gold.ToString(), new Font("Arial", 14 * Scaling), new SolidBrush(Color.Black),
        new Point(Convert.ToInt32((460 + MoneyPict.Width + DeltaX * 2) * Scaling), Convert.ToInt32(9 * Scaling)));
    }
    #endregion
    #endregion

    #region Обработка действий пользователя
    public void MouseUp(System.Windows.Forms.MouseEventArgs e)
    {
      bool Flag = false;
      #region Если уровень ещё не начат и игрок захотел начать
      if ((!LevelStarted) && (CurrentLevelNumber < LevelsNumber))
      {
        if (BuildRect(RectBuilder.NewLevelEnabled).Contains(e.X, e.Y))
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
      if ((!Flag) && (TowerParamsForBuilding.Count > 15) && ((e.X >= Convert.ToInt32((460 + DeltaX * 2) * Scaling))
        && (e.Y >= Convert.ToInt32((7 + MoneyPict.Height + 30) * Scaling))
        && (e.Y <= Convert.ToInt32((7 + MoneyPict.Height + 240) * Scaling))))
      {
        Flag = ShopPageSelectorAction(ProcAction.Select, GraphicalBuffer.Graphics, e.X, e.Y);
      }
      #endregion
      #region Tower Selected in Shop
      if ((!Flag) && (e.X >= (Convert.ToInt32((460 + DeltaX * 2) * Scaling))
        && (e.Y >= Convert.ToInt32((50 + MoneyPict.Height + 40) * Scaling))
        && (e.Y <= Convert.ToInt32((60 + MoneyPict.Height + 40 + 42 * ((TowerParamsForBuilding.Count / 5) + 1)) * Scaling))))//Если в границах
      {
        Flag = ShopPageAction(ProcAction.Select, GraphicalBuffer.Graphics, e.X, e.Y);
      }
      #endregion
      #region Если пользователь хочет выделить вышку
      if ((!Flag) && (TowerConfSelectedID == -1)
        && ((e.X >= DeltaX) && (e.X <= (int)((450) * Scaling) + DeltaX) && (e.Y >= DeltaY) && (e.Y <= (int)((450) * Scaling) + DeltaY)))
      {
        switch (e.Button)
        {
          case System.Windows.Forms.MouseButtons.Left:
            Point ArrPos = new Point((e.X - DeltaX) / Convert.ToInt32(15 * Scaling),
              (e.Y - DeltaY) / Convert.ToInt32(15 * Scaling));
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
        if (BuildRect(RectBuilder.Destroy).Contains(e.X, e.Y))
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
        else if ((BuildRect(RectBuilder.Upgrade).Contains(e.X, e.Y)) && (Towers[TowerMapSelectedID].CanUpgrade) &&
          Towers[TowerMapSelectedID].CurrentTowerParams.Cost <= Gold)
        {
          Gold -= Towers[TowerMapSelectedID].Upgrade();
        }
      }
      #endregion
    }

    public bool MapAreaChanging(Point Position)
    {
      #region Перемещение границ карты
      if ((Map.Width <= 30) || (Map.Height <= 30))
        return false;
      if (((Position.X > DeltaX) && (Position.X < (Convert.ToInt32(450 * Scaling) + DeltaX))) && ((Position.Y > DeltaY) && (Position.Y < (Convert.ToInt32(450 * Scaling) + DeltaY))))
      {
        if ((Position.X - DeltaX < 15) && (Map.VisibleXStart != 0))
        {
          Map.ChangeVisibleArea(-1, 0);
          return true;
        }
        if ((Position.Y - DeltaY < 15) && (Map.VisibleYStart != 0))
        {
          Map.ChangeVisibleArea(0, -1);
          return true;
        }
        if (((-Position.X + Convert.ToInt32(450 * Scaling) + DeltaX) < 15) && (Map.VisibleXFinish != Map.Width))
        {
          Map.ChangeVisibleArea(1, 0);
          return true;
        }
        if (((-Position.Y + Convert.ToInt32(450 * Scaling) + DeltaY) < 15) && (Map.VisibleYFinish != Map.Height))
        {
          Map.ChangeVisibleArea(0, 1);
          return true;
        }
      }
      return false;
      #endregion
    }

    public void MouseMove(System.Windows.Forms.MouseEventArgs e)
    {
      #region Обработка перемещения при попытке постановки башни
      if ((TowerConfSelectedID != -1) && (new Rectangle(Convert.ToInt32(DeltaX * Scaling), Convert.ToInt32(DeltaY * Scaling),
        Convert.ToInt32(450 * Scaling), Convert.ToInt32(450 * Scaling)).Contains(e.X, e.Y)))
      {
        ArrayPosForTowerStanding = new Point((e.X - DeltaX) / Convert.ToInt32(15 * Scaling), (e.Y - DeltaY) / Convert.ToInt32(15 * Scaling));
        if (!Check(ArrayPosForTowerStanding, true))
          ArrayPosForTowerStanding = new Point(-1, -1);
      }
      else
        ArrayPosForTowerStanding = new Point(-1, -1);
      #endregion
    }
    #endregion

    #region Game Logic

    #region Построитель областей
    /*
     * А зачем вообще это? Если захочется изменить положение кнопок, чтобы переписать в одном месте и для вывода на экран и для проверки 
     попадает ли курсор на кнопку при нажатии
     * Проверки на попали ли вообще в область магазина башен(к примеру) делается в одном месте и выносить оттуда проверку смысла не имеет
     */

    private Rectangle BuildRect(RectBuilder RectType)
    {
      switch (RectType)
      {
        case RectBuilder.Destroy:
          return new Rectangle(Convert.ToInt32((700 - BDestroyTower.Width) * Scaling), Convert.ToInt32(350 * Scaling),
          Convert.ToInt32(BDestroyTower.Width * Scaling), Convert.ToInt32(BDestroyTower.Height * Scaling));
        case RectBuilder.Upgrade:
          return new Rectangle(Convert.ToInt32((700 - BUpgradeTower.Width) * Scaling), Convert.ToInt32((340 - BDestroyTower.Height) * Scaling),
          Convert.ToInt32(BUpgradeTower.Width * Scaling), Convert.ToInt32(BUpgradeTower.Height * Scaling));
        case RectBuilder.NewLevelEnabled:
          return new Rectangle(Convert.ToInt32((DeltaX + (450 / 2) - (BStartLevelDisabled.Width / 2)) * Scaling),
          Convert.ToInt32((DeltaY * 2 + 450) * Scaling), Convert.ToInt32(BStartLevelDisabled.Width * Scaling), Convert.ToInt32(BStartLevelDisabled.Height * Scaling));
        case RectBuilder.NewLevelDisabled:
          return new Rectangle(Convert.ToInt32((DeltaX + (450 / 2) - (BStartLevelEnabled.Width / 2)) * Scaling),
          Convert.ToInt32((DeltaY * 2 + 450) * Scaling), Convert.ToInt32(BStartLevelEnabled.Width * Scaling), Convert.ToInt32(BStartLevelEnabled.Height * Scaling));
      }
      return new Rectangle();
    }
    #endregion

    #region "Финализаторы" действий
    //Если была выделена вышка и необходимо снять выделение
    private void FinishTowerMapSelectAct()
    {
      TowerMapSelectedID = -1;
    }

    //Если поставили вышку или отменили её поставку
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

    //Действие с Page Selector'ом магазина(Вывод или выбор)
    private bool ShopPageSelectorAction(ProcAction Act, Graphics Canva, int XMouse = 0, int YMouse = 0)
    {
      int DY = 0;//Если больше одного ряда страниц будет изменена в процессе цикла
      Func<int, Rectangle> LambdaBuildRect = (x) => new Rectangle(Convert.ToInt32((460 + (x % 3) * ("Page " + (x + 1).ToString()).Length * 12 + DeltaX * 2) * Scaling),
             Convert.ToInt32((MoneyPict.Height + 35 * (DY + 1)) * Scaling), Convert.ToInt32(("Page " + (x + 1).ToString()).Length * 11 * Scaling), Convert.ToInt32(24 * Scaling));
      for (int i = 0; i < PageCount; i++)
      {
        if ((i != 0) && (i % 3 == 0))
          DY++;
        switch (Act)
        {
          case ProcAction.Show:
            //Строка
            Canva.DrawString("Page " + (i + 1).ToString(), new Font("Arial", 14 * Scaling), new SolidBrush(Color.Black),
              new Point(Convert.ToInt32((460 + (i % 3) * ("Page " + (i + 1).ToString()).Length * 12 + DeltaX * 2) * Scaling),
                Convert.ToInt32((MoneyPict.Height + 35 * (DY + 1)) * Scaling)));//Эта часть с new Point сильно раздражает
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

    //Действие со страницей магазина(Вывод или выбор)
    private bool ShopPageAction(ProcAction Act, Graphics Canva, int XMouse = 0, int YMouse = 0)
    {
      int TowersAtCurrentPage = GetNumberOfTowersAtPage(CurrentShopPage);
      int offset = 0;
      Func<int, int, Rectangle> LambdaBuildRect = (x, y) => new Rectangle(Convert.ToInt32((460 + x * 42 + DeltaX * 2) * Scaling),
                    Convert.ToInt32((60 + MoneyPict.Height + y * 42 + 40) * Scaling), Convert.ToInt32(32 * Scaling), Convert.ToInt32(32 * Scaling));
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

    #region Проверки
    //Проверка при попытке постановки башни, входит ли в границы массива
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
    #endregion

    #region Все действия с монстрами
    //Добавление врага
    private void AddMonster()
    {
      Monsters.Add(new TMonster(CurrentLevelConf, Map.Way, MonstersCreated, Scaling));
      MonstersCreated++;
    }
    #endregion

    //Число вышек на выбраной странице магазина
    private int GetNumberOfTowersAtPage(int PageNumber = 1)
    {
      return (PageCount != PageNumber)
       ? (LinesInOnePage * MaxTowersInLine) : TowerParamsForBuilding.Count - (PageNumber - 1) * (LinesInOnePage * MaxTowersInLine);
    }

    //Освобождение таймера
    public void GetFreedomToTimer()
    {
      GameTimer.Tick -= Timer_Tick;
      this.GameTimer.Stop();
    }

    //Проигрыш
    private void Looser()
    {
      GetFreedomToTimer();
      Lose = true;
      Show(true);
    }

    //Игровой таймер
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
