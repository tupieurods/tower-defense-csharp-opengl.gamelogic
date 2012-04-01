//#define Debug
//#define Debug2
//#define Debug3

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Structures;

namespace GameCoClassLibrary.Classes
{

  public class Map
  {
    #region Private
    private readonly MapElem[,] _mapArray;
    /*описание состава массива MapArray
    в каждом элементе масства MapArray хранятся координаты точки на которую он указывает
    Хранится ID картинки которую мы должны изобразить на поле, а так же угол поворота этой картинки
    Так же элемент массива хранит можно ли по нему перемещаться, находится ли в этой части поля юнит или башня
    а также можно ли тут строить*/
    //private List<Point> Way;
    private Point _start = new Point(-1, -1);
    private Point _finish = new Point(-1, -1);
    private static readonly string[] MapStatusString;
    //Массив, в котором мы храним какой элемент карты каким статусом должен обладать
    //0-CanMove
    //1-CanBuild
    //2,3-BusyByUnit. For bigger Map
    private static float _mapScale = 1.0F;//Используется для масштабирования.
    //Масштабирование включено сюда для оптимизации, хотя это спорный момент
    private readonly Bitmap[] _scaledBitmaps;
    #endregion

    #region Public
    //Изображения поля
    public static Bitmap[] Bitmaps { get; private set; }
    public List<Point> Way { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int VisibleXStart { get; private set; }
    public int VisibleXFinish { get; private set; }
    public int VisibleYStart { get; private set; }
    public int VisibleYFinish { get; private set; }
    public float Scaling
    {
      get
      {
        return _mapScale;
      }
      set
      {
        _mapScale = value <= 0 ? 1 : value;
        RebuildBitmaps();
      }
    }
    #endregion

    #region Constructors
    static Map()
    {
      string path = Environment.CurrentDirectory;
      if (File.Exists(path + "\\Data\\Settings.cfg"))
      {
        try
        {
          StreamReader settingsFileStrm = new StreamReader(new FileStream(path + "\\Data\\Settings.cfg", FileMode.Open));
          string s;
          MapStatusString = new string[3];
          #region Обработка файла конфигурации
          while ((s = settingsFileStrm.ReadLine()) != null)
          {
            if (s.IndexOf("CountMapElemPict", 0, StringComparison.Ordinal) != -1)
            {
              Bitmaps = new Bitmap[Convert.ToInt32(s.Substring((s.IndexOf(' ') + 1)))];
              continue;
            }
            if (s.IndexOf("CanMove", 0, StringComparison.Ordinal) != -1)
            {
              MapStatusString[0] = s.Substring((s.IndexOf(' ') + 1));
              continue;
            }
            if (s.IndexOf("CanBuild", 0, StringComparison.Ordinal) != -1)
            {
              MapStatusString[1] = s.Substring((s.IndexOf(' ') + 1));
              continue;
            }
            if (s.IndexOf("BusyByUnit", 0, StringComparison.Ordinal) != -1)
            {
              MapStatusString[2] = s.Substring((s.IndexOf(' ') + 1));
              // ReSharper disable RedundantJumpStatement
              continue;
              // ReSharper restore RedundantJumpStatement
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
      {
        Bitmaps = new Bitmap[4];
      }
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
          Bitmaps[i] = new Bitmap(path + "\\Data\\Images\\I" + i + ".png");
        }
        catch
        {
          System.Windows.Forms.MessageBox.Show("Bitmaps loading error, can't continue. Application closing");
          Environment.Exit(1);
        }
      }
    }

    public Map(int width, int height)
    {
      Width = VisibleXFinish = width;
      Height = VisibleYFinish = height;
      VisibleXStart = 0;
      VisibleYStart = 0;
      _mapArray = new MapElem[height, width];//Сначала строки, затем столбцы
      _start = new Point(-1, -1);
      _finish = new Point(-1, -1);
      Way = new List<Point>();
      _scaledBitmaps = new Bitmap[Bitmaps.Length];
      RebuildBitmaps();
      for (int i = 0; i < height; i++)
        for (int j = 0; j < width; j++)
          _mapArray[i, j] = new MapElem(1, 0, MapElemStatus.CanBuild);
    }

    public Map(string pathToFile, bool clippedArea = false)
    {
      try
      {
        FileStream fileLoadStream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
        IFormatter formatter = new BinaryFormatter();
        //Сначала прочтём структуру типа Point, в которой X-ширина, Y-высота карты
        Point tmp = (Point)formatter.Deserialize(fileLoadStream);//Получили размеры
        Width = VisibleXFinish = tmp.X;
        Height = VisibleYFinish = tmp.Y;
        if (clippedArea)
        {
          VisibleXFinish = Width > 30 ? 30 : VisibleXFinish;
          VisibleYFinish = Width > 30 ? 30 : VisibleYFinish;
        }
        VisibleXStart = 0;
        VisibleYStart = 0;
        //Начало и конец пути
        _start = (Point)formatter.Deserialize(fileLoadStream);
        _finish = (Point)formatter.Deserialize(fileLoadStream);
        //Сама карта
        _mapArray = new MapElem[Height, Width];
        for (int i = 0; i < Height; i++)
          for (int j = 0; j < Width; j++)
          {
            _mapArray[i, j] = (MapElem)(formatter.Deserialize(fileLoadStream));
          }
        fileLoadStream.Close();
        RebuildWay();
        _scaledBitmaps = new Bitmap[Bitmaps.Length];
        RebuildBitmaps();
      }
      catch
      {
        throw new Exception("Map load Error");
      }
    }
    #endregion

    //Вывод на Graphics
    public void ShowOnGraphics(Graphics canva, int startCanvaX = 0, int startCanvaY = 0, int finishCanvaX = 6000, int finishCanvaY = 6000)
    {
      if (canva == null) throw new ArgumentNullException("canva");
      //Проверки на выход за границу прорисоки сделаны на будущее,
      //Если игра будет переноситься на Opengl или другой 2d/3d движок, и мы не будем иметь возможности
      //контролировать вывод с помощью BitMap и Graphics, чтобы уже имелись наработки для контроля вывода карты и UI
      int realY = 0;
      for (int i = VisibleYStart; i < VisibleYFinish; i++, realY++)
      {
        /*if ((i * ElemSize * MapScale) > FinishCanvaY)//Если вышли за границы прорисовки по Y
          break;*/
        int realX = 0;
        for (int j = VisibleXStart; j < VisibleXFinish; j++, realX++)
        {
          if (_mapArray[i, j].PictNumber == -1)//пустой элемент
            continue;
          /*if ((j * Settings.ElemSize * MapScale) > FinishCanvaX)//Если вышли за границы прорисовки по X
            break;*/
          try
          {
            Bitmap tmpBitmap = new Bitmap(_scaledBitmaps[_mapArray[i, j].PictNumber]);
            for (int k = 0; k < _mapArray[i, j].AngleOfRotate; k++)
              tmpBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            canva.DrawImage(tmpBitmap, Convert.ToInt32(startCanvaX + realX * Settings.ElemSize * _mapScale),
              Convert.ToInt32(startCanvaY + realY * Settings.ElemSize * _mapScale), tmpBitmap.Width, tmpBitmap.Height);
#if Debug
            Canva.DrawString(Convert.ToString(MapArray[i, j].PictNumber), new Font(new FontFamily("Arial"), 10), new SolidBrush(Color.Black),
              new Point(j * Settings.ElemSize, i * Settings.ElemSize));
#endif

#if Debug2
            if (Start.X != -1)
            {
              Canva.DrawString("Start", new Font(new FontFamily("Arial"), Settings.ElemSize/2), new SolidBrush(Color.Black),
                new Point(StartCanvaX + Convert.ToInt32(Start.X * Settings.ElemSize * MapScale), StartCanvaY + Convert.ToInt32(Start.Y * Settings.ElemSize * MapScale)));
            }
            if (Finish.X != -1)
            {
              Canva.DrawString("Finish", new Font(new FontFamily("Arial"),Settings.ElemSize/2), new SolidBrush(Color.Black),
                new Point(StartCanvaX + Convert.ToInt32(Finish.X * Settings.ElemSize * MapScale), StartCanvaY + Convert.ToInt32(Finish.Y * Settings.ElemSize * MapScale)));
            }
#endif
          }
          catch (Exception e)
          {
            System.Windows.Forms.MessageBox.Show(e.Message);
            Environment.Exit(1);
          }
        }
      }
      //Вывод пути
#if Debug3
      if (Way.Count != 0)
      {
        foreach (Point tmp in Way)
        {
          Canva.DrawEllipse(new Pen(new SolidBrush(Color.Red), 1), new Rectangle(StartCanvaX + Convert.ToInt32(tmp.X * Settings.ElemSize * MapScale) + Convert.ToInt32((Settings.ElemSize/2) * MapScale),
            StartCanvaY + Convert.ToInt32(tmp.Y * Settings.ElemSize * MapScale) + Convert.ToInt32((Settings.ElemSize/2) * MapScale), 3, 3));
        }
      }
#endif
    }

    //Сохранение в файл
    public bool SaveToFile(string pathToFile)
    {
      try
      {
        FileStream fileSaveStream = new FileStream(pathToFile, FileMode.Create, FileAccess.Write);
        IFormatter formatter = new BinaryFormatter();
        //Сначала запишем структуру типа Point, в которой X-ширина, Y-высота карты
        formatter.Serialize(fileSaveStream, new Point(Width, Height));
        //Сохраним точки старта и финиша
        formatter.Serialize(fileSaveStream, _start);
        formatter.Serialize(fileSaveStream, _finish);
        //Сохраним карту
        for (int i = 0; i < Height; i++)
          for (int j = 0; j < Width; j++)
          {
            formatter.Serialize(fileSaveStream, _mapArray[i, j]);
          }
        fileSaveStream.Close();
      }
      catch
      {
        return false;
      }
      return true;
    }

    //Добавление элемента на карту
    public bool AddElemToMap(Point coord, MapElem elem)
    {
      try
      {
        _mapArray[coord.Y, coord.X] = elem;
        if (MapStatusString[0].IndexOf("\\" + Convert.ToString(elem.PictNumber) + "\\", 0, StringComparison.Ordinal) != -1)
        {
          _mapArray[coord.Y, coord.X].Status = MapElemStatus.CanMove;
          return true;
        }
        if (MapStatusString[1].IndexOf("\\" + Convert.ToString(elem.PictNumber) + "\\", 0, StringComparison.Ordinal) != -1)
        {
          _mapArray[coord.Y, coord.X].Status = MapElemStatus.CanBuild;
          return true;
        }
        if (MapStatusString[2].IndexOf("\\" + Convert.ToString(elem.PictNumber) + "\\", 0, StringComparison.Ordinal) != -1)
        {
          _mapArray[coord.Y, coord.X].Status = MapElemStatus.BusyByUnit;
          return true;
        }
      }
      catch
      {
        return false;
      }
      return false;
    }

    //Установка позиции старта
    public bool SetStart(Point startPos)
    {
      if (_mapArray[startPos.Y, startPos.X].Status == MapElemStatus.CanMove)
      {
        _start = new Point(startPos.X, startPos.Y);
        return true;
      }
      return false;
    }

    //установка позиции финиша
    public bool SetFinish(Point finishPos)
    {
      if (_mapArray[finishPos.Y, finishPos.X].Status == MapElemStatus.CanMove)
      {
        _finish = new Point(finishPos.X, finishPos.Y);
        return true;
      }
      return false;
    }

    //Получение нового пути
    public void RebuildWay()
    {
      if ((_start.X == -1) & (_finish.X == -1))
        return;
      Way = new List<Point>();
      GetWay(_start, _finish);
      //System.Windows.Forms.MessageBox.Show("success");
    }
    //Рекурсивное истинное получение пути
    private void GetWay(Point pos, Point endPos)
    {
      if (!(((pos.Y >= 0) && (pos.Y < Height)) && ((pos.X >= 0) && (pos.X < Width))))
        return;
      if (_mapArray[pos.Y, pos.X].Status != MapElemStatus.CanMove)
        return;
      /*if (Way.Contains(Pos)){
        System.Windows.Forms.MessageBox.Show("WayBuildFailed");
        return;
      }*/
      _mapArray[pos.Y, pos.X].Status = MapElemStatus.BusyByUnit;
      Way.Add(pos);
      //Последующие проверки следят за тем чтобы если мы в предыдущем запуске
      //рекурсии нашли путь то дальше не запускаться
      if (!((pos.X == endPos.X) && (pos.Y == endPos.Y)))
      {
        Point tmp = new Point(pos.X + 1, pos.Y);
        GetWay(tmp, endPos);//столбец+1
        if (!((Way[Way.Count - 1].X == endPos.X) && (Way[Way.Count - 1].Y == endPos.Y)))
        {
          tmp = new Point(pos.X, pos.Y + 1);
          GetWay(tmp, endPos);//строка+1
        }
        if (!((Way[Way.Count - 1].X == endPos.X) && (Way[Way.Count - 1].Y == endPos.Y)))
        {
          tmp = new Point(pos.X, pos.Y - 1);
          GetWay(tmp, endPos);//строка-1
        }
        if (!((Way[Way.Count - 1].X == endPos.X) && (Way[Way.Count - 1].Y == endPos.Y)))
        {
          tmp = new Point(pos.X - 1, pos.Y);
          GetWay(tmp, endPos);//столбец-1
        }
      }
      _mapArray[pos.Y, pos.X].Status = MapElemStatus.CanMove;
    }

    //Изменение размера выводимых кусочков карты
    private void RebuildBitmaps()
    {
      for (int i = 0; i < Bitmaps.Length; i++)
      {
        _scaledBitmaps[i] = new Bitmap(Convert.ToInt32(Settings.ElemSize * _mapScale), Convert.ToInt32(Settings.ElemSize * _mapScale));
        Graphics canva = Graphics.FromImage(_scaledBitmaps[i]);
        canva.DrawImage(Bitmaps[i], 0, 0, _scaledBitmaps[i].Width, _scaledBitmaps[i].Height);
      }
    }

    //Получение постоянного изображения карты
    public void GetConstantBitmap(Bitmap workingBitmap, int width, int height)
    {
      Graphics canva = Graphics.FromImage(workingBitmap);
      ShowOnGraphics(canva);
    }

    //Получение статуса элемента карты
    public MapElemStatus GetMapElemStatus(int x, int y)
    {
      return _mapArray[y, x].Status;
    }

    //Установка статуса элемента карты
    public void SetMapElemStatus(int x, int y, MapElemStatus status)
    {
      _mapArray[y, x].Status = status;
    }

    //Установка видимой игроку площади
    public void ChangeVisibleArea(int dx = 0, int dy = 0)
    {
      if ((VisibleXStart + dx >= 0) && (VisibleXFinish + dx <= Width))
      {
        VisibleXStart += dx;
        VisibleXFinish += dx;
      }
      if ((VisibleYStart + dy >= 0) && (VisibleYFinish + dy <= Height))
      {
        VisibleYStart += dy;
        VisibleYFinish += dy;
      }
    }
  }
}