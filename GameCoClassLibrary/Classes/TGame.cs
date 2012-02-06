using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
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
      Map = new TMap(Environment.CurrentDirectory + "\\Data\\Maps\\" + Convert.ToString(GameSettings[0]).Substring(Convert.ToString(GameSettings[0]).LastIndexOf('\\')));
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
        : new Bitmap(Convert.ToInt32(490 * Scaling), Convert.ToInt32(600 * Scaling));
        Canva = Graphics.FromImage(DrawingBitmap);
      }
      else
      {
        Canva = GraphicalBuffer.Graphics;
      }
      //Залили одним цветом область карты
      Canva.FillRectangle(new SolidBrush(BackgroundColor), 0, 0, Convert.ToSingle(490 * Scaling), Convert.ToSingle(600 * Scaling));
      //Вывели карту
      MapAreaShowing(Canva);
      //StartLevelButton
      BStartLevelShow(Canva);
      if (WithGUI)
      {
        Canva.FillRectangle(new SolidBrush(BackgroundColor), Convert.ToSingle(490 * Scaling), 0, Convert.ToSingle(490 * Scaling), Convert.ToSingle(600 * Scaling));
        //Вывели линию разделения
        Canva.DrawLine(new Pen(new SolidBrush(Color.White), 3), new Point(Convert.ToInt32(490 * Scaling), 0),
              new Point(Convert.ToInt32(490 * Scaling), Convert.ToInt32(700 * Scaling)));
        //Картинки монеток, up и прочие масштабироваться не будут
        ShowMoney(Canva, true);
        for (int j = 0; j <= TowerParamsForBuilding.Count / 5; j++)
        {
          int NumberOfTowersInLine = (TowerParamsForBuilding.Count - (TowerParamsForBuilding.Count % 5)) == (j * 5) ? (TowerParamsForBuilding.Count % 5) : 5;
          for (int i = 0; i < NumberOfTowersInLine; i++)
          {
            Canva.DrawImage(TowerParamsForBuilding[i + j * 5].Icon, Convert.ToInt32(500 * Scaling) + i * 42, Convert.ToInt32(50 * Scaling) + MoneyPict.Height + j * 42,
              TowerParamsForBuilding[i + j * 5].Icon.Width, TowerParamsForBuilding[i + j * 5].Icon.Height);
          }
        }
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

    private void ShowMoney(Graphics Canva, bool ShowMoneyPict = false)
    {
      //Изображение монеты
      if (ShowMoneyPict)
      {
        Canva.DrawImage(MoneyPict, Convert.ToInt32(500 * Scaling), Convert.ToInt32(5 * Scaling), MoneyPict.Width, MoneyPict.Height);
      }
      //Вывод числа денег
      Canva.FillRectangle(new SolidBrush(BackgroundColor), Convert.ToSingle(535 * Scaling), Convert.ToSingle(5 * Scaling), Convert.ToSingle(700 * Scaling), Convert.ToSingle(32 * Scaling));
      Canva.DrawString(Gold.ToString(), new Font("Arial", 14), new SolidBrush(Color.Black), new Point(Convert.ToInt32(500 * Scaling) + 35, Convert.ToInt32(9 * Scaling)));
    }

    //Перерисовка лишь области карты, основная процедура в игровое время
    private void MapAreaShowing(Graphics Canva, int DX = 30, int DY = 30)
    {
      if (ConstantMapImage == null)
        ConstantMapImage = Map.GetConstantBitmap((int)(450 * Scaling), (int)(450 * Scaling));
      Canva.Clip = new Region(new Rectangle(DX, DY, Convert.ToInt32(450 * Scaling), Convert.ToInt32(450 * Scaling)));
      Canva.DrawImage(ConstantMapImage, DX, DY, ConstantMapImage.Width, ConstantMapImage.Height);
      #region Вывод изображений башен
      #endregion
      #region Вывод изображений монстров
      foreach (TMonster Monster in Monsters)
      {
        Monster.ShowMonster(Canva);
      }
      #endregion
      #region Вывод таких вещей как попытка постановки башни или выделение поставленой
      #endregion
      #region Вывод снарядов
      #endregion
      //Random rnd = new Random();
      //Canva.DrawString("FUCK SOPA", new Font("Arial", 14), new SolidBrush(Color.Black), new Point(DX + rnd.Next(290), DY + rnd.Next(300)));
      Canva.Clip = new Region();
    }

    //Показ кнопки начать новый уровень
    private void BStartLevelShow(Graphics Canva, bool Fill = false)
    {
      if (Fill)
      {
        int Width = LevelStarted ? BStartLevelDisabled.Width : BStartLevelEnabled.Width;
        int Height = LevelStarted ? BStartLevelDisabled.Height : BStartLevelEnabled.Height;
        Canva.FillRectangle(new SolidBrush(BackgroundColor), 30 + ((450 * Scaling) / 2) - (BStartLevelEnabled.Width / 2), 80 + (450 * Scaling), Width, Height);
        //myBuffer.Graphics.FillRectangle(new SolidBrush(BackgroundColor), 30 + ((450 * Scaling) / 2) - (BStartLevelEnabled.Width / 2), 80 + (450 * Scaling), Width, Height);
      }
      if (LevelStarted)
      {
        /*myBuffer.Graphics.DrawImage(BStartLevelDisabled, 30 + ((450 * Scaling) / 2) - (BStartLevelDisabled.Width / 2),
          80 + (450 * Scaling), BStartLevelDisabled.Width, BStartLevelDisabled.Height);*/
        Canva.DrawImage(BStartLevelDisabled, 30 + ((450 * Scaling) / 2) - (BStartLevelDisabled.Width / 2),
          80 + (450 * Scaling), BStartLevelDisabled.Width, BStartLevelDisabled.Height);
      }
      else
      {
        /*myBuffer.Graphics.DrawImage(BStartLevelEnabled, 30 + ((450 * Scaling) / 2) - (BStartLevelEnabled.Width / 2),
          80 + (450 * Scaling), BStartLevelEnabled.Width, BStartLevelEnabled.Height);*/
        Canva.DrawImage(BStartLevelEnabled, 30 + ((450 * Scaling) / 2) - (BStartLevelDisabled.Width / 2),
          80 + (450 * Scaling), BStartLevelEnabled.Width, BStartLevelEnabled.Height);
      }
    }
    #endregion

    #region Обработка действий пользователя
    public void MouseUp(System.Windows.Forms.MouseEventArgs e)
    {
      //если уровень ещё не начат и игрок захотел начать
      if ((!LevelStarted) && (CurrentLevelNumber < LevelsNumber))
      {
        if (((e.X >= (30 + ((450 * Scaling) / 2) - (BStartLevelEnabled.Width / 2))) && (e.X <= (30 + ((450 * Scaling) / 2) + (BStartLevelEnabled.Width / 2))))
            && ((e.Y >= 80 + (450 * Scaling)) && (e.Y <= 80 + (450 * Scaling) + BStartLevelEnabled.Height)))
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
    }

    #endregion

    #region Game Logic

    //Добавление врага
    private void AddMonster()
    {
      Monsters.Add(new TMonster(CurrentLevelConf, Map.Way, Scaling));
      MonstersCreated++;
    }

    private void Looser()
    {
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
            //Вывод жизней
            if (NumberOfLives == 0)
            {
              Looser();
              Lose = true;
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
      MapAreaShowing(GraphicalBuffer.Graphics);
      GraphicalBuffer.Render();
    }
    #endregion
  }
}
