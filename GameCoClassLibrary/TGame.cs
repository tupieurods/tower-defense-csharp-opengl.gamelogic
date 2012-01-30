using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
    static private Bitmap MoneyPict, StartLevelEnabledPict, StartLevelDisabledPict, BDestroyTowerPict, BUpgradeTowerPict;
    #endregion

    private System.Windows.Forms.PictureBox GameDrawingSpace;//Picture Box для отрисовки

    private TMap Map;//Карта
    private MonsterParam CurrentLevelConf;//Текущая конфигурация монстров
    private Color BackgroundColor;
    private long Position;//Позиция в файле конфигурации монстров
    private int CurrentLevelNumber;//Номер текущего уровня
    private int LevelsNumber;//Число уровней
    private int Gold;//Золото игрока
    private int NumberOfLives;//Число монстров которых можно пропустить
    private float GameScale=1.0F;//Масштаб, используемый в игре
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
    private TGame(System.Windows.Forms.PictureBox PBForDraw, string ConfigurationName)
    {
      BinaryReader Loader = new BinaryReader(new FileStream(Environment.CurrentDirectory + "\\Data\\GameConfigs\\" + ConfigurationName + ".tdgc",
                                                              FileMode.Open, FileAccess.Read));
      object[] GameSettings;
      SaveNLoad.LoadMainGameConf(Loader, out NumberOfMonstersAtLevel, out GoldForSuccessfulLevelFinish, out GameSettings);
      Map = new TMap(Environment.CurrentDirectory + "\\Data\\Maps\\" + Convert.ToString(GameSettings[0]).Substring(Convert.ToString(GameSettings[0]).LastIndexOf('\\')));
      GameDrawingSpace = PBForDraw;
      Scaling = 1F;
      Map.Scaling = Scaling;
      GameDrawingSpace.Width = Convert.ToInt32(GameDrawingSpace.Width * Scaling);
      GameDrawingSpace.Height = Convert.ToInt32(GameDrawingSpace.Height * Scaling);
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
      LevelsNumber = (int)GameSettings[2];
      Gold = (int)GameSettings[4];
      NumberOfLives = (int)GameSettings[5];
      BackgroundColor = Color.Silver;
      Show(true, true);
    }

    static TGame()
    {
      MoneyPict = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\money.png");
      MoneyPict.MakeTransparent();
      StartLevelEnabledPict = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\StartLevelEnabled.jpg");
      StartLevelDisabledPict = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\StartLevelDisabled.jpg");
      BDestroyTowerPict = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\Destr.jpg");
      BUpgradeTowerPict = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\Up.jpg");
    }
    #endregion

    //Используется фабрика, если произойдёт ошибка мы просто вернём null, а не получим франкинштейна
    public static TGame Factory(System.Windows.Forms.PictureBox PBForDraw, string ConfigurationName)
    {
      TGame Result;
      try
      {
        Result = new TGame(PBForDraw, ConfigurationName);
      }
      catch (Exception exc)
      {
        System.Windows.Forms.MessageBox.Show("Game files damadged: " + exc.Message, "Fatal error");
        return null;
      }
      return Result;
    }

    #region Graphical Part
    private void Show(bool LinkToImage = false, bool WithGUI = false)
    {
      Bitmap DrawingBitmap = new Bitmap(Convert.ToInt32(700 * Scaling), Convert.ToInt32(600 * Scaling));
      Graphics Canva = Graphics.FromImage(DrawingBitmap);
      //Залили одним цветом область карты
      Canva.FillRectangle(new SolidBrush(BackgroundColor), 0, 0, Convert.ToSingle(490 * Scaling), Convert.ToSingle(600 * Scaling));
      //Вывели карту
      Map.ShowOnGraphics(Canva, 30, 30);
      if (WithGUI)
      {
        Canva.FillRectangle(new SolidBrush(BackgroundColor), Convert.ToSingle(490 * Scaling), 0, Convert.ToSingle(490 * Scaling), Convert.ToSingle(600 * Scaling));
        //Вывели линию разделения
        Canva.DrawLine(new Pen(new SolidBrush(Color.White), 3), new Point(Convert.ToInt32(490 * Scaling), 0),
              new Point(Convert.ToInt32(490 * Scaling), Convert.ToInt32(700 * Scaling)));
        Canva.DrawImage(MoneyPict, Convert.ToInt32(500 * Scaling), Convert.ToInt32(5 * Scaling));//Картинки монеток, up и прочие масштабироваться не будут
      }
      Canva.FillRectangle(new SolidBrush(BackgroundColor), Convert.ToSingle(535 * Scaling), Convert.ToSingle(5 * Scaling), Convert.ToSingle(700 * Scaling), Convert.ToSingle(32 * Scaling));
      Canva.DrawString(Gold.ToString(), new Font("Arial", 14), new SolidBrush(Color.Black), new Point(Convert.ToInt32(500 * Scaling)+35, Convert.ToInt32(9 * Scaling)));
      if (LinkToImage)
        GameDrawingSpace.Image = DrawingBitmap;
      else
      {
        Graphics Tmp = GameDrawingSpace.CreateGraphics();
        Tmp.DrawImage(DrawingBitmap, 0, 0);
      }
    }
    #endregion
  }
}
