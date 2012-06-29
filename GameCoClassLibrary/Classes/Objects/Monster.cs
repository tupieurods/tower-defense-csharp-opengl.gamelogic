using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Structures;
using GameCoClassLibrary.Interfaces;

namespace GameCoClassLibrary.Classes
{

  /// <summary>
  /// Monster class
  /// </summary>
  internal class Monster
  {
    #region Private Vars

    /// <summary>
    /// Basic monster params
    /// </summary>
    private readonly MonsterParam _params;
    /// <summary>
    /// Changed basic params
    /// </summary>
    private BaseMonsterParams _currentBaseParams;
    /// <summary>
    /// Way on the map
    /// </summary>
    private readonly List<Point> _way;
    /// <summary>
    /// Effects on monster
    /// </summary>
    private readonly List<AttackModificators> _effects;
    /// <summary>
    /// Map array position
    /// </summary>
    private Point _arrayPos;
    /// <summary>
    /// Position at the IGraphic
    /// </summary>
    private PointF _canvaPos;
    /// <summary>
    /// Way stage
    /// </summary>
    private int _wayPos;
    /// <summary>
    /// Moving phase(graphical moving visualization)
    /// </summary>
    private int _movingPhase;

    #endregion Private Vars

    #region Internal Vars

    /// <summary>
    /// If monster started new lap, decrease lives
    /// </summary>
    /// <value>
    ///   <c>true</c> if [new lap]; otherwise, <c>false</c>.
    /// </value>
    internal bool NewLap { get; set; }

    /// <summary>
    /// Gets the monster moving direction.
    /// </summary>
    internal MonsterDirection GetDirection { get; private set; }

    /// <summary>
    /// Gets the moster array position
    /// </summary>
    internal Point GetArrayPos
    {
      get
      {
        return new Point(_arrayPos.X, _arrayPos.Y);
      }
    }

    /// <summary>
    /// Gets the moster canvas position
    /// </summary>
    internal PointF GetCanvaPos
    {
      get
      {
        return new PointF(_canvaPos.X, _canvaPos.Y);
      }
    }

    /// <summary>
    /// Gets the ID.
    /// </summary>
    internal int ID { get; private set; }

    /// <summary>
    /// Remove monster from list or not
    /// </summary>
    /// <value>
    ///   <c>true</c> if hp==0; otherwise, <c>false</c>.
    /// </value>
    internal bool DestroyMe { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Monster"/> is visible.
    /// </summary>
    /// <value>
    ///   <c>true</c> if visible; otherwise, <c>false</c>.
    /// </value>
    internal bool Visible { get { return !_currentBaseParams.Invisible; } }

    #region internal static
    /// <summary>
    /// Cache.
    /// </summary>
    /// <value>
    /// The half sizes.
    /// </value>
    internal static int[] HalfSizes
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the scaling.
    /// </summary>
    /// <value>
    /// The scaling.
    /// </value>
    internal static float Scaling { get; set; }
    #endregion

    #endregion Internal Vars

    /// <summary>
    /// Initializes a new instance of the <see cref="Monster"/> class.
    /// </summary>
    /// <param name="Params">The params.</param>
    /// <param name="way">The way.</param>
    /// <param name="id">The id.</param>
    /// <param name="scaling">The scaling.</param>
    public Monster(MonsterParam Params, List<Point> way, int id = -1, float scaling = 1F)
    {
      _params = Params;
      _way = way;
      Scaling = scaling;
      ID = id;
      _currentBaseParams = Params.Base;
      //CurrentBaseParams.CanvasSpeed = 3F;//Debug, что бы не сидеть и не ждать когда же монстр добежит до финиша
      _effects = new List<AttackModificators>();
      NewLap = false;
      _arrayPos = new Point(way[0].X, way[0].Y);
      _wayPos = 0;
      DestroyMe = false;
      _movingPhase = 0;
      SetCanvaDirectionAndPosition(true);
    }

    /*/// <summary>
    /// Initializes static elements of  the <see cref="Monster"/> class.
    /// </summary>
    static Monster()
    {
      HalfSizes = new int[4];
    }*/

    /// <summary>
    /// Sets the canva direction and position.
    /// </summary>
    /// <param name="flag">if set to <c>false</c> sets only direction.</param>
    private void SetCanvaDirectionAndPosition(bool flag)
    {
      #region Direction Selection

      if (_wayPos != _way.Count - 1)
      {
        if (_way[_wayPos].X == _way[_wayPos + 1].X)//Move along Y
        {
          GetDirection = _way[_wayPos].Y < _way[_wayPos + 1].Y ? MonsterDirection.Down : MonsterDirection.Up;
        }
        else//Move along X
        {
          GetDirection = _way[_wayPos].X < _way[_wayPos + 1].X ? MonsterDirection.Right : MonsterDirection.Left;
        }
      }

      #endregion Direction Selection

      #region If need change postion

      if (flag)
        switch (GetDirection)
        {
          case MonsterDirection.Down:
            _canvaPos.Y = ((_arrayPos.Y - 1) * Settings.ElemSize);
            _canvaPos.X = (_arrayPos.X * Settings.ElemSize + Settings.ElemSize / 2);
            break;
          case MonsterDirection.Up:
            _canvaPos.Y = ((_arrayPos.Y + 1) * Settings.ElemSize);
            _canvaPos.X = (_arrayPos.X * Settings.ElemSize + Settings.ElemSize / 2);
            break;
          case MonsterDirection.Left:
            _canvaPos.X = ((_arrayPos.X + 1) * Settings.ElemSize);
            _canvaPos.Y = (_arrayPos.Y * Settings.ElemSize + Settings.ElemSize / 2);
            break;
          case MonsterDirection.Right:
            _canvaPos.X = ((_arrayPos.X - 1) * Settings.ElemSize);
            _canvaPos.Y = (_arrayPos.Y * Settings.ElemSize + Settings.ElemSize / 2);
            break;
        }

      #endregion If need change postion
    }

    /// <summary>
    /// Monster moving.
    /// </summary>
    /// <param name="flag">if set to <c>false</c> only moving phase changing.</param>
    public void Move(bool flag)
    {
      _currentBaseParams.Invisible = _params.Base.Invisible;
      if (!CanvasMove(flag)) return;
      _wayPos++;
      if (_wayPos == _way.Count - 1)
      {
        _wayPos = 0;
        NewLap = true;
      }
      _arrayPos = new Point(_way[_wayPos].X, _way[_wayPos].Y);//New array position
      if (_wayPos == 0)
        SetCanvaDirectionAndPosition(true);//direction and position
      else if (_wayPos != _way.Count)
        SetCanvaDirectionAndPosition(false);//direction only
    }

    /// <summary>
    /// Monster moving on canva
    /// </summary>
    /// <param name="flag">if set to <c>false</c> only moving phase changing.</param>
    /// <returns></returns>
    private bool CanvasMove(bool flag)
    {
      EffectsImpact();
      //Graphic moving phase calculation
      _movingPhase = (_movingPhase == (_params.NumberOfPhases - 1)) ? 0 : _movingPhase + 1;
      if (flag)
      {
        switch (GetDirection)
        {
          case MonsterDirection.Down:
            #region Moving down
            _canvaPos.Y += _currentBaseParams.CanvasSpeed;
            if (_wayPos == _way.Count - 2)//At the end of way
            {
              return _canvaPos.Y >= (_way[_way.Count - 1].Y * Settings.ElemSize + HalfSizes[0]);
            }
            if (_canvaPos.Y >= ((_way[_wayPos + 1].Y * Settings.ElemSize + Settings.ElemSize / 2)))
              return true;
            #endregion Движение вниз
            break;
          case MonsterDirection.Up:
            #region Moving up
            _canvaPos.Y -= _currentBaseParams.CanvasSpeed;
            if (_wayPos == _way.Count - 2)//At the end of way
            {
              return _canvaPos.Y <= (-HalfSizes[0]);
            }
            if ((_wayPos == _way.Count - 1) || (_canvaPos.Y <= ((_way[_wayPos + 1].Y * Settings.ElemSize + Settings.ElemSize / 2))))
              return true;
            #endregion Moving up
            break;
          case MonsterDirection.Left:
            #region Moving left
            _canvaPos.X -= _currentBaseParams.CanvasSpeed;
            if (_wayPos == _way.Count - 2)//At the end of way
            {
              return _canvaPos.X <= (-_params[MonsterDirection.Up, 0].Width / 2.0);
            }
            if (_canvaPos.X <= ((_way[_wayPos + 1].X * Settings.ElemSize + Settings.ElemSize / 2)))
              return true;
            #endregion Движение влево
            break;
          case MonsterDirection.Right:
            #region Moving right
            _canvaPos.X += _currentBaseParams.CanvasSpeed;
            if (_wayPos == _way.Count - 2)//At the end of way
            {
              if (_canvaPos.X >= (_way[_way.Count - 1].X * Settings.ElemSize + _params[MonsterDirection.Up, 0].Width / 2))
                return true;
              return false;
            }
            if (_canvaPos.X >= ((_way[_wayPos + 1].X * Settings.ElemSize + Settings.ElemSize / 2)))
              return true;
            #endregion Движение вправо
            break;
        }
      }

      return false;
    }

    /// <summary>
    /// Effects impact
    /// </summary>
    private void EffectsImpact()
    {
      _currentBaseParams.CanvasSpeed = _params.Base.CanvasSpeed;
      _currentBaseParams.Armor = _params.Base.Armor;
      foreach (var effect in _effects)
      {
        effect.DoEffect(ref _currentBaseParams.CanvasSpeed, ref _currentBaseParams.HealthPoints, ref _currentBaseParams.Armor);
      }
      _effects.RemoveAll(x => x.DestroyMe);
      if (_currentBaseParams.HealthPoints > 0) return;
      _currentBaseParams.HealthPoints = 0;
      DestroyMe = true;
    }


    /// <summary>
    /// Shows the monster.
    /// </summary>
    /// <param name="canva">The canva.</param>
    /// <param name="visibleStart">The visible map area start.</param>
    /// <param name="visibleFinish">The visible map area finish.</param>
    public void ShowMonster(IGraphic canva, Point visibleStart, Point visibleFinish)
    {
      if (_currentBaseParams.Invisible)
        return;
      if (!InVisibleMapArea(visibleStart, visibleFinish))
        return;
      //Unit picture
      Bitmap tmp = _params[GetDirection, _movingPhase];
      //Real coords calculating
      int realX = Settings.DeltaX + (int)(_canvaPos.X * Scaling - visibleStart.X * Settings.ElemSize * Scaling);
      int realY = Settings.DeltaY + (int)(_canvaPos.Y * Scaling - visibleStart.Y * Settings.ElemSize * Scaling);
      canva.DrawImage(tmp, (int)(realX - (tmp.Width / 2.0) * Scaling), (int)(realY - (tmp.Height / 2.0) * Scaling), (int)(tmp.Width * Scaling), (int)(tmp.Height * Scaling));
      #region Effect Colors
      int r = 0;
      int g = 0;
      int b = 0;
      foreach (var effect in _effects)//Effects visualization
      {
        r += effect.EffectColor.R;
        g += effect.EffectColor.G;
        b += effect.EffectColor.B;
      }
      if (r != 0 || g != 0 || b != 0)
        canva.FillEllipse(new SolidBrush(Color.FromArgb((byte)r, (byte)g, (byte)b)), realX - 5 * Scaling, realY - 5 * Scaling, 10 * Scaling, 10 * Scaling);
      #endregion Effect Colors

      //HP bar showing

      int hpLineLength = (int)(Math.Round(_currentBaseParams.HealthPoints * 100.0 / _params.Base.HealthPoints) / 10.0);

      if (hpLineLength < 0)
        hpLineLength = 0;
      switch (GetDirection)
      {
        case MonsterDirection.Left:
        case MonsterDirection.Right:
          canva.DrawLine(Helpers.BlackPen, realX - 5 * Scaling, realY, realX + 5 * Scaling, realY);
          if (hpLineLength == 0)
            break;
          canva.DrawLine(Helpers.GreenPen, realX - 5 * Scaling, realY, realX + (-5 + hpLineLength) * Scaling, realY);
          break;
        case MonsterDirection.Up:
        case MonsterDirection.Down:
          canva.DrawLine(Helpers.BlackPen, realX, realY + 5 * Scaling, realX, realY - 5 * Scaling);
          if (hpLineLength == 0)
            break;
          canva.DrawLine(Helpers.GreenPen, realX, realY - 5 * Scaling, realX, realY + (-5 + hpLineLength) * Scaling);
          break;
      }
    }

    /// <summary>
    /// Check, if monster in visible map area or not
    /// </summary>
    /// <param name="visibleStart">The visible map area start.</param>
    /// <param name="visibleFinish">The visible map area finish.</param>
    /// <returns></returns>
    private bool InVisibleMapArea(Point visibleStart, Point visibleFinish)
    {
      return (((int)(_canvaPos.Y + HalfSizes[(int)MonsterDirection.Up]) >= (visibleStart.Y * Settings.ElemSize)) ||
              ((int)(_canvaPos.Y - HalfSizes[(int)MonsterDirection.Down]) <= (visibleFinish.Y * Settings.ElemSize)))
             && (((int)(_canvaPos.X + HalfSizes[(int)MonsterDirection.Right]) >= (visibleStart.X * Settings.ElemSize)) ||
                 ((int)(_canvaPos.X - HalfSizes[(int)MonsterDirection.Left]) <= (visibleFinish.X * Settings.ElemSize)));
    }

    /// <summary>
    /// Damadge to monster
    /// </summary>
    /// <param name="damadge">The damadge.</param>
    /// <param name="modificator">The modificator.</param>
    /// <param name="reduceable">if set to <c>true</c> [reduceable].</param>
    public void GetDamadge(int damadge, eModificatorName modificator = eModificatorName.NoEffect, bool reduceable = true/*may be reduced by armor*/)
    {
      _currentBaseParams.HealthPoints -= reduceable ? damadge * (1 - _currentBaseParams.Armor / 100) : damadge;
      if (_currentBaseParams.HealthPoints > 0)//If unit alive
      {
        if (modificator != eModificatorName.NoEffect)//If missle with effect
        {
          bool find = false;
          foreach (var effect in _effects.Where(effect => effect.Type == modificator))
          {
            effect.Reset();
            find = true;
          }
          if (!find)
          {
            _effects.Add(AttackModificators.CreateEffectByID(modificator));
          }
        }
        return;
      }
      _currentBaseParams.HealthPoints = 0;
      DestroyMe = true;
    }

    /// <summary>
    /// Makes unit visible.
    /// </summary>
    public void MakeVisible()
    {
      _currentBaseParams.Invisible = false;
    }

    /// <summary>
    /// Saving monster to file
    /// </summary>
    /// <param name="saveStream">The save stream.</param>
    public void Save(BinaryWriter saveStream)
    {
      saveStream.Write(ID);
      // _currentBaseParams saving
      saveStream.Write(_currentBaseParams.HealthPoints);
      saveStream.Write(_currentBaseParams.CanvasSpeed);
      saveStream.Write(_currentBaseParams.Armor);
      saveStream.Write(_currentBaseParams.Invisible);
      saveStream.Write((int)GetDirection);//direction
      saveStream.Write(_arrayPos.X);
      saveStream.Write(_arrayPos.Y);//position in map array
      saveStream.Write(_canvaPos.X);
      saveStream.Write(_canvaPos.Y);//position on canva
      saveStream.Write(_wayPos);//Way stage
      saveStream.Write(_movingPhase);//Moving phase
      saveStream.Write(NewLap);
      saveStream.Write(_effects.Count(x => !x.DestroyMe));
      _effects.ForEach(x => x.Save(saveStream));
    }

    /// <summary>
    /// Loads monster from file
    /// </summary>
    /// <param name="loadStream">The load stream.</param>
    public void Load(BinaryReader loadStream)
    {
      ID = loadStream.ReadInt32();
      //_currentBaseParams
      _currentBaseParams.HealthPoints = loadStream.ReadInt32();
      _currentBaseParams.CanvasSpeed = loadStream.ReadInt32();
      _currentBaseParams.Armor = loadStream.ReadInt32();
      _currentBaseParams.Invisible = loadStream.ReadBoolean();
      GetDirection = (MonsterDirection)loadStream.ReadInt32();//direction
      _arrayPos = new Point(loadStream.ReadInt32(), loadStream.ReadInt32());//array position
      _canvaPos = new PointF(loadStream.ReadSingle(), loadStream.ReadSingle());//position on canva
      _wayPos = loadStream.ReadInt32();//way stage
      _movingPhase = loadStream.ReadInt32();//moving phase
      NewLap = loadStream.ReadBoolean();
      int n = loadStream.ReadInt32();
      for (int i = 0; i < n; i++)
      {
        _effects.Add(AttackModificators.CreateEffectByID((eModificatorName)loadStream.ReadInt32(), loadStream.ReadInt32()));
      }
    }
  }
}