﻿using System;
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
    private List<int> NumberOfMonstersAtLevel;
    private List<int> GoldForSuccessfulLevelFinish;
    private List<sTowerParam> TowerParamsForBuilding;
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
    private Color BackgroundColor;
    private long Position;//Позиция в файле конфигурации монстров
    private int CurrentLevelNumber;//Номер текущего уровня
    private int LevelsNumber;//Число уровней
    private int Gold;//Золото игрока
    private int NumberOfLives;//Число монстров которых можно пропустить
    private float GameScale = 1.0F;//Масштаб, используемый в игре
    private bool LevelStarted;
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
        if ((value * 15 - Math.Floor(value * 15) == 0))//Если программист не догадывается что изображение не может содержать
        //не целый пиксель мы защитимся от такого тормоза
        {
          GameScale = value;
        }
      }
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
      object[] GameSettings;
      SaveNLoad.LoadMainGameConf(Loader, out NumberOfMonstersAtLevel, out GoldForSuccessfulLevelFinish, out GameSettings);
      //Загрузили карту
      Map = new TMap(Environment.CurrentDirectory + "\\Data\\Maps\\" + Convert.ToString(GameSettings[0]).Substring(Convert.ToString(GameSettings[0]).LastIndexOf('\\')));
      ConstantMapImage = null;
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
      TGame Result;
      try
      {
        Result = new TGame(PBForDraw, GameTimer, ConfigurationName);
      }
      catch (Exception exc)
      {
        System.Windows.Forms.MessageBox.Show("Game files damadged: " + exc.Message, "Fatal error");
        return null;
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
    private void MapAreaShowing(Graphics Canva)
    {
      if (ConstantMapImage == null)
        ConstantMapImage=Map.GetConstantBitmap((int)(450 * Scaling), (int)(450 * Scaling));
      Bitmap WorkingBitmap = new Bitmap(ConstantMapImage);
      Graphics WorkingCanva = Graphics.FromImage(WorkingBitmap);
      Random rnd=new Random();
      WorkingCanva.DrawString("FUCK SOPA", new Font("Arial", 14), new SolidBrush(Color.Black), new Point(rnd.Next(290), rnd.Next(300)));
      Canva.DrawImage(WorkingBitmap, 30, 30);
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
    public void MousePressed(System.Windows.Forms.MouseEventArgs e)
    {
      //если уровень ещё не начат и игрок захотел начать
      //if (!LevelStarted)
      //{
      if (((e.X >= (30 + ((450 * Scaling) / 2) - (BStartLevelEnabled.Width / 2))) && (e.X <= (30 + ((450 * Scaling) / 2) + (BStartLevelEnabled.Width / 2))))
          && ((e.Y >= 80 + (450 * Scaling)) && (e.Y <= 80 + (450 * Scaling) + BStartLevelEnabled.Height)))
      {
        LevelStarted = !LevelStarted;//true;
        BStartLevelShow(GraphicalBuffer.Graphics, true);
        GraphicalBuffer.Render();
      }
      //}
    }

    //Пользователь не отличается умом и сообразительностью и свернул окно(там пауза ставиться будет, это позже)
    //Постановка паузы

    //Пользователь вдуплил что неполохо бы вернуться в игру и развернул окно
    public void Render()//Не рекомендуется использовать этот метод вообще
    {
      GraphicalBuffer.Render();
    }
    #endregion

    #region Game Logic
    private void Timer_Tick(object sender, EventArgs e)
    {
      MapAreaShowing(GraphicalBuffer.Graphics);
      GraphicalBuffer.Render();
    }
    #endregion
  }
}