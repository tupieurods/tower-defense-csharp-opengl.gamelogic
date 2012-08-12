using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using GameCoClassLibrary.Enums;
using GraphicLib.Interfaces;

namespace GameCoClassLibrary.Classes
{
  /// <summary>
  /// Missle Class
  /// </summary>
  internal class Missle
  {
    #region Private

    /// <summary>
    /// Missle damadge
    /// </summary>
    private readonly int _damadge;
    /// <summary>
    /// Monster ID
    /// </summary>
    private readonly int _aimID;
    /// <summary>
    /// Missle type
    /// </summary>
    private readonly eTowerType _missleType;
    /// <summary>
    /// Missle pen color
    /// </summary>
    private readonly Color _misslePenColor;
    /// <summary>
    /// Missle brush color
    /// </summary>
    private readonly Color _missleBrushColor;
    /// <summary>
    /// Attack modificator
    /// </summary>
    private readonly eModificatorName _modificator;
    /// <summary>
    /// Position at canva
    /// </summary>
    private PointF _position;
    /// <summary>
    /// Moving progress
    /// </summary>
    private int _progress;

    #endregion Private

    #region Internal

    /// <summary>
    /// Indicates, should Game class remove this missle from missles list or not
    /// </summary>
    internal bool DestroyMe//обозначает что нужно удалить из списка снарядов
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets or sets the scaling.
    /// </summary>
    /// <value>
    /// The scaling.
    /// </value>
    static internal float Scaling
    {
      get;
      set;
    }

    #endregion Internal

    /// <summary>
    /// Prevents a default instance of the <see cref="Missle"/> class from being created.
    /// </summary>
    /// <param name="aimID">The aim ID.</param>
    /// <param name="damadge">The damadge.</param>
    /// <param name="missleType">Type of the missle.</param>
    /// <param name="misslePenColor">Color of the missle pen.</param>
    /// <param name="missleBrushColor">Color of the missle brush.</param>
    /// <param name="modificator">The modificator.</param>
    /// <param name="position">The position.</param>
    /// <param name="progress">The progress.</param>
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

    /// <summary>
    /// Factories the specified act.
    /// </summary>
    /// <param name="act">The act.</param>
    /// <param name="listOfParams">The list of params.</param>
    /// <returns>Missle object</returns>
    internal static Missle Factory(FactoryAct act, params object[] listOfParams)
    {
      try
      {
        Missle result;
        switch (act)
        {
          case FactoryAct.Create:
            result = new Missle((int)listOfParams[0], (int)listOfParams[1], (eTowerType)listOfParams[2], (Color)listOfParams[3], (Color)listOfParams[4],
              (eModificatorName)listOfParams[5], new PointF(Convert.ToSingle(listOfParams[6]), Convert.ToSingle(listOfParams[7])) /*, (int)listOfParams[8]*//*Progress*/);
            break;
          case FactoryAct.Load:
            BinaryReader reader = (BinaryReader)listOfParams[0];
            // ReSharper disable RedundantArgumentName
            result = new Missle(aimID: reader.ReadInt32(), damadge: reader.ReadInt32(), missleType: (eTowerType)reader.ReadInt32(), misslePenColor: Color.FromArgb(reader.ReadByte(),
              reader.ReadByte(), reader.ReadByte()), missleBrushColor: Color.FromArgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
              modificator: (eModificatorName)reader.ReadInt32(), position: new PointF(reader.ReadSingle(), reader.ReadSingle()), progress: reader.ReadInt32());
            // ReSharper restore RedundantArgumentName
            break;
          default:
            throw new ArgumentOutOfRangeException("act");
        }
        return result;
      }
      catch (Exception exc)
      {
        //TODO add NLog logging
        throw;
      }
    }

    /// <summary>
    /// Missle moving.
    /// </summary>
    /// <param name="monsters">Monsters list</param>
    internal void Move(IEnumerable<Monster> monsters)
    {
      var monstersList = monsters.ToList();//Multiple Enumeration of IEnumerable fixing
      //Getting Monster
      Monster aim = monstersList.FirstOrDefault(elem => elem.ID == _aimID);
      if (aim == null)
      {
        DestroyMe = true;
        return;
      }
      #region Missle canvas moving
      //Missle dx and dx matching
      int dx = (int)Math.Abs((aim.GetCanvaPos.X - _position.X) / _progress);
      int dy = (int)Math.Abs((aim.GetCanvaPos.Y - _position.Y) / _progress);
      //Check monster and missle coords.
      //X:
      if (_position.X > aim.GetCanvaPos.X)
        _position.X -= dx;
      else
        _position.X += dx;
      //Y:
      if (_position.Y > aim.GetCanvaPos.Y)
        _position.Y -= dy;
      else
        _position.Y += dy;
      #endregion
      //Decrease number of moving phases
      _progress--;
      if (_progress != 0)//No contact with aim
        return;
      #region Damadge if contact
      {
        DestroyMe = true;
        aim.GetDamadge(_damadge, _modificator);
        switch (_missleType)
        {
          case eTowerType.Splash:
            var splashedAims = from monster in monstersList
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

    /// <summary>
    /// Shows the missle on canva
    /// </summary>
    /// <param name="canva">The canva.</param>
    /// <param name="visibleStart">The visible map area start.</param>
    /// <param name="visibleFinish">The visible map area finish.</param>
    /// <param name="monsters">The monsters.</param>
    internal void Show(IGraphic canva, Point visibleStart, Point visibleFinish, IEnumerable<Monster> monsters)
    {
      if (DestroyMe
        || ((_position.X - visibleStart.X * Settings.ElemSize < 5)
        || (_position.Y - visibleStart.Y * Settings.ElemSize < 5)
        || (-_position.X + visibleFinish.X * Settings.ElemSize < 5)
        || (-_position.Y + visibleFinish.Y * Settings.ElemSize < 5))
        || (monsters == null))
        return;
      //Getting Monster
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
          Point secondPosition;//Position of second missle point
          #region Second point calculation
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
          #endregion
          canva.DrawLine(new Pen(_misslePenColor, 2),
                         new Point((int)((_position.X + Settings.DeltaX - visibleStart.X * Settings.ElemSize) * Scaling),
                                   (int)((_position.Y + Settings.DeltaY - visibleStart.Y * Settings.ElemSize) * Scaling)),
                         new Point((int)((secondPosition.X + Settings.DeltaX - visibleStart.X * Settings.ElemSize) * Scaling),
                                   (int)((secondPosition.Y + Settings.DeltaY - visibleStart.Y * Settings.ElemSize) * Scaling)));
          break;
        case eTowerType.Splash:
          canva.FillEllipse(new SolidBrush(_missleBrushColor),
                            (int)(_position.X + Settings.DeltaX - 5 - visibleStart.X * Settings.ElemSize) * Scaling,
                            (int)(_position.Y + Settings.DeltaY - 5 - visibleStart.Y * Settings.ElemSize) * Scaling,
                            10 * Scaling, 10 * Scaling);
          canva.DrawEllipse(new Pen(_misslePenColor),
                            (int)(_position.X + Settings.DeltaX - 5 - visibleStart.X * Settings.ElemSize) * Scaling,
                            (int)(_position.Y + Settings.DeltaY - 5 - visibleStart.Y * Settings.ElemSize) * Scaling,
                            10 * Scaling, 10 * Scaling);
          break;
      }
    }

    /// <summary>
    /// Missle saving
    /// </summary>
    /// <param name="saveStream">The save stream.</param>
    internal void Save(BinaryWriter saveStream)
    {
      saveStream.Write(_aimID);//monster id
      saveStream.Write(_damadge);//damadge
      saveStream.Write((int)_missleType);//Missle type
      //Because ToArgb impure method
      saveStream.Write(_misslePenColor.R);
      saveStream.Write(_misslePenColor.G);
      saveStream.Write(_misslePenColor.B);
      //Because ToArgb impure method
      saveStream.Write(_missleBrushColor.R);
      saveStream.Write(_missleBrushColor.G);
      saveStream.Write(_missleBrushColor.B);
      saveStream.Write((int)_modificator);//modificator
      saveStream.Write(_position.X);//Position
      saveStream.Write(_position.Y);
      saveStream.Write(_progress);//Progress
    }
  }
}