using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Interfaces;

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

    private Missle(int aimID, int damadge,
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

    public static Missle Factory(FactoryAct act, params object[] listOfParams)
    {
      try
      {
        Missle result;
        switch (act)
        {
          case FactoryAct.Create:
            result = new Missle((int)listOfParams[0], (int)listOfParams[1], (eTowerType)listOfParams[2], (Color)listOfParams[3], (Color)listOfParams[4], (eModificatorName)listOfParams[5], new PointF(Convert.ToSingle(listOfParams[6]), Convert.ToSingle(listOfParams[7])) /*, (int)listOfParams[8]*//*Прогресс*/);
            break;
          case FactoryAct.Load:
            BinaryReader reader = (BinaryReader)listOfParams[0];
            // ReSharper disable RedundantArgumentName
            result = new Missle(aimID: reader.ReadInt32(), damadge: reader.ReadInt32(), missleType: (eTowerType)reader.ReadInt32(), misslePenColor: Color.FromArgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()), missleBrushColor: Color.FromArgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()), modificator: (eModificatorName)reader.ReadInt32(), position: new PointF(reader.ReadSingle(), reader.ReadSingle()), progress: reader.ReadInt32());
            // ReSharper restore RedundantArgumentName
            break;
          default:
            throw new ArgumentOutOfRangeException("act");
        }
        return result;
      }
      catch (Exception exc)
      {
        throw;
      }
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
              monster.GetDamadge((int)(_damadge * 0.5),
                                 _modificator != eModificatorName.Posion ? _modificator : eModificatorName.NoEffect, false);
            break;
          case eTowerType.Simple:
            break;
        }
      }
      #endregion
    }

    public void Show(IGraphic canva, Point visibleStart, Point visibleFinish, IEnumerable<Monster> monsters)
    {
      if ((DestroyMe)/*нужно уничтожить*/ ||
        ((_position.X - visibleStart.X * Settings.ElemSize < 5) || (_position.Y - visibleStart.Y * Settings.ElemSize < 5) ||
        (-_position.X + visibleFinish.X * Settings.ElemSize < 5) || (-_position.Y + visibleFinish.Y * Settings.ElemSize < 5))/*не видим*/
        || (monsters == null)/*внезапно все монстры мертвы*/)
        return;
      /*//Проверка снаряда на видимость
      if ((_position.X - visibleStart.X * Settings.ElemSize < 5) || (_position.Y - visibleStart.Y * Settings.ElemSize < 5) ||
        (-_position.X + visibleFinish.X * Settings.ElemSize < 5) || (-_position.Y + visibleFinish.Y * Settings.ElemSize < 5))
        return;
      if (monsters == null) return;*/
      var monster = monsters.FirstOrDefault(elem => elem.ID == _aimID);
      if (monster == null)
      {
        DestroyMe = true;
        return;
      }
      Point aimPos = new Point((int)monster.GetCanvaPos.X, (int)monster.GetCanvaPos.Y);
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

    public void Save(BinaryWriter saveStream)
    {
      saveStream.Write(_aimID);//ID цели
      saveStream.Write(_damadge);//Урон
      saveStream.Write((int)_missleType);//Тип снаряда
      //Т.к. ToArgb impure метод
      saveStream.Write(_misslePenColor.R);
      saveStream.Write(_misslePenColor.G);
      saveStream.Write(_misslePenColor.B);
      //Т.к. ToArgb impure метод
      saveStream.Write(_missleBrushColor.R);
      saveStream.Write(_missleBrushColor.G);
      saveStream.Write(_missleBrushColor.B);
      saveStream.Write((int)_modificator);//Модификатор
      saveStream.Write(_position.X);//Позиция
      saveStream.Write(_position.Y);
      saveStream.Write(_progress);
    }
  }
}