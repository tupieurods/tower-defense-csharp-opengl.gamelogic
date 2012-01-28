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

    System.Windows.Forms.PictureBox GameDrawingSpace;

    TMap Map;
    MonsterParam CurrentLevelConf;
    int CurrentLevelNumber;
    int LevelsNumber;
    int Gold;
    int NumberOfLives;

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
      //Далее идёт вывод карты, приделано на время тестирования, возможно останется здесь навсегда
      Bitmap TmpBitmap = new Bitmap(450, 450);
      Graphics Canva = Graphics.FromImage(TmpBitmap);
      MapShow(Canva);
      GameDrawingSpace.Image = TmpBitmap;
      //Вывод карты окончен
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
    }
    #endregion

    public static TGame Factory(System.Windows.Forms.PictureBox PBForDraw, string ConfigurationName)
    {
      TGame Result;
      try
      {
        Result = new TGame(PBForDraw, ConfigurationName);
      }
      catch (Exception exc)
      {
        System.Windows.Forms.MessageBox.Show("Game files damadged: " + exc.Message,"Fatal error");
        return null;
      }
      return Result;
    }

    #region Graphical Part
    //Сделано на период разработки, в будущем возможно умрёт
    private void MapShow(Graphics DrawingSpace)
    {
      Map.ShowOnGraphics(DrawingSpace);
    }
    #endregion
  }
}
