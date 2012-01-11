//#define Debug
#define Debug2
#define Debug3

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameCoClassLibrary
{
  public enum MapElemStatus { CanMove, CanBuild, BusyByUnit, BusyByTower };

  [Serializable]
  public struct MapElem
  {
    public int PictNumber;//Картинка для рисования
    public int AngleOfRotate;//и угол её поворота
    public MapElemStatus Status;
    public MapElem(int PictNumber, int AngleOfRotate, MapElemStatus Status)
    {
      this.PictNumber = PictNumber;
      this.AngleOfRotate = AngleOfRotate;
      this.Status = Status;
    }
  }

  public class TMap
  {
    #region Private
    private MapElem[,] MapArray;
    //описание состава массива MapArray
    //в каждом элементе масства MapArray хранятся координаты точки на которую он указывает
    //Хранится ID картинки которую мы должны изобразить на поле, а так же угол поворота этой картинки
    //Так же элемент массива хранит можно ли по нему перемещаться, находится ли в этой части поля юнит или башня
    //а также можно ли тут строить
    private List<Point> Way;
    private Point Start = new Point(-1, -1);
    private Point Finish = new Point(-1, -1);
    private static string[] MapStatusString;
    //Массив, в котором мы храним какой элемент карты каким статусом должен обладать
    //0-CanMove
    //1-CanBuild
    //2,3-BusyByUnit. For bigger Map
    #endregion

    #region Public
    //Изображения поля
    public static Bitmap[] Bitmaps
    {
      get;
      private set;
    }
    public int Width
    {
      get;
      private set;
    }
    public int Height
    {
      get;
      private set;
    }
    public int WayLength
    {
      get
      {
        return Way.Count;
      }
    }
    #endregion

    #region Constructors
    static TMap()
    {
      string path = Environment.CurrentDirectory;
      if (File.Exists(path + "\\Data\\Settings.cfg"))
      {
        try
        {
          StreamReader SettingsFileStrm = new StreamReader(new FileStream(path + "\\Data\\Settings.cfg", FileMode.Open));
          string s;
          MapStatusString = new string[3];
          #region Обработка файла конфигурации
          while ((s = SettingsFileStrm.ReadLine()) != null)
          {
            if (s.IndexOf("CountMapElemPict", 0) != -1)
            {
              Bitmaps = new Bitmap[Convert.ToInt32(s.Substring((s.IndexOf(' ') + 1)))];
              continue;
            }
            if (s.IndexOf("CanMove", 0) != -1)
            {
              MapStatusString[0] = s.Substring((s.IndexOf(' ') + 1));
              continue;
            }
            if (s.IndexOf("CanBuild", 0) != -1)
            {
              MapStatusString[1] = s.Substring((s.IndexOf(' ') + 1));
              continue;
            }
            if (s.IndexOf("BusyByUnit", 0) != -1)
            {
              MapStatusString[2] = s.Substring((s.IndexOf(' ') + 1));
              continue;
            }
          #endregion
          }
        }
        catch
        {
          System.Windows.Forms.MessageBox.Show("Configuration file loading error, standart settings will be used");
        }
      }
      else
        Bitmaps = new Bitmap[4];
      if ((MapStatusString[0] == "") || (MapStatusString[1] == "") || (MapStatusString[2] == ""))//Если не заполнены все необходимые поля
      {
        System.Windows.Forms.MessageBox.Show("Configuration file loading error. Can't continue");
        Environment.Exit(1);
      }
      //Загрузка картинок
      for (int i = 0; i < Bitmaps.Length; i++)
      {
        try
        {
          Bitmaps[i] = new Bitmap(path + "\\Data\\I" + i + ".bmp");
        }
        catch
        {
          System.Windows.Forms.MessageBox.Show("Bitmaps loading error, can't continue. Application closing");
          Environment.Exit(1);
        }
      }
    }

    public TMap(int Width, int Height)
    {
      this.Width = Width;
      this.Height = Height;
      MapArray = new MapElem[Height, Width];//Сначала строки, затем столбцы
      Start = new Point(-1, -1);
      Finish = new Point(-1, -1);
      Way = new List<Point>();
      for (int i = 0; i < Height; i++)
        for (int j = 0; j < Width; j++)
          MapArray[i, j] = new MapElem(1, 0, MapElemStatus.CanBuild);
    }

    public TMap(string PathToFile)
    {
      try
      {
        FileStream FileLoadStream = new FileStream(PathToFile, FileMode.Open, FileAccess.Read);
        IFormatter Formatter = new BinaryFormatter();
        //Сначала прочтём структуру типа Point, в которой X-ширина, Y-высота карты
        Point tmp = (Point)Formatter.Deserialize(FileLoadStream);//Получили размеры
        Width = tmp.X;
        Height = tmp.Y;
        //Начало и конец пути
        Start = (Point)Formatter.Deserialize(FileLoadStream);
        Finish = (Point)Formatter.Deserialize(FileLoadStream);
        //Сама карта
        MapArray = new MapElem[Height, Width];
        for (int i = 0; i < Height; i++)
          for (int j = 0; j < Width; j++)
          {
            MapArray[i, j] = (MapElem)(Formatter.Deserialize(FileLoadStream));
          }
        FileLoadStream.Close();
        RebuildWay();
      }
      catch
      {
        throw;
      }
    }
    #endregion

    public void ShowOnGraphics(Graphics Canva)
    {
      for (int i = 0; i < Height; i++)
        for (int j = 0; j < Width; j++)
        {
          if (MapArray[i, j].PictNumber == -1)//пустой элемент
            continue;
          try
          {
            Bitmap TmpBitmap = new Bitmap(Bitmaps[MapArray[i, j].PictNumber]);
            for (int k = 0; k < MapArray[i, j].AngleOfRotate; k++)
              TmpBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            Canva.DrawImage(TmpBitmap, j * 15, i * 15,15,15);
#if Debug
          Canva.DrawString(Convert.ToString(MapArray[i, j].PictNumber), new Font(new FontFamily("Arial"), 10), new SolidBrush(Color.Black),
            new Point(j * 15, i * 15));
#endif
#if Debug2
          if (Start.X != -1)
          {
            Canva.DrawString("Start", new Font(new FontFamily("Arial"), 8), new SolidBrush(Color.Black),
              new Point(Start.X * 15, Start.Y * 15));
          }
          if (Finish.X != -1)
          {
            Canva.DrawString("Finish", new Font(new FontFamily("Arial"), 8), new SolidBrush(Color.Black),
              new Point(Finish.X * 15, Finish.Y * 15));
          }
#endif
          }
          catch (Exception e)
          {
            System.Windows.Forms.MessageBox.Show(e.Message);
            Environment.Exit(1);
          }
        }
      //Вывод пути
#if Debug3
    if (Way.Count != 0)
    {
      foreach (Point tmp in Way)
      {
        Canva.DrawEllipse(new Pen(new SolidBrush(Color.Red), 1), new Rectangle(tmp.X * 15 + 7, tmp.Y * 15 + 7, 3, 3));
      }
    }
#endif
    }

    public bool SaveToFile(string PathToFile)
    {
      try
      {
        FileStream FileSaveStream = new FileStream(PathToFile, FileMode.Create, FileAccess.Write);
        IFormatter Formatter = new BinaryFormatter();
        //Сначала запишем структуру типа Point, в которой X-ширина, Y-высота карты
        Formatter.Serialize(FileSaveStream, new Point(Width, Height));
        //Сохраним точки старта и финиша
        Formatter.Serialize(FileSaveStream, Start);
        Formatter.Serialize(FileSaveStream, Finish);
        //Сохраним карту
        for (int i = 0; i < Height; i++)
          for (int j = 0; j < Width; j++)
          {
            Formatter.Serialize(FileSaveStream, MapArray[i, j]);
          }
        FileSaveStream.Close();
      }
      catch
      {
        return false;
      }
      return true;
    }

    public bool AddElemToMap(Point Coord, MapElem Elem)
    {
      try
      {
        MapArray[Coord.Y, Coord.X] = Elem;
        if (MapStatusString[0].IndexOf("\\" + Convert.ToString(Elem.PictNumber) + "\\", 0) != -1)
        {
          MapArray[Coord.Y, Coord.X].Status = MapElemStatus.CanMove;
          return true;
        };
        if (MapStatusString[1].IndexOf("\\" + Convert.ToString(Elem.PictNumber) + "\\", 0) != -1)
        {
          MapArray[Coord.Y, Coord.X].Status = MapElemStatus.CanBuild;
          return true;
        };
        if (MapStatusString[2].IndexOf("\\" + Convert.ToString(Elem.PictNumber) + "\\", 0) != -1)
        {
          MapArray[Coord.Y, Coord.X].Status = MapElemStatus.BusyByUnit;
          return true;
        };
      }
      catch
      {
        return false;
      }
      return false;
    }

    public bool SetStart(Point StartPos)
    {
      if (MapArray[StartPos.Y, StartPos.X].Status == MapElemStatus.CanMove)
      {
        Start = new Point(StartPos.X, StartPos.Y);
        return true;
      }
      else
        return false;
    }

    public bool SetFinish(Point FinishPos)
    {
      if (MapArray[FinishPos.Y, FinishPos.X].Status == MapElemStatus.CanMove)
      {
        Finish = new Point(FinishPos.X, FinishPos.Y);
        return true;
      }
      else
        return false;
    }

    public void RebuildWay()
    {
      if ((Start.X == -1) & (Finish.X == -1))
        return;
      Way = new List<Point>();
      GetWay(Start, Finish);
      //System.Windows.Forms.MessageBox.Show("success");
    }

    private void GetWay(Point Pos, Point EndPos)
    {
      if (!(((Pos.Y >= 0) && (Pos.Y <= Height)) && ((Pos.X >= 0) && (Pos.X <= Width))))
        return;
      if (MapArray[Pos.Y, Pos.X].Status != MapElemStatus.CanMove)
        return;
      /*if (Way.Contains(Pos)){
        System.Windows.Forms.MessageBox.Show("WayBuildFailed");
        return;
      }*/
      MapArray[Pos.Y, Pos.X].Status = MapElemStatus.BusyByUnit;
      Way.Add(Pos);
      //Последующие проверки следят за тем чтобы если мы в предыдущем запуске
      //рекурсии нашли путь то дальше не запускаться
      if (!((Pos.X == EndPos.X) && (Pos.Y == EndPos.Y)))
      {
        Point Tmp = new Point(Pos.X + 1, Pos.Y);
        GetWay(Tmp, EndPos);//столбец+1
        if (!((Way[Way.Count - 1].X == EndPos.X) && (Way[Way.Count - 1].Y == EndPos.Y)))
        {
          Tmp = new Point(Pos.X, Pos.Y + 1);
          GetWay(Tmp, EndPos);//строка+1
        }
        if (!((Way[Way.Count - 1].X == EndPos.X) && (Way[Way.Count - 1].Y == EndPos.Y)))
        {
          Tmp = new Point(Pos.X, Pos.Y - 1);
          GetWay(Tmp, EndPos);//строка-1
        }
        if (!((Way[Way.Count - 1].X == EndPos.X) && (Way[Way.Count - 1].Y == EndPos.Y)))
        {
          Tmp = new Point(Pos.X - 1, Pos.Y);
          GetWay(Tmp, EndPos);//столбец-1
        }
      }
      MapArray[Pos.Y, Pos.X].Status = MapElemStatus.CanMove;
    }

    public Point GetWayElement(int WayPos)
    {
      if ((WayPos >= 0) & (WayPos < Way.Count))
        return Way[WayPos];
      else
        return new Point(-1, -1);
    }
  }
}