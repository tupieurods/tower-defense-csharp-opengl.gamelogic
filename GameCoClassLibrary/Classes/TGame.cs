﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

/*
 * На данном этапе разработки масшатабирование картинок не производится, если игра будет переноситься
 * в будущем и понадобится масштабирование, придётся сделать картинки масштабируемыми
 */

namespace GameCoClassLibrary
{
  public class TGame
  {
    #region Private Vars

    #region Lists
    private List<int> NumberOfMonstersAtLevel;//Число монстров на каждом из уровней
    private List<int> GoldForSuccessfulLevelFinish;//Число золота за успешное завершение уровня
    private List<int> GoldForKillMonster;//Золото за убийство монстра на уровне
    private List<sTowerParam> TowerParamsForBuilding;//Параметры башен
    private List<TMonster> Monsters;//Список с монстрами на текущем уровне(!НЕ КОНФИГУРАЦИИ ВСЕХ УРОВНЕЙ)
    #endregion

    #region Static
    //Если кто-то читает эти исходники кроме меня и не опнимает названия переменных, закройте этот файл
    //B в начале названия переменной- означает Button
    static private Bitmap MoneyPict, BStartLevelEnabled, BStartLevelDisabled, BDestroyTower, BUpgradeTower;
    static private Bitmap RedArrow;
    #endregion

    private System.Windows.Forms.PictureBox GameDrawingSpace;//Picture Box для отрисовки
    private BufferedGraphics GraphicalBuffer;
    private Bitmap ConstantMapImage;

    private System.Windows.Forms.Timer GameTimer;

    private TMap Map;//Карта
    private MonsterParam CurrentLevelConf;//Текущая конфигурация монстров
    private Color BackgroundColor;//Цвет заднего фона
    private long Position;//Позиция в файле конфигурации монстров
    private int CurrentLevelNumber;//Номер текущего уровня
    private int LevelsNumber;//Число уровней
    private int Gold;//Золото игрока
    private int NumberOfLives;//Число монстров которых можно пропустить
    private float GameScale = 1.0F;//Масштаб, используемый в игре
    private bool LevelStarted;//Начат уровень или нет
    private string PathToLevelConfigurations;//Путь к файлу конфигурации уровней

    private int MonstersCreated;//Число созданых монстров
    private int DeltaX = 10;//Отступы от левого верхнего края для карты
    private int DeltaY = 10;
    #endregion

