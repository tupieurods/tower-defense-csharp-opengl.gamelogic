using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GameCoClassLibrary.Enums;

namespace GameCoClassLibrary.Classes
{
  internal class Missle
  {
    #region Private

    private readonly int _damadge;//урон
    private readonly int _aimID;//ID цели
    private readonly eTowerType _missleType;//Тип снаряда
    private readonly Color _misslePenColor;//Цвет карандаша
    private readonly Color _missleBrushColor;//цвет кисти
    private readonly eModificatorName _modificator;//Модификатор
    private PointF _position;//Позиция на канве
    private int _progress;//Временно неизменяемо

    #endregion Private

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

    #endregion Public

    public Missle(int aimID, int damadge,
     eTowerType missleType,
     Color misslePenColor,
     Color missleBrushColor,
     eModificatorName modificator, PointF position, int progress = 30)
    {
      _aimID = aimID;
      _damadge = damadge;
      _missleType = missleType;
      _missleBrushColor = missleBrushColor;
      _misslePenColor = misslePenColor;
      _modificator = modificator;
      DestroyMe = false;
      _position = new PointF(position.X, position.Y);
      _progress = progress;
    }

    public void Move(IEnumerable<Monster> monsters)
    {
      #region Getting Monster
      Func<Monster, bool> predicate = elem => elem.ID == _aimID;
      Monster aim;
      try
      {
        // ReSharper disable PossibleMultipleEnumeration
        aim = monsters.First(predicate);
        // ReSharper restore PossibleMultipleEnumeration
      }
      catch
      {
        DestroyMe = true;
        return;
      }
      #endregion
      #region Missle canvas moving
      //Вычисляем смещение снаряда
      int dx = (int)Math.Abs((aim.GetCanvaPos.X - _position.X) / _progress);
      int dy = (int)Math.Abs((aim.GetCanvaPos.Y - _position.Y) / _progress);
      //Проверям положение снаряда и цели, для правильного полёта по X:
      if (_position.X > aim.GetCanvaPos.X)
        _position.X -= dx;
      else
        _position.X += dx;
      //По Y:
      if (_position.Y > aim.GetCanvaPos.Y)
        _position.Y -= dy;
      else
        _position.Y += dy;
      #endregion
      //Уменьшаем число фаз полёта
      _progress--;
      //Если снаряд долетел до цели
      if (_progress != 0)//No contact with aim
        return;
      #region Damadge if contact
      {
        DestroyMe = true;
        aim.GetDamadge(_damadge, _modificator); //В любом случае башния должна нанести урон цели в которую стреляла
        switch (_missleType)
        {
          case eTowerType.Splash:
            // ReSharper disable PossibleMultipleEnumeration
            var splashedAims = from monster in monsters
                               // ReSharper restore PossibleMultipleEnumeration
                               where monster.ID != _aimID
                               where
                                 (Math.Sqrt(Math.Pow(monster.GetCanvaPos.X - aim.GetCanvaPos.X, 2) +
                                            Math.Pow(monster.GetCanvaPos.Y - aim.GetCanvaPos.Y, 2))) <= (70)
                               select monster;
            foreach (var monster in splashedAims)
              monster.GetDamadge((int) (_damadge*0.5),
                                 _modificator != eModificatorName.Posion ? _modificator : eModificatorName.NoEffect,false);
            break;
          case eTowerType.Simple:
            break;
        }
      }
      #endregion
    }

    public void Show(Graphics canva, Point visibleStart, Point visibleFinish, IEnumerable<Monster> monsters)
    {
      if (DestroyMe)
        return;
      //Проверка снаряда на видимость
      if ((_position.X - visibleStart.X * Settings.ElemSize < 5) || (_position.Y - visibleStart.Y * Settings.ElemSize < 5) ||
        (-_position.X + visibleFinish.X * Settings.ElemSize < 5) || (-_position.Y + visibleFinish.Y * Settings.ElemSize < 5))
        return;
      Func<Monster, bool> predicate = elem => elem.ID == _aimID;
      if (monsters == null) return;
      // ReSharper disable PossibleMultipleEnumeration
      Point aimPos = new Point((int)monsters.First(predicate).GetCanvaPos.X,
                               (int)monsters.First(predicate).GetCanvaPos.Y);
      // ReSharper restore PossibleMultipleEnumeration
      switch (_missleType)
      {
        case eTowerType.Simple:
          float tang;
          if ((Math.Abs((_position.X - aimPos.X) - 0) > 0.01) && (Math.Abs((_position.Y - aimPos.Y) - 0) > 0.01))
            tang = Math.Abs((_position.Y - aimPos.Y) / (_position.X - aimPos.X));
          else
            tang = 1;
          Point secondPosition;//Позиция конца снаряда
          if (_position.X > aimPos.X)
          {
            if (_position.Y > aimPos.Y)
              secondPosition = new Point(
                Convert.ToInt32(_position.X + 10 * Math.Sqrt(1 / (1 + Math.Pow(tang, 2)))),
                Convert.ToInt32(_position.Y + 10 * Math.Sqrt(1 / (1 + Math.Pow(1 / tang, 2)))));
            else
              secondPosition = new Point(
                Convert.ToInt32(_position.X + 10 * Math.Sqrt(1 / (1 + Math.Pow(tang, 2)))),
                Convert.ToInt32(_position.Y - 10 * Math.Sqrt(1 / (1 + Math.Pow(1 / tang, 2)))));
          }
          else
          {
            if (_position.Y > aimPos.Y)
              secondPosition = new Point(
                Convert.ToInt32(_position.X - 10 * Math.Sqrt(1 / (1 + Math.Pow(tang, 2)))),
                Convert.ToInt32((_position.Y + 10 * Math.Sqrt(1 / (1 + Math.Pow(1 / tang, 2))))));
            else
              secondPosition = new Point(
                Convert.ToInt32(_position.X - 10 * Math.Sqrt(1 / (1 + Math.Pow(tang, 2)))),
                Convert.ToInt32(_position.Y - 10 * Math.Sqrt(1 / (1 + Math.Pow(1 / tang, 2)))));
          }
          canva.DrawLine(new Pen(_misslePenColor, 2),
                         new Point((int)((_position.X - visibleStart.X * Settings.ElemSize) * Scaling) + Settings.DeltaX,
                                   (int)((_position.Y - visibleStart.Y * Settings.ElemSize) * Scaling) + Settings.DeltaY),
                         new Point((int)((secondPosition.X - visibleStart.X * Settings.ElemSize) * Scaling) + Settings.DeltaX,
                                   (int)((secondPosition.Y - visibleStart.Y * Settings.ElemSize) * Scaling) + Settings.DeltaY));
          break;
        case eTowerType.Splash:
          canva.FillEllipse(new SolidBrush(_missleBrushColor),
                            (int)(_position.X - 5 - visibleStart.X * Settings.ElemSize) * Scaling + Settings.DeltaX,
                            (int)(_position.Y - 5 - visibleStart.Y * Settings.ElemSize) * Scaling + Settings.DeltaY,
                            10 * Scaling, 10 * Scaling);
          canva.DrawEllipse(new Pen(_misslePenColor),
                            (int)(_position.X - 5 - visibleStart.X * Settings.ElemSize) * Scaling + Settings.DeltaX,
                            (int)(_position.Y - 5 - visibleStart.Y * Settings.ElemSize) * Scaling + Settings.DeltaY,
                            10 * Scaling, 10 * Scaling);
          break;
      }
    }
  }
}