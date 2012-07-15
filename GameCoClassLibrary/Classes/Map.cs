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

  /// <summary>
  /// Map class
  /// </summary>
  public class Map
  {
    #region Private
    /// <summary>
    /// Logical representation of the map
    /// </summary>
    private readonly MapElem[,] _mapArray;
    /// <summary>
    /// Start position of the way at map
    /// </summary>
    private Point _start = new Point(-1, -1);
    /// <summary>
    /// Finish position of the way at map
    /// </summary>
    private Point _finish = new Point(-1, -1);
    /// <summary>
    /// Link between PictNumber and MapElemStatus
    /// </summary>
    private static readonly string[] MapStatusString;
    //0-CanMove
    //1-CanBuild
    //2,3-BusyByUnit. For bigger Map
    /// <summary>
    /// Scaling factor
    /// </summary>
    private static float _mapScale = 1.0F;
    /// <summary>
    /// Chache, scaled bitmaps
    /// </summary>
    private readonly Bitmap[] _scaledBitmaps;
    #endregion

    #region Public
    /// <summary>
    /// Gets the bitmaps for field visualization
    /// </summary>
    public static Bitmap[] Bitmaps { get; private set; }
    /// <summary>
    /// Gets the way.
    /// </summary>
    public List<Point> Way { get; private set; }
    /// <summary>
    /// Gets the width.
    /// </summary>
    public int Width { get; private set; }
    /// <summary>
    /// Gets the height.
    /// </summary>
    public int Height { get; private set; }
    /// <summary>
    /// Gets the visible X start.
    /// </summary>
    public int VisibleXStart { get; private set; }
    /// <summary>
    /// Gets the visible X finish.
    /// </summary>
    public int VisibleXFinish { get; private set; }
    /// <summary>
    /// Gets the visible Y start.
    /// </summary>
    public int VisibleYStart { get; private set; }
    /// <summary>
    /// Gets the visible Y finish.
    /// </summary>
    public int VisibleYFinish { get; private set; }
    /// <summary>
    /// Gets or sets scaling factor
    /// </summary>
    /// <value>
    /// Scaling factor
    /// </value>
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
    /// <summary>
    /// Initializes the <see cref="Map"/> class.
    /// Loads:Map configuration, bitmaps
    /// </summary>
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
          string[] confFileSection = new[] { "CanMove", "CanBuild", "BusyByUnit" };
          #region Configuration File Processing
          while ((s = settingsFileStrm.ReadLine()) != null)
          {
            if (s.IndexOf("CountMapElemPict", 0, StringComparison.Ordinal) != -1)
            {
              Bitmaps = new Bitmap[Convert.ToInt32(s.Substring((s.IndexOf(' ') + 1)))];
              continue;
            }
            for (int i = 0; i < confFileSection.Length; i++)
              if (s.IndexOf(confFileSection[i], 0, StringComparison.Ordinal) != -1)
              {
                MapStatusString[i] = s.Substring((s.IndexOf(' ') + 1));
                break;
              }
          }
          #endregion
        }
        catch
        {
          System.Windows.Forms.MessageBox.Show("Configuration file loading error. Can't continue");
          Environment.Exit(1);
        }
        finally
        {
          if (Bitmaps.Length == 0)
            Bitmaps = new Bitmap[4];
        }
      }
      //Bitmaps loading
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

    /// <summary>
    /// Initializes a new instance of the <see cref="Map"/> class.
    /// Using in Map Editor
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Map(int width, int height)
    {
      Width = VisibleXFinish = width;
      Height = VisibleYFinish = height;
      VisibleXStart = 0;
      VisibleYStart = 0;
      _mapArray = new MapElem[height, width];//[lines,columns]
      _start = new Point(-1, -1);
      _finish = new Point(-1, -1);
      Way = new List<Point>();
      _scaledBitmaps = new Bitmap[Bitmaps.Length];
      RebuildBitmaps();
      for (int i = 0; i < height; i++)
        for (int j = 0; j < width; j++)
          _mapArray[i, j] = new MapElem(1, 0, MapElemStatus.CanBuild);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Map"/> class.
    /// </summary>
    /// <param name="pathToFile">The path to file.</param>
    /// <param name="clippedArea">if set to <c>true</c> than class must render only visible part of the map.</param>
    public Map(string pathToFile, bool clippedArea = false/*, int vXStart = 0, int vYStart = 0, int vXFinish = 30, int vYFinish = 30*/)
    {
      try
      {
        FileStream fileLoadStream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
        IFormatter formatter = new BinaryFormatter();
        //Read Point struct, where X-Width, Y-Height
        Point tmp = (Point)formatter.Deserialize(fileLoadStream);
        Width = VisibleXFinish = tmp.X;
        Height = VisibleYFinish = tmp.Y;
        if (clippedArea)
        {
          VisibleXFinish = Width > 30 ? 30/*vXFinish*/ : VisibleXFinish;
          VisibleYFinish = Height > 30 ? 30/*vYFinish*/ : VisibleYFinish;
        }
        VisibleXStart = 0; //vXStart;
        VisibleYStart = 0; //vYStart;
        //Start and end of the way
        _start = (Point)formatter.Deserialize(fileLoadStream);
        _finish = (Point)formatter.Deserialize(fileLoadStream);
        //The map
        _mapArray = new MapElem[Height, Width];
        for (int i = 0; i < Height; i++)
          for (int j = 0; j < Width; j++)
          {
            _mapArray[i, j] = (MapElem)(formatter.Deserialize(fileLoadStream));
          }
        fileLoadStream.Close();
        Way = new List<Point>();
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

    /// <summary>
    /// Shows the on graphics.
    /// </summary>
    /// <param name="canva">The canva.</param>
    /// <param name="startCanvaX">The start canva X.</param>
    /// <param name="startCanvaY">The start canva Y.</param>
    /// <param name="finishCanvaX">The finish canva X.</param>
    /// <param name="finishCanvaY">The finish canva Y.</param>
    /// <param name="showWay">if true, drawing way on the map </param>
    public void ShowOnGraphics(Graphics canva, bool showWay = false, int startCanvaX = 0, int startCanvaY = 0/*, int finishCanvaX = 6000, int finishCanvaY = 6000*/)
    {
      if (canva == null)
        throw new ArgumentNullException("canva");
      //{start|finish}Canva{X|Y} for future, may be its useless and will be removed
      int realY = 0;
      for (int i = VisibleYStart; i < VisibleYFinish; i++, realY++)
      {
        /*if ((i * ElemSize * MapScale) > FinishCanvaY)//moved out from drawing space by   Y
          break;*/
        int realX = 0;
        for (int j = VisibleXStart; j < VisibleXFinish; j++, realX++)
        {
          if (_mapArray[i, j].PictNumber == -1)//пустой элемент
            continue;
          /*if ((j * Settings.ElemSize * MapScale) > FinishCanvaX)//moved out from drawing space by X
            break;*/
          try
          {
            Bitmap tmpBitmap = new Bitmap(_scaledBitmaps[_mapArray[i, j].PictNumber]);
            for (int k = 0; k < _mapArray[i, j].AngleOfRotate; k++)
              tmpBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            canva.DrawImage(tmpBitmap, Convert.ToInt32(startCanvaX + realX * Settings.ElemSize * _mapScale),
                            Convert.ToInt32(startCanvaY + realY * Settings.ElemSize * _mapScale), tmpBitmap.Width,
                            tmpBitmap.Height);
          }
          catch (Exception e)
          {
            System.Windows.Forms.MessageBox.Show(e.Message);
            Environment.Exit(1);
          }
        }
      }
      if (showWay)
      {
        if (_start.X != -1)
        {
          canva.DrawString("Start", new Font(new FontFamily("Arial"), Settings.ElemSize / 2), new SolidBrush(Color.Black),
            new Point(startCanvaX + Convert.ToInt32(_start.X * Settings.ElemSize * _mapScale), startCanvaY + Convert.ToInt32(_start.Y * Settings.ElemSize * _mapScale)));
        }
        if (_finish.X != -1)
        {
          canva.DrawString("Finish", new Font(new FontFamily("Arial"), Settings.ElemSize / 2), new SolidBrush(Color.Black),
            new Point(startCanvaX + Convert.ToInt32(_finish.X * Settings.ElemSize * _mapScale), startCanvaY + Convert.ToInt32(_finish.Y * Settings.ElemSize * _mapScale)));
        }
        if (Way.Count != 0)
        {
          foreach (Point tmp in Way)
          {
            canva.DrawEllipse(new Pen(new SolidBrush(Color.Red), 1), new Rectangle(startCanvaX + Convert.ToInt32(tmp.X * Settings.ElemSize * _mapScale) + Convert.ToInt32((Settings.ElemSize / 2) * _mapScale),
              startCanvaY + Convert.ToInt32(tmp.Y * Settings.ElemSize * _mapScale) + Convert.ToInt32((Settings.ElemSize / 2) * _mapScale), 3, 3));
          }
        }
      }
    }


    /// <summary>
    /// Saves map to file.
    /// </summary>
    /// <param name="pathToFile">The path to file.</param>
    /// <returns></returns>
    public bool SaveToFile(string pathToFile)
    {
      try
      {
        FileStream fileSaveStream = new FileStream(pathToFile, FileMode.Create, FileAccess.Write);
        IFormatter formatter = new BinaryFormatter();
        //Write Point struct, where X-Width, Y-Height
        formatter.Serialize(fileSaveStream, new Point(Width, Height));
        //Save start and finish point
        formatter.Serialize(fileSaveStream, _start);
        formatter.Serialize(fileSaveStream, _finish);
        //Write the map to file
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

    /// <summary>
    /// Adds the elem to map.
    /// </summary>
    /// <param name="coord">The coord.</param>
    /// <param name="elem">The elem.</param>
    /// <returns></returns>
    public bool AddElemToMap(Point coord, MapElem elem)
    {
      try
      {
        _mapArray[coord.Y, coord.X] = elem;
        for (int i = 0; i < MapStatusString.Length; i++)
        {
          if (MapStatusString[i].IndexOf("\\" + Convert.ToString(elem.PictNumber) + "\\", 0, StringComparison.Ordinal) != -1)
          {
            _mapArray[coord.Y, coord.X].Status = (MapElemStatus)i;
            return true;
          }
        }
      }
      catch
      {
        return false;
      }
      return false;
    }

    /// <summary>
    /// Sets the start.
    /// </summary>
    /// <param name="startPos">The start pos.</param>
    /// <returns></returns>
    public bool SetStart(Point startPos)
    {
      if (_mapArray[startPos.Y, startPos.X].Status == MapElemStatus.CanMove)
      {
        _start = new Point(startPos.X, startPos.Y);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Sets the finish.
    /// </summary>
    /// <param name="finishPos">The finish pos.</param>
    /// <returns></returns>
    public bool SetFinish(Point finishPos)
    {
      if (_mapArray[finishPos.Y, finishPos.X].Status == MapElemStatus.CanMove)
      {
        _finish = new Point(finishPos.X, finishPos.Y);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Rebuilds the way.
    /// </summary>
    public void RebuildWay()
    {
      if ((_start.X == -1) & (_finish.X == -1))
        return;
      Way = new List<Point>();
      GetWay(_start, _finish);
    }

    //TODO Change GetWay algorithm
    /// <summary>
    /// Recursive path finder
    /// </summary>
    /// <param name="pos">Current postion</param>
    /// <param name="endPos">End position</param>
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
      if (!((pos.X == endPos.X) && (pos.Y == endPos.Y)))
      {
        Point tmp = new Point(pos.X + 1, pos.Y);
        GetWay(tmp, endPos);//column+1
        if (!((Way[Way.Count - 1].X == endPos.X) && (Way[Way.Count - 1].Y == endPos.Y)))
        {
          tmp = new Point(pos.X, pos.Y + 1);
          GetWay(tmp, endPos);//line+1
        }
        if (!((Way[Way.Count - 1].X == endPos.X) && (Way[Way.Count - 1].Y == endPos.Y)))
        {
          tmp = new Point(pos.X, pos.Y - 1);
          GetWay(tmp, endPos);//line-1
        }
        if (!((Way[Way.Count - 1].X == endPos.X) && (Way[Way.Count - 1].Y == endPos.Y)))
        {
          tmp = new Point(pos.X - 1, pos.Y);
          GetWay(tmp, endPos);//column-1
        }
      }
      _mapArray[pos.Y, pos.X].Status = MapElemStatus.CanMove;
    }

    /// <summary>
    /// Rebuilds the bitmaps.
    /// </summary>
    private void RebuildBitmaps()
    {
      for (int i = 0; i < Bitmaps.Length; i++)
      {
        _scaledBitmaps[i] = new Bitmap(Convert.ToInt32(Settings.ElemSize * _mapScale), Convert.ToInt32(Settings.ElemSize * _mapScale));
        Graphics canva = Graphics.FromImage(_scaledBitmaps[i]);
        canva.DrawImage(Bitmaps[i], 0, 0, _scaledBitmaps[i].Width, _scaledBitmaps[i].Height);
      }
    }

    /// <summary>
    /// Gets the constant bitmap of visible area. Caching
    /// </summary>
    /// <param name="workingBitmap">The working bitmap.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    internal void GetConstantBitmap(Bitmap workingBitmap, int width, int height)
    {
      Graphics canva = Graphics.FromImage(workingBitmap);
      ShowOnGraphics(canva);
    }

    /// <summary>
    /// Gets the map elem status.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    internal MapElemStatus GetMapElemStatus(int x, int y)
    {
      return _mapArray[y, x].Status;
    }

    /// <summary>
    /// Sets the map elem status.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="status">The status.</param>
    internal void SetMapElemStatus(int x, int y, MapElemStatus status)
    {
      _mapArray[y, x].Status = status;
    }

    /// <summary>
    /// Changes the visible area.
    /// </summary>
    /// <param name="dx">Visible area dx</param>
    /// <param name="dy">Visible area dy</param>
    internal void ChangeVisibleArea(int dx = 0, int dy = 0)
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