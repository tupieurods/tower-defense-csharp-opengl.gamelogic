using System;
using System.Drawing;

namespace GameCoClassLibrary
{
  public enum MonsterDirection { Up, Right, Down, Left };

  [Serializable]
  public struct BaseMonsterParams
  {
    public int HealthPoints;
    public float CanvasSpeed;
    public int Armor;
    public bool Invisible;
  }

  [Serializable]
  public struct MonsterParam
  {
    public int NumberOfPhases;
    public int NumberOfDirectionsInFile;
    public string AdditionalParams;
    public BaseMonsterParams Base;
    private Bitmap MonsterPict;
    public string SetMonsterPict
    {
      set
      {
        MonsterPict = new Bitmap(value);
      }
    }
    public Bitmap this[MonsterDirection direction, int phase]
    {
      get
      {
        if (MonsterPict == null)
          return null;
        int PhaseLength = MonsterPict.Size.Width / NumberOfPhases;
        Bitmap Tmp = MonsterPict.Clone(new Rectangle(PhaseLength * phase, 0, PhaseLength, MonsterPict.Size.Height), System.Drawing.Imaging.PixelFormat.Undefined);
        if ((NumberOfDirectionsInFile == 2) && (Tmp.Size.Width > Tmp.Size.Height))
        {
          throw new Exception("Incorrect phases number");
        }
        if ((NumberOfDirectionsInFile == 4) && ((Tmp.Size.Width*2) > Tmp.Size.Height))//Длина одной фазы*2 будет меньше высоты, если число фаз указано верно
          //в противном случае будет превышать высоту
        {
          throw new Exception("Incorrect phases number");
        }
        switch (direction)
        {
          case MonsterDirection.Right:
            #region Обработка в зависимости от числа направлений в файле
            switch (NumberOfDirectionsInFile)
            {
              case 1:
                Tmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                break;
              case 2:
                Tmp = Tmp.Clone(new Rectangle(0, 0, Tmp.Size.Width, Tmp.Size.Height - Tmp.Size.Width), System.Drawing.Imaging.PixelFormat.Undefined);
                Tmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                break;
              case 4:
                Tmp = Tmp.Clone(new Rectangle(0, (Tmp.Size.Height / 2) - Tmp.Size.Width, Tmp.Size.Width, (Tmp.Size.Height / 2) - Tmp.Size.Width),
                  System.Drawing.Imaging.PixelFormat.Undefined);
                break;
            }
            #endregion
            break;
          case MonsterDirection.Left:
            #region Обработка в зависимости от числа направлений в файле
            switch (NumberOfDirectionsInFile)
            {
              //case 1:В файле и так одно направление, влево
              //break;
              case 2:
                Tmp = Tmp.Clone(new Rectangle(0, 0, Tmp.Size.Width, Tmp.Size.Height - Tmp.Size.Width), System.Drawing.Imaging.PixelFormat.Undefined);
                break;
              case 4:
                Tmp = Tmp.Clone(new Rectangle(0, 0, Tmp.Size.Width, (Tmp.Size.Height / 2) - Tmp.Size.Width), System.Drawing.Imaging.PixelFormat.Undefined);
                break;
            }
            #endregion
            break;
          case MonsterDirection.Up:
            #region Обработка в зависимости от числа направлений в файле
            switch (NumberOfDirectionsInFile)
            {
              case 1:
                Tmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                break;
              case 2:
                Tmp = Tmp.Clone(new Rectangle(0, Tmp.Size.Height - Tmp.Size.Width, Tmp.Size.Width, Tmp.Size.Width),
                  System.Drawing.Imaging.PixelFormat.Undefined);
                break;
              case 4:
                Tmp = Tmp.Clone(new Rectangle(0, Tmp.Size.Height - (Tmp.Size.Width * 2), Tmp.Size.Width, Tmp.Size.Width),
                  System.Drawing.Imaging.PixelFormat.Undefined);
                break;
            }
            #endregion
            break;
          case MonsterDirection.Down:
            #region Обработка в зависимости от числа направлений в файле
            switch (NumberOfDirectionsInFile)
            {
              case 1:
                Tmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                break;
              case 2:
                Tmp = Tmp.Clone(new Rectangle(0, Tmp.Size.Height - Tmp.Size.Width, Tmp.Size.Width, Tmp.Size.Width),
                  System.Drawing.Imaging.PixelFormat.Undefined);
                Tmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                break;
              case 4:
                Tmp = Tmp.Clone(new Rectangle(0, Tmp.Size.Height - Tmp.Size.Width, Tmp.Size.Width, Tmp.Size.Width),
                  System.Drawing.Imaging.PixelFormat.Undefined);
                break;
            }
            #endregion
            break;
        }
        Tmp.MakeTransparent(Color.FromArgb(255,0,255));
        return Tmp;
      }
    }

    public MonsterParam(int NumberOfPhases, int HealthPoints, float CanvasSpeed, int Armor, string AdditionalParams, int NumberOfDirectionsInFile)
    {
      this.NumberOfPhases = NumberOfPhases;
      this.Base.HealthPoints = HealthPoints;
      this.Base.CanvasSpeed = CanvasSpeed;
      this.AdditionalParams = AdditionalParams;
      this.Base.Armor = Armor;
      this.MonsterPict = null;
      this.NumberOfDirectionsInFile = NumberOfDirectionsInFile;
      this.Base.Invisible = false;
    }
  }
}