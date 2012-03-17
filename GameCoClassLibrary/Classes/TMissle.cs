using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GameCoClassLibrary.Enums;

namespace GameCoClassLibrary
{
  class TMissle
  {
    #region Private
    private int Damadge;//урон
    private int AimID;//ID цели
    private eTowerType MissleType;//Тип снаряда
    private Color MisslePenColor;//Цвет карандаша
    private Color MissleBrushColor;//цвет кисти
    private eModificatorName Modificator;//Модификатор
    private PointF Position;//Позиция на канве
    private int Progress;//Временно неизменяемо
    #endregion

    #region Public
    public bool DestroyMe//обозначает что нужно удалить из списка снарядов
    {
      get;
      private set;
    }
    static public float Scaling
    {
      get;
      set;
    }
    #endregion

    private TMissle()
    {
    }

    public TMissle(int AimID, int Damadge,
     eTowerType MissleType,
     Color MisslePenColor,
     Color MissleBrushColor,
     eModificatorName Modificator, PointF Position, int Progress = 30)
    {
      this.AimID = AimID;
      this.Damadge = Damadge;
      this.MissleType = MissleType;
      this.MissleBrushColor = MissleBrushColor;
      this.MisslePenColor = MisslePenColor;
      this.Modificator = Modificator;
      this.DestroyMe = false;
      this.Position = new PointF(Position.X, Position.Y);
      this.Progress = Progress;
    }

    public void Move(IEnumerable<TMonster> Monsters)
    {
      Func<TMonster, bool> predicate = (Elem) => Elem.ID == AimID;
      TMonster Aim;
      try
      {
        Aim = Monsters.First<TMonster>(predicate);
      }
      catch
      {
        DestroyMe = true;
        return;
      }
      //Вычисляем смещение снаряда
      int Dx = (int)Math.Abs((Aim.GetCanvaPos.X - Position.X) / Progress);
      int Dy = (int)Math.Abs((Aim.GetCanvaPos.Y - Position.Y) / Progress);
      //Проверям положение снаряда и цели, для правильного полёта по X:
      if (Position.X > Aim.GetCanvaPos.X)
        Position.X -= Dx;
      else
        Position.X += Dx;
      //По Y:
      if (Position.Y > Aim.GetCanvaPos.Y)
        Position.Y -= Dy;
      else
        Position.Y += Dy;
      //Уменьшаем число фаз полёта
      Progress--;
      //Если снаряд долетел до цели
      if (Progress == 0)
      {
        DestroyMe = true;
        Aim.GetDamadge(Damadge, Modificator);//В любом случае башния должна нанести урон цели в которую стреляла
        switch (MissleType)
        {
          case eTowerType.Splash:
            var SplashedAims = from Monster in Monsters
                               where Monster.ID!=AimID
                               where (Math.Sqrt(Math.Pow(Monster.GetCanvaPos.X - Aim.GetCanvaPos.X, 2) + Math.Pow(Monster.GetCanvaPos.Y - Aim.GetCanvaPos.Y, 2))) <= (70)
                               select Monster;
            foreach (var Monster in SplashedAims)
              Monster.GetDamadge((int)(Damadge * 0.5), Modificator != eModificatorName.Posion ? Modificator : eModificatorName.NoEffect, false);//нельзя Posion effect
            //делать сплешевым
            break;
          case eTowerType.Simple:
            break;
        }
      }
    }

    public void Show(Graphics Canva, Point VisibleStart, Point VisibleFinish, IEnumerable<TMonster> Monsters, int DX = 10, int DY = 10)
    {
      if (DestroyMe)
        return;
      //Проверка снаряда на видимость
      if ((Position.X - VisibleStart.X * Settings.ElemSize < 5) || (Position.Y - VisibleStart.Y * Settings.ElemSize < 5) ||
        (-Position.X + VisibleFinish.X * Settings.ElemSize < 5) || (-Position.Y + VisibleFinish.Y * Settings.ElemSize < 5))
        return;
      Func<TMonster, bool> predicate = (Elem) => Elem.ID == AimID;
      Point AimPos = new Point((int)Monsters.First<TMonster>(predicate).GetCanvaPos.X,
        (int)Monsters.First<TMonster>(predicate).GetCanvaPos.Y);
      switch (MissleType)
      {
        case eTowerType.Simple:
          float Tang;
          if (((Position.X - AimPos.X) != 0) && ((Position.Y - AimPos.Y) != 0))
            Tang = Math.Abs((Position.Y - AimPos.Y) / (Position.X - AimPos.X));
          else
            Tang = 1;
          Point SecondPosition;//Позиция конца снаряда
          if (Position.X > AimPos.X)
          {
            if (Position.Y > AimPos.Y)
              SecondPosition = new Point(
                Convert.ToInt32(Position.X + 10 * Math.Sqrt(1 / (1 + Math.Pow(Tang, 2)))),
                Convert.ToInt32(Position.Y + 10 * Math.Sqrt(1 / (1 + Math.Pow(1 / Tang, 2)))));
            else
              SecondPosition = new Point(
                Convert.ToInt32(Position.X + 10 * Math.Sqrt(1 / (1 + Math.Pow(Tang, 2)))),
                Convert.ToInt32(Position.Y - 10 * Math.Sqrt(1 / (1 + Math.Pow(1 / Tang, 2)))));
          }
          else
          {
            if (Position.Y > AimPos.Y)
              SecondPosition = new Point(
                Convert.ToInt32(Position.X - 10 * Math.Sqrt(1 / (1 + Math.Pow(Tang, 2)))),
                Convert.ToInt32((Position.Y + 10 * Math.Sqrt(1 / (1 + Math.Pow(1 / Tang, 2))))));
            else
              SecondPosition = new Point(
                Convert.ToInt32(Position.X - 10 * Math.Sqrt(1 / (1 + Math.Pow(Tang, 2)))),
                Convert.ToInt32(Position.Y - 10 * Math.Sqrt(1 / (1 + Math.Pow(1 / Tang, 2)))));
          }
          Canva.DrawLine(new Pen(MisslePenColor, 2),
            new Point((int)((Position.X - VisibleStart.X * Settings.ElemSize) * Scaling) + DX,
              (int)((Position.Y - VisibleStart.Y * Settings.ElemSize) * Scaling) + DY),
            new Point((int)((SecondPosition.X - VisibleStart.X * Settings.ElemSize) * Scaling) + DX,
              (int)((SecondPosition.Y - VisibleStart.Y * Settings.ElemSize) * Scaling) + DY));
          break;
        case eTowerType.Splash:
          Canva.FillEllipse(new SolidBrush(MissleBrushColor),
            (int)(Position.X - 5 - VisibleStart.X * Settings.ElemSize) * Scaling + DX,
            (int)(Position.Y - 5 - VisibleStart.Y * Settings.ElemSize) * Scaling + DY,
            10 * Scaling, 10 * Scaling);
          Canva.DrawEllipse(new Pen(MisslePenColor),
            (int)(Position.X - 5 - VisibleStart.X * Settings.ElemSize) * Scaling + DX,
            (int)(Position.Y - 5 - VisibleStart.Y * Settings.ElemSize) * Scaling + DY,
            10 * Scaling, 10 * Scaling);
          break;
      }
    }
  }
}