    #region Public
    public float Scaling//На время тестирования без всяких проверок, когда это реально понадобится, будет переделано
    {
      get
      {
        return GameScale;
      }
      set
      {
        if (Convert.ToInt32(value * 15 - Math.Floor(value * 15)) == 0)//Если программист не догадывается что изображение не может содержать
        //не целый пиксель мы защитимся от такого тормоза
        {
          GameScale = value;
          ConstantMapImage = null;
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
      Lose = false;
      //Получили основную конфигурацию
      BinaryReader Loader = new BinaryReader(new FileStream(Environment.CurrentDirectory + "\\Data\\GameConfigs\\" + ConfigurationName + ".tdgc",
                                                              FileMode.Open, FileAccess.Read));
      PathToLevelConfigurations = Environment.CurrentDirectory + "\\Data\\GameConfigs\\" + ConfigurationName + ".tdlc";
      object[] GameSettings;
      SaveNLoad.LoadMainGameConf(Loader, out NumberOfMonstersAtLevel, out GoldForSuccessfulLevelFinish, out GoldForKillMonster, out GameSettings);
      Loader.Close();
      //Создание оставшихся списков
      Monsters = new List<TMonster>();
      //Позиция в файле уровней
      Position = 0;
      //Загрузили карту
      Map = new TMap(Environment.CurrentDirectory + "\\Data\\Maps\\" + Convert.ToString(GameSettings[0]).Substring(Convert.ToString(GameSettings[0]).LastIndexOf('\\')), true);
      //В будущем изменить масштабирование, чтобы не было лишней площади
      GameDrawingSpace = PBForDraw;
      Scaling = 1F;
      Map.Scaling = Scaling;
      GameDrawingSpace.Width = Convert.ToInt32(GameDrawingSpace.Width * Scaling);
      GameDrawingSpace.Height = Convert.ToInt32(GameDrawingSpace.Height * Scaling);
      //Создание буфера кадров
      BufferedGraphicsContext CurrentContext;
      CurrentContext = BufferedGraphicsManager.Current;
      GraphicalBuffer = CurrentContext.Allocate(GameDrawingSpace.CreateGraphics(), new Rectangle(new Point(0, 0), GameDrawingSpace.Size));
      #region Загрузка параметров башен
      DirectoryInfo DIForLoad = new DirectoryInfo(Environment.CurrentDirectory + "\\Data\\Towers\\" + Convert.ToString(GameSettings[1]));
      FileInfo[] TowerConfigs = DIForLoad.GetFiles();
      TowerParamsForBuilding = new List<sTowerParam>();
      foreach (FileInfo i in TowerConfigs)
      {
        if (i.Extension == ".tdtc")
        {
          using (FileStream TowerConfLoadStream = new FileStream(i.FullName, FileMode.Open, FileAccess.Read))
          {
            IFormatter Formatter = new BinaryFormatter();
            TowerParamsForBuilding.Add((sTowerParam)Formatter.Deserialize(TowerConfLoadStream));
          }
        }
      }
      #endregion
      //Число уровней, жизни
      LevelsNumber = (int)GameSettings[2];
      CurrentLevelNumber = 0;
      Gold = (int)GameSettings[4];
      NumberOfLives = (int)GameSettings[5];
      //Прилинковывание события таймера
      this.GameTimer = GameTimer;
      this.GameTimer.Tick += new System.EventHandler(Timer_Tick);
      //Цвет фона, уровень не начат, показ всего окна
      BackgroundColor = Color.Silver;
      LevelStarted = false;
      Show(false, true);
      //Настройка и запуск таймера
      this.GameTimer.Interval = 30;
      this.GameTimer.Start();
    }

    static TGame()
    {
      MoneyPict = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\Money.png");
      MoneyPict.MakeTransparent();
      RedArrow = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\RedArrow.png");
      RedArrow.MakeTransparent();
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

    #region Graphical Part
    //Эта процедура используется лишь тогда, когда нужно вывести весь игровой экран
    //В процессе игры будут использоваться другие процедуры
    //Данная процедура введена на начальном этапе разработки, используется при создании игры
    private void Show(bool LinkToImage = false, bool WithGUI = false)
    {
      Graphics Canva;
      Bitmap DrawingBitmap = null;
      if (LinkToImage)
      {
        DrawingBitmap = WithGUI ? new Bitmap(Convert.ToInt32(700 * Scaling), Convert.ToInt32(600 * Scaling))
        : new Bitmap(Convert.ToInt32((450 + DeltaX * 2) * Scaling), Convert.ToInt32(600 * Scaling));
        Canva = Graphics.FromImage(DrawingBitmap);
      }
      else
      {
        Canva = GraphicalBuffer.Graphics;
      }
      //Залили одним цветом область карты
      Canva.FillRectangle(new SolidBrush(BackgroundColor), 0, 0, Convert.ToInt32((450 + DeltaX * 2) * Scaling), Convert.ToSingle(600 * Scaling));
      //Вывели карту
      MapAreaShowing(Canva);
      //StartLevelButton
      BStartLevelShow(Canva);
      if (WithGUI)
      {
        Canva.FillRectangle(new SolidBrush(BackgroundColor), Convert.ToInt32((450 + DeltaX * 2) * Scaling), 0,
          Convert.ToInt32((450 + DeltaX * 2) * Scaling), Convert.ToSingle(600 * Scaling));
        //Вывели линию разделения
        Canva.DrawLine(new Pen(new SolidBrush(Color.White), 3), new Point(Convert.ToInt32((450 + DeltaX * 2) * Scaling), 0),
              new Point(Convert.ToInt32((450 + DeltaX * 2) * Scaling), Convert.ToInt32(700 * Scaling)));
        //Картинки монеток, up и прочие масштабироваться не будут
        ShowMoney(Canva, true);
        ShowLives(Canva);
        ShowTowerShopPage(Canva, true, 1);
        Canva.DrawImage(BUpgradeTower, 500, 300, BUpgradeTower.Width, BUpgradeTower.Height);
        Canva.DrawImage(BDestroyTower, 500, 365, BDestroyTower.Width, BDestroyTower.Height);
        //Tower Description
        Canva.DrawString("Tower Description will\n be here", new Font("Arial", 14), new SolidBrush(Color.Black), new Point(Convert.ToInt32(500 * Scaling), Convert.ToInt32(450 * Scaling)));
      }
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

    //Вывод количества денег 
    private void ShowMoney(Graphics Canva, bool ShowMoneyPict = false)
    {
      //Изображение монеты
      if (ShowMoneyPict)
      {
        Canva.DrawImage(MoneyPict, Convert.ToInt32((460 + DeltaX * 2) * Scaling), Convert.ToInt32(5 * Scaling), MoneyPict.Width, MoneyPict.Height);
      }
      //Вывод числа денег
      Canva.FillRectangle(new SolidBrush(BackgroundColor), Convert.ToSingle((460 + DeltaX * 2) * Scaling + MoneyPict.Width),
        Convert.ToSingle(5 * Scaling), Convert.ToSingle(700 * Scaling), Convert.ToSingle(MoneyPict.Width));
      Canva.DrawString(Gold.ToString(), new Font("Arial", 14), new SolidBrush(Color.Black),
        new Point(Convert.ToInt32((460 + DeltaX * 2) * Scaling) + MoneyPict.Width, Convert.ToInt32(9 * Scaling)));
    }

    //Показ страницы магазина
    private void ShowTowerShopPage(Graphics Canva, bool Clear = true, int PageNumber = 1)
    {
      int LinesInOnePage = 3;
      int MaxTowersInLine = 5;
      if (Clear)
      {
        Canva.FillRectangle(new SolidBrush(BackgroundColor), (460 + DeltaX * 2) * Scaling - 2, 50 * Scaling + MoneyPict.Height +/*30*/28,
          (MaxTowersInLine * 42) * Scaling - 6, 42 * LinesInOnePage - 6);//Три линии всего видно
        //-2 в первом и втором числовом параметре для компенсации обводки иконки башни при выделении\
        //-6в 3м и 4м числовых параметрах чтобы не закрасить лишнего(+10 делается для разраничения иконок)+обводка иконки
      }
      for (int j = 0; j <= TowerParamsForBuilding.Count / MaxTowersInLine; j++)
      {
        int NumberOfTowersInLine = (TowerParamsForBuilding.Count - (TowerParamsForBuilding.Count % MaxTowersInLine)) == (j * MaxTowersInLine) ?
          (TowerParamsForBuilding.Count % MaxTowersInLine) : MaxTowersInLine;
        for (int i = 0; i < NumberOfTowersInLine; i++)
        {
          Canva.DrawImage(TowerParamsForBuilding[i + j * MaxTowersInLine].Icon, Convert.ToInt32((460 + DeltaX * 2) * Scaling) + i * 42,
            Convert.ToInt32(50 * Scaling) + MoneyPict.Height + j * 42 + 30, 32, 32);
          /*В текущей реализации все иконки башенобрезаются до 32x32, ниже если выводить реальный размер*/
          //TowerParamsForBuilding[i + j * MaxTowersInLine].Icon.Width, TowerParamsForBuilding[i + j * MaxTowersInLine].Icon.Height
        }
      }
    }

    //Перерисовка лишь области карты, основная процедура в игровое время
    private void MapAreaShowing(Graphics Canva)
    {
      if (ConstantMapImage == null)
        ConstantMapImage = Map.GetConstantBitmap((int)(450 * Scaling), (int)(450 * Scaling));
      Canva.Clip = new Region(new Rectangle(DeltaX, DeltaY, Convert.ToInt32(450 * Scaling), Convert.ToInt32(450 * Scaling)));
      Canva.DrawImage(ConstantMapImage, DeltaX, DeltaY, ConstantMapImage.Width, ConstantMapImage.Height);
      #region Вывод изображений башен
      #endregion
      #region Вывод изображений монстров
      foreach (TMonster Monster in Monsters)
      {
        if (Monster.InVisibleMapArea(new Point(Map.VisibleXStart, Map.VisibleYStart), new Point(Map.VisibleXFinish, Map.VisibleYFinish)))
          Monster.ShowMonster(Canva, new Point(Map.VisibleXStart, Map.VisibleYStart), new Point(Map.VisibleXFinish, Map.VisibleYFinish), DeltaX, DeltaY);
      }
      #endregion
      #region Вывод таких вещей как попытка постановки башни или выделение поставленой
      #endregion
      #region Вывод снарядов
      #endregion
      Canva.Clip = new Region();
    }

    //Показ кнопки начать новый уровень
    private void BStartLevelShow(Graphics Canva, bool Fill = false)
    {
      if (Fill)
      {
        int Width = LevelStarted ? BStartLevelDisabled.Width : BStartLevelEnabled.Width;
        int Height = LevelStarted ? BStartLevelDisabled.Height : BStartLevelEnabled.Height;
        Canva.FillRectangle(new SolidBrush(BackgroundColor), DeltaX + ((450 * Scaling) / 2) - (BStartLevelEnabled.Width / 2), 80 + (450 * Scaling), Width, Height);
      }
      if (LevelStarted)
      {
        Canva.DrawImage(BStartLevelDisabled, DeltaX + ((450 * Scaling) / 2) - (BStartLevelDisabled.Width / 2),
          DeltaY + 50 + (450 * Scaling), BStartLevelDisabled.Width, BStartLevelDisabled.Height);
      }
      else
      {
        Canva.DrawImage(BStartLevelEnabled, DeltaX + ((450 * Scaling) / 2) - (BStartLevelDisabled.Width / 2),
          DeltaY + 50 + (450 * Scaling), BStartLevelEnabled.Width, BStartLevelEnabled.Height);
      }
    }

    //Вывод числа жизней
    private void ShowLives(Graphics Canva)
    {
      Canva.FillRectangle(new SolidBrush(BackgroundColor), Convert.ToSingle((460 + DeltaX * 2) * Scaling),
        Convert.ToSingle(5 * Scaling + MoneyPict.Height), Convert.ToSingle(700 * Scaling), Convert.ToSingle(MoneyPict.Height));
      Canva.DrawString("Lives: " + NumberOfLives.ToString(), new Font("Arial", 14), new SolidBrush(Color.Black),
        new Point(Convert.ToInt32((460 + DeltaX * 2) * Scaling), MoneyPict.Height + 10));
    }
    #endregion

    #region Обработка действий пользователя
    public void MouseUp(System.Windows.Forms.MouseEventArgs e)
    {
      #region Если уровень ещё не начат и игрок захотел начать
      if ((!LevelStarted) && (CurrentLevelNumber < LevelsNumber))
      {
        if (((e.X >= (DeltaX + ((450 * Scaling) / 2) - (BStartLevelEnabled.Width / 2))) && (e.X <= (DeltaX + ((450 * Scaling) / 2) + (BStartLevelEnabled.Width / 2))))
            && ((e.Y >= DeltaY + 50 + (450 * Scaling)) && (e.Y <= DeltaY + 50 + (450 * Scaling) + BStartLevelEnabled.Height)))
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
          BStartLevelShow(GraphicalBuffer.Graphics, true);
          GraphicalBuffer.Render();
        }
      }
      #endregion
      #region Tower Selected in Shop

      if (e.X >= (Convert.ToInt32((460 + DeltaX * 2) * Scaling))
        && (e.Y >= Convert.ToInt32(50 * Scaling) + MoneyPict.Height + 30)
        && (e.Y <= Convert.ToInt32(50 * Scaling) + MoneyPict.Height + 30 + 42 * ((TowerParamsForBuilding.Count / 5) + 1)))
      {
        //System.Windows.Forms.MessageBox.Show("Debug");
        ShowTowerShopPage(GraphicalBuffer.Graphics, true, 1);
        bool Flag = false;
        for (int j = 0; j <= TowerParamsForBuilding.Count / 5; j++)
        {
          int NumberOfTowersInLine = (TowerParamsForBuilding.Count - (TowerParamsForBuilding.Count % 5)) == (j * 5) ? (TowerParamsForBuilding.Count % 5) : 5;
          for (int i = 0; i < NumberOfTowersInLine; i++)
          {
            if (new Rectangle(Convert.ToInt32((460 + DeltaX * 2) * Scaling) + i * 42, Convert.ToInt32(50 * Scaling) + MoneyPict.Height + 30 + j * 42, 32, 32).
              Contains(new Point(e.X, e.Y)))
            {
              GraphicalBuffer.Graphics.DrawRectangle(new Pen(Color.Red, 3), new Rectangle(Convert.ToInt32((460 + DeltaX * 2) * Scaling) + i * 42,
                Convert.ToInt32(50 * Scaling) + MoneyPict.Height + 30 + j * 42, 32, 32));
              Flag = true;
              break;
            }
          }
          if (Flag)
            break;
        }
        GraphicalBuffer.Render();
      }
      #endregion
    }

    public void MapAreaChanging(Point Position)
    {
      #region Перемещение границ карты
      if ((Map.Width <= 30) || (Map.Height <= 30))
        return;
      if (((Position.X > DeltaX) && (Position.X < (Convert.ToInt32(450 * Scaling) + DeltaX))) && ((Position.Y > DeltaY) && (Position.Y < (Convert.ToInt32(450 * Scaling) + DeltaY))))
      {
        if ((Position.X - DeltaX < 15))
        {
          Map.ChangeVisibleArea(-1, 0);
          ConstantMapImage = null;
        }
        if ((Position.Y - DeltaY < 15))
        {
          Map.ChangeVisibleArea(0, -1);
          ConstantMapImage = null;
        }
        if ((-Position.X + Convert.ToInt32(450 * Scaling) + DeltaX) < 15)
        {
          Map.ChangeVisibleArea(1, 0);
          ConstantMapImage = null;
        }
        if ((-Position.Y + Convert.ToInt32(450 * Scaling) + DeltaY) < 15)
        {
          Map.ChangeVisibleArea(0, 1);
          ConstantMapImage = null;
        }
      }
      #endregion
    }

    public void MouseMove(System.Windows.Forms.MouseEventArgs e)
    {
      #region Обработка перемещения при попытке постановки башни
      #endregion
    }
    #endregion

    #region Game Logic

    //Добавление врага
    private void AddMonster()
    {
      Monsters.Add(new TMonster(CurrentLevelConf, Map.Way, Scaling));
      MonstersCreated++;
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
      Show(true, true);
    }

    //Игровой таймер
    private void Timer_Tick(object sender, EventArgs e)
    {
      if (LevelStarted)
      {
        #region Действия башен(Выстрелы, подсветка невидимых юнитов)
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
            ShowLives(GraphicalBuffer.Graphics);
            //Вывод жизней
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
        MapAreaChanging(GameDrawingSpace.PointToClient(System.Windows.Forms.Control.MousePosition));
      MapAreaShowing(GraphicalBuffer.Graphics);
      GraphicalBuffer.Render();
    }
    #endregion
  }
}
