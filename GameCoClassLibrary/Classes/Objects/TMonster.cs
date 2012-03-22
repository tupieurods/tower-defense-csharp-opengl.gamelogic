using System;
using System.Collections.Generic;
using System.Drawing;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Structures;

namespace GameCoClassLibrary.Classes
{
  internal class Monster
  {
    #region Private Vars

    private MonsterParam _params;//Параметры при создании
    private BaseMonsterParams _currentBaseParams;//Текущие базовые параметры
    private readonly List<Point> _way;//Путь
    private MonsterDirection _direction;//Направление
    private Point _arrayPos;//позиция в массиве карты
    private PointF _canvaPos;//позиция на экране
    private int _wayPos;//Позиция в списке пути
    private int _movingPhase;
    private Single _gameScale;

    #endregion Private Vars

    #region Public Vars

    //Начат ли новый круг(уменьшать ли жизни)
    public bool NewLap
    {
      get;
      set;
    }

    public MonsterDirection GetDirection
    {
      get
      {
        return _direction;
      }
    }

    public Point GetArrayPos
    {
      get
      {
        return new Point(_arrayPos.X, _arrayPos.Y);
      }
    }

    public PointF GetCanvaPos
    {
      get
      {
        return new PointF(_canvaPos.X, _canvaPos.Y);
      }
    }

    public float Scaling//О правильности масштабирования позаботится класс TGame
    {
      get
      {
        return _gameScale;
      }
      set
      {
        _gameScale = value;
        SetCanvaDirectionAndPosition(true);
      }
    }

    public int ID
    {
      get;
      private set;
    }

    public bool DestroyMe
    {
      get;
      private set;
    }

    #endregion Public Vars

    //Для оптимизации проверки на попадавиние в видимую область экрана
    internal static int[] HalfSizes
    {
      get;
      set;
    }

    //Constructor
    public Monster(MonsterParam Params, List<Point> way, int id, float scaling = 1F)
    {
      _params = Params;
      _way = way;
      Scaling = scaling;
      ID = id;
      _currentBaseParams = Params.Base;
      //CurrentBaseParams.CanvasSpeed = 3F;//Debug, что бы не сидеть и не ждать когда же монстр добежит до финиша
      NewLap = false;
      _arrayPos = new Point(way[0].X, way[0].Y);
      _wayPos = 0;
      DestroyMe = false;
      _movingPhase = 0;
      SetCanvaDirectionAndPosition(true);
    }

    //Устнавливаем позицию монстра только при создании и прохождении нового круга
    //Или же при смене разрешения в игровое время
    //На данный момент смена разрешения реализована лишь в теории, т.е код её поддерживает, но на практике никто это не проверял
    //Это задаток на будущее, если разработка будет продолжена
    private void SetCanvaDirectionAndPosition(bool flag)
    {
      #region Direction Selection

      if (_wayPos != _way.Count - 1)
      {
        if (_way[_wayPos].X == _way[_wayPos + 1].X)//движемся вдоль Y
        {
          _direction = _way[_wayPos].Y < _way[_wayPos + 1].Y ? MonsterDirection.Down : MonsterDirection.Up;
        }
        else//вдоль X
        {
          _direction = _way[_wayPos].X < _way[_wayPos + 1].X ? MonsterDirection.Right : MonsterDirection.Left;
        }
      }

      #endregion Direction Selection

      #region If need change postion

      if (flag)
        switch (_direction)//Позиции ставятся с упором на применение в начале круга или создании монстра
        //Если пользователь сменил разрешение во время уровня, то он сам дурак
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

    //перемещение монстра
    public void Move(bool flag)
    {
      if (CanvasMove(flag))//разрешили переместиться в массиве
      {
        _wayPos++;
        if (_wayPos == _way.Count - 1)
        {
          _wayPos = 0;
          NewLap = true;
        }
        _arrayPos = new Point(_way[_wayPos].X, _way[_wayPos].Y);//Новая точка
        if (_wayPos == 0)
          SetCanvaDirectionAndPosition(true);//Направление и позиция
        else if (_wayPos != _way.Count)
          SetCanvaDirectionAndPosition(false);//Только направлениеS
      }
    }

    //перемещение монстра по канве
    private bool CanvasMove(bool flag)
    {
      //Добавить здесь ещё воздействие эффектов
      _movingPhase = (_movingPhase == (_params.NumberOfPhases - 1)) ? 0 : _movingPhase + 1;
      if (flag)
      {
        switch (_direction)//тестировался нормальный уход за границу карты только при движении вверх
        {
          case MonsterDirection.Down:

            #region Движение вниз

            _canvaPos.Y += _currentBaseParams.CanvasSpeed;
            if (_wayPos == _way.Count - 2)//В конце пути
            {
              if (_canvaPos.Y >= (_way[_way.Count - 1].Y * Settings.ElemSize + _params[MonsterDirection.Up, 0].Height / 2))
                return true;
              return false;
            }
            if (_canvaPos.Y >= ((_way[_wayPos + 1].Y * Settings.ElemSize + Settings.ElemSize / 2)))
              return true;

            #endregion Движение вниз

            break;
          case MonsterDirection.Up:

            #region Движение вверх

            _canvaPos.Y -= _currentBaseParams.CanvasSpeed;
            if (_wayPos == _way.Count - 2)//В конце пути
            {
              // ReSharper disable PossibleLossOfFraction
              return _canvaPos.Y <= (-_params[MonsterDirection.Up, 0].Height / 2);
              // ReSharper restore PossibleLossOfFraction
            }
            if ((_wayPos == _way.Count - 1) || (_canvaPos.Y <= ((_way[_wayPos + 1].Y * Settings.ElemSize + Settings.ElemSize / 2))))
              return true;

            #endregion Движение вверх

            break;
          case MonsterDirection.Left:

            #region Движение влево

            _canvaPos.X -= _currentBaseParams.CanvasSpeed;
            if (_wayPos == _way.Count - 2)//В конце пути
            {
              // ReSharper disable PossibleLossOfFraction
              return _canvaPos.X <= (-_params[MonsterDirection.Up, 0].Width / 2);
              // ReSharper restore PossibleLossOfFraction
            }
            if (_canvaPos.X <= ((_way[_wayPos + 1].X * Settings.ElemSize + Settings.ElemSize / 2)))
              return true;

            #endregion Движение влево

            break;
          case MonsterDirection.Right:

            #region Движение вправо

            _canvaPos.X += _currentBaseParams.CanvasSpeed;
            if (_wayPos == _way.Count - 2)//В конце пути
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

    //отрисовка монстра на канве
    public void ShowMonster(Graphics canva, Point visibleStart, Point visibleFinish)
    {
      if (!InVisibleMapArea(visibleStart, visibleFinish))
        return;
      //Вывод самого юнита
      Bitmap tmp = _params[_direction, _movingPhase];
      //Высчитывание реальных координат отображения
      int realX = Settings.DeltaX + (int)(_canvaPos.X * Scaling - visibleStart.X * Settings.ElemSize);
      int realY = Settings.DeltaY + (int)(_canvaPos.Y * Scaling - visibleStart.Y * Settings.ElemSize);
      // ReSharper disable PossibleLossOfFraction
      canva.DrawImage(tmp, (int)(realX - (tmp.Width / 2) * Scaling), (int)(realY - (tmp.Height / 2) * Scaling), (int)(tmp.Width * Scaling), (int)(tmp.Height * Scaling));
      // ReSharper restore PossibleLossOfFraction

      #region Effect Colors(not implemented yet)

      /*If Length(FEffects)<>0 then//Визуальное воздействие эффектов
  begin
    FullColor:=ClBlack;
    For i:=0 to High(FEffects) do
    begin
      If FEffects[i].ClassName='TFreezeEffect' then
        FullColor:=FullColor+ClBlue
      else
      begin
        If FEffects[i].ClassName='TBurningEffect' then
          FullColor:=FullColor+ClMaroon
      end;
    end;
    GameCanv.Pen.Color:=FullColor;
    GameCanv.Brush.Color:=FullColor;
    GameCanv.Ellipse(FcanvX-(Image[MovingStages*GetDirection+MovingPhase].Width div 4),
            FCanvY-(Image[MovingStages*GetDirection+MovingPhase].Height div 4),
              FcanvX+(Image[MovingStages*GetDirection+MovingPhase].Width div 4),
                FCanvY+(Image[MovingStages*GetDirection+MovingPhase].Height div 4));
  end;*/

      #endregion Effect Colors(not implemented yet)

      //Вывод полоски жизней
      // ReSharper disable PossibleLossOfFraction
      int hpLineLength = (int)((Math.Round((double)((_currentBaseParams.HealthPoints * 100) / _params.Base.HealthPoints))) / 10);
      // ReSharper restore PossibleLossOfFraction
      if (hpLineLength < 0)
        hpLineLength = 0;
      switch (_direction)
      {
        case MonsterDirection.Left:
        case MonsterDirection.Right:
          canva.DrawLine(Helpers.BlackPen, realX - 5, realY, realX + 5, realY);
          if (hpLineLength == 0)
            break;
          canva.DrawLine(Helpers.GreenPen, realX - 5, realY, realX - 5 + hpLineLength, realY);
          break;
        case MonsterDirection.Up:
        case MonsterDirection.Down:
          canva.DrawLine(Helpers.BlackPen, realX, realY + 5, realX, realY - 5);
          if (hpLineLength == 0)
            break;
          canva.DrawLine(Helpers.GreenPen, realX, realY - 5, realX, realY - 5 + hpLineLength);
          break;
      }
    }

    private bool InVisibleMapArea(Point visibleStart, Point visibleFinish)
    {
      return (((int)(_canvaPos.Y + HalfSizes[(int)MonsterDirection.Up]) >= (visibleStart.Y * Settings.ElemSize)) ||
              ((int)(_canvaPos.Y - HalfSizes[(int)MonsterDirection.Down]) <= (visibleFinish.Y * Settings.ElemSize)))
             && (((int)(_canvaPos.X + HalfSizes[(int)MonsterDirection.Right]) >= (visibleStart.X * Settings.ElemSize)) ||
                 ((int)(_canvaPos.X - HalfSizes[(int)MonsterDirection.Left]) <= (visibleFinish.X * Settings.ElemSize)));
    }

    public void GetDamadge(int damadge, eModificatorName modificator = eModificatorName.NoEffect, bool reduceable = true/*может уменьшаться броней*/)
    {
      if (reduceable)
      {
        damadge = damadge * (1 - _currentBaseParams.Armor / 100);
      }
      _currentBaseParams.HealthPoints -= damadge;
      if (_currentBaseParams.HealthPoints > 0) return;
      _currentBaseParams.HealthPoints = 0;
      DestroyMe = true;
    }
  }
}