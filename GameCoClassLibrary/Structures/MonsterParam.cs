using System;
using System.Collections.Generic;
using System.Drawing;
using GameCoClassLibrary.Enums;

namespace GameCoClassLibrary.Structures
{
  /// <summary>
  /// Basic monster parametrs
  /// </summary>
  [Serializable]
  public struct BaseMonsterParams
  {
    /// <summary>
    /// Healt points
    /// </summary>
    public int HealthPoints;

    /// <summary>
    /// Unit canvas speed
    /// </summary>
    public float CanvasSpeed;

    /// <summary>
    /// Unit Armor
    /// </summary>
    public int Armor;

    /// <summary>
    /// Is unit invisible
    /// </summary>
    public bool Invisible;
  }

  /// <summary>
  /// All monster parametrs
  /// </summary>
  [Serializable]
  public struct MonsterParam
  {
    /// <summary>
    /// Number of moving phases
    /// </summary>
    public int NumberOfPhases;

    /// <summary>
    /// Number of monster directions in file
    /// </summary>
    public int NumberOfDirectionsInFile;

    /// <summary>
    /// Additional params, for new fucntional without loosing old game configurations
    /// </summary>
    public string AdditionalParams;

    /// <summary>
    /// Basic params for this monster
    /// </summary>
    public BaseMonsterParams Base;

    /// <summary>
    /// Monster picture
    /// </summary>
    private Bitmap MonsterPict;

    /// <summary>
    /// Sets the monster pict.
    /// </summary>
    /// <value>
    /// The set monster pict.
    /// </value>
    public string SetMonsterPict
    {
      set { MonsterPict = new Bitmap(value); }
    }

    /// <summary>
    /// For caching
    /// </summary>
    private struct CacheElem
    {
      public int ID;
      public Bitmap CachedBitmap;
    }

    /// <summary>
    /// Cache
    /// </summary>
    [NonSerialized] private Dictionary<MonsterDirection, List<CacheElem>> _cache;

    /// <summary>
    /// Gets the <see cref="System.Drawing.Bitmap"/> with the specified direction and phase.
    /// </summary>
    public Bitmap this[MonsterDirection direction, int phase]
    {
      get
      {
        if(MonsterPict == null)
        {
          return null;
        }
        if(_cache == null)
        {
          _cache = new Dictionary<MonsterDirection, List<CacheElem>>();
        }
        if(!_cache.ContainsKey(direction))
        {
          _cache.Add(direction, new List<CacheElem>());
        }
        if(_cache[direction].Find(x => x.ID == phase).CachedBitmap != null)
        {
          return _cache[direction].Find(x => x.ID == phase).CachedBitmap;
        }
        int phaseLength = MonsterPict.Size.Width / NumberOfPhases;
        Bitmap tmp = MonsterPict.Clone(new Rectangle(phaseLength * phase, 0, phaseLength, MonsterPict.Size.Height),
                                       System.Drawing.Imaging.PixelFormat.Undefined);
        if((NumberOfDirectionsInFile == 2) && (tmp.Size.Width > tmp.Size.Height))
        {
          throw new Exception("Incorrect phases number");
        }
        if((NumberOfDirectionsInFile == 4) && ((tmp.Size.Width * 2) > tmp.Size.Height))
          //Длина одной фазы*2 будет меньше высоты, если число фаз указано верно
          //в противном случае будет превышать высоту
        {
          throw new Exception("Incorrect phases number");
        }
        switch(direction)
        {
          case MonsterDirection.Right:

            #region Обработка в зависимости от числа направлений в файле

            switch(NumberOfDirectionsInFile)
            {
              case 1:
                tmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                break;
              case 2:
                tmp = tmp.Clone(new Rectangle(0, 0, tmp.Size.Width, tmp.Size.Height - tmp.Size.Width),
                                System.Drawing.Imaging.PixelFormat.Undefined);
                tmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                break;
              case 4:
                tmp =
                  tmp.Clone(
                    new Rectangle(0, (tmp.Size.Height / 2) - tmp.Size.Width, tmp.Size.Width,
                                  (tmp.Size.Height / 2) - tmp.Size.Width),
                    System.Drawing.Imaging.PixelFormat.Undefined);
                break;
            }

            #endregion

            break;
          case MonsterDirection.Left:

            #region Обработка в зависимости от числа направлений в файле

            switch(NumberOfDirectionsInFile)
            {
                //case 1:В файле и так одно направление, влево
                //break;
              case 2:
                tmp = tmp.Clone(new Rectangle(0, 0, tmp.Size.Width, tmp.Size.Height - tmp.Size.Width),
                                System.Drawing.Imaging.PixelFormat.Undefined);
                break;
              case 4:
                tmp = tmp.Clone(new Rectangle(0, 0, tmp.Size.Width, (tmp.Size.Height / 2) - tmp.Size.Width),
                                System.Drawing.Imaging.PixelFormat.Undefined);
                break;
            }

            #endregion

            break;
          case MonsterDirection.Up:

            #region Обработка в зависимости от числа направлений в файле

            switch(NumberOfDirectionsInFile)
            {
              case 1:
                tmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                break;
              case 2:
                tmp = tmp.Clone(new Rectangle(0, tmp.Size.Height - tmp.Size.Width, tmp.Size.Width, tmp.Size.Width),
                                System.Drawing.Imaging.PixelFormat.Undefined);
                break;
              case 4:
                tmp = tmp.Clone(
                  new Rectangle(0, tmp.Size.Height - (tmp.Size.Width * 2), tmp.Size.Width, tmp.Size.Width),
                  System.Drawing.Imaging.PixelFormat.Undefined);
                break;
            }

            #endregion

            break;
          case MonsterDirection.Down:

            #region Обработка в зависимости от числа направлений в файле

            switch(NumberOfDirectionsInFile)
            {
              case 1:
                tmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                break;
              case 2:
                tmp = tmp.Clone(new Rectangle(0, tmp.Size.Height - tmp.Size.Width, tmp.Size.Width, tmp.Size.Width),
                                System.Drawing.Imaging.PixelFormat.Undefined);
                tmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                break;
              case 4:
                tmp = tmp.Clone(new Rectangle(0, tmp.Size.Height - tmp.Size.Width, tmp.Size.Width, tmp.Size.Width),
                                System.Drawing.Imaging.PixelFormat.Undefined);
                break;
            }

            #endregion

            break;
        }
        CacheElem tmpCache = new CacheElem {ID = phase};
        tmp.MakeTransparent(Color.FromArgb(255, 0, 255));
        tmpCache.CachedBitmap = new Bitmap(tmp);
        _cache[direction].Add(tmpCache);
        return tmp;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MonsterParam"/> struct.
    /// </summary>
    /// <param name="numberOfPhases">The number of phases.</param>
    /// <param name="healthPoints">The health points.</param>
    /// <param name="canvasSpeed">The canvas speed.</param>
    /// <param name="armor">The armor.</param>
    /// <param name="additionalParams">The additional params.</param>
    /// <param name="numberOfDirectionsInFile">The number of directions in file.</param>
    public MonsterParam(int numberOfPhases, int healthPoints, float canvasSpeed, int armor, string additionalParams,
                        int numberOfDirectionsInFile)
    {
      NumberOfPhases = numberOfPhases;
      Base.HealthPoints = healthPoints;
      Base.CanvasSpeed = canvasSpeed;
      AdditionalParams = additionalParams;
      Base.Armor = armor;
      MonsterPict = null;
      NumberOfDirectionsInFile = numberOfDirectionsInFile;
      Base.Invisible = false;
      _cache = null;
    }
  }
}