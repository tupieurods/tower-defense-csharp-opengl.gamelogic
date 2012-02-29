using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameCoClassLibrary
{
  class TMonster
  {
    //Delegats
    delegate bool Check();

    #region Private Vars
    private MonsterParam Params;//Параметры при создании
    private BaseMonsterParams CurrentBaseParams;//Текущие базовые параметры
    private List<Point> Way;//Путь
    private MonsterDirection Direction;//Направление
    private Point ArrayPos;//позиция в массиве карты
    private PointF CanvaPos;//позиция на экране
    private int WayPos;//Позиция в списке пути
    private int MovingPhase;
    private Single GameScale;
    #endregion

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
        return Direction;
      }
    }
    public Point GetArrayPos
    {
      get
      {
        return new Point(ArrayPos.X, ArrayPos.Y);
      }
    }
    public PointF GetCanvaPos
    {
      get
      {
        return new PointF(CanvaPos.X, CanvaPos.Y);
      }
    }
    public float Scaling//О правильности масштабирования позаботится класс TGame
    {
      get
      {
        return GameScale;
      }
      set
      {
        GameScale = value;
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
    #endregion

    //Constructor
    public TMonster(MonsterParam Params, List<Point> Way, int ID, float Scaling = 1F)
    {
      this.Params = Params;
      this.Way = Way;
      this.Scaling = Scaling;
      this.ID = ID;
      CurrentBaseParams = Params.Base;
      //CurrentBaseParams.CanvasSpeed = 3F;//Debug, что бы не сидеть и не ждать когда же монстр добежит до финиша
      NewLap = false;
      ArrayPos = new Point(Way[0].X, Way[0].Y);
      WayPos = 0;
      DestroyMe = false;
      MovingPhase = 0;
      SetCanvaDirectionAndPosition(true);
    }

    //Устнавливаем позицию монстра только при создании и прохождении нового круга
    //Или же при смене разрешения в игровое время
    //На данный момент смена разрешения реализована лишь в теории, т.е код её поддерживает, но на практике никто это не проверял
    //Это задаток на будущее, если разработка будет продолжена
    private void SetCanvaDirectionAndPosition(bool Flag)
    {
      #region Direction Selection
      if (WayPos != Way.Count - 1)
      {
        if (Way[WayPos].X == Way[WayPos + 1].X)//движемся вдоль Y
        {
          if (Way[WayPos].Y < Way[WayPos + 1].Y)
            Direction = MonsterDirection.Down;
          else
            Direction = MonsterDirection.Up;
        }
        else//вдоль X
        {
          if (Way[WayPos].X < Way[WayPos + 1].X)
            Direction = MonsterDirection.Right;
          else
            Direction = MonsterDirection.Left;
        }
      }
      #endregion
      #region If need change postion
      if (Flag)
        switch (Direction)//Позиции ставятся с упором на применение в начале круга или создании монстра
        //Если пользователь сменил разрешение во время уровня, то он сам дурак
        {
          case MonsterDirection.Down:
            CanvaPos.Y = ((ArrayPos.Y - 1) * 15);
            CanvaPos.X = (ArrayPos.X * 15 + 8);
            break;
          case MonsterDirection.Up:
            CanvaPos.Y = ((ArrayPos.Y + 1) * 15);
            CanvaPos.X = (ArrayPos.X * 15 + 8);
            break;
          case MonsterDirection.Left:
            CanvaPos.X = ((ArrayPos.X + 1) * 15);
            CanvaPos.Y = (ArrayPos.Y * 15 + 8);
            break;
          case MonsterDirection.Right:
            CanvaPos.X = ((ArrayPos.X - 1) * 15);
            CanvaPos.Y = (ArrayPos.Y * 15 + 8);
            break;
        }
      #endregion
    }

    //перемещение монстра
    public void Move(bool Flag)
    {
      if (CanvasMove(Flag))//разрешили переместиться в массиве
      {
        WayPos++;
        if (WayPos == Way.Count - 1)
        {
          WayPos = 0;
          NewLap = true;
        }
        ArrayPos = new Point(Way[WayPos].X, Way[WayPos].Y);//Новая точка
        if (WayPos == 0)
          SetCanvaDirectionAndPosition(true);//Направление и позиция
        else if (WayPos != Way.Count)
          SetCanvaDirectionAndPosition(false);//Только направлениеS
      }
    }

    //перемещение монстра по канве
    private bool CanvasMove(bool Flag)
    {
      //Добавить здесь ещё воздействие эффектов
      MovingPhase = (MovingPhase == (Params.NumberOfPhases - 1)) ? 0 : MovingPhase + 1;
      if (Flag)
      {
        switch (Direction)//тестировался нормальный уход за границу карты только при движении вверх
        {
          case MonsterDirection.Down:
            #region Движение вниз
            CanvaPos.Y += CurrentBaseParams.CanvasSpeed;
            if (WayPos == Way.Count - 2)//В конце пути
            {
              if (CanvaPos.Y >= (Way[Way.Count - 1].Y * 15 + Params[MonsterDirection.Up, 0].Height / 2))
                return true;
              else
                return false;
            }
            else if (CanvaPos.Y >= ((Way[WayPos + 1].Y * 15 + 8)))
              return true;
            #endregion
            break;
          case MonsterDirection.Up:
            #region Движение вверх
            CanvaPos.Y -= CurrentBaseParams.CanvasSpeed;
            if (WayPos == Way.Count - 2)//В конце пути
            {
              if (CanvaPos.Y <= (-Params[MonsterDirection.Up, 0].Height / 2))
                return true;
              else
                return false;
            }
            else if ((WayPos == Way.Count - 1) || (CanvaPos.Y <= ((Way[WayPos + 1].Y * 15 + 8))))
              return true;
            #endregion
            break;
          case MonsterDirection.Left:
            #region Движение влево
            CanvaPos.X -= CurrentBaseParams.CanvasSpeed;
            if (WayPos == Way.Count - 2)//В конце пути
            {
              if (CanvaPos.X <= (-Params[MonsterDirection.Up, 0].Width / 2))
                return true;
              else
                return false;
            }
            else if (CanvaPos.X <= ((Way[WayPos + 1].X * 15 + 8)))
              return true;
            #endregion
            break;
          case MonsterDirection.Right:
            #region Движение вправо
            CanvaPos.X += CurrentBaseParams.CanvasSpeed;
            if (WayPos == Way.Count - 2)//В конце пути
            {
              if (CanvaPos.X >= (Way[Way.Count - 1].X * 15 + Params[MonsterDirection.Up, 0].Width / 2))
                return true;
              else
                return false;
            }
            if (CanvaPos.X >= ((Way[WayPos + 1].X * 15 + 8)))
              return true;
            #endregion
            break;
        }
      }

      return false;
    }

    //отрисовка монстра на канве
    public void ShowMonster(Graphics Canva, Point VisibleStart, Point VisibleFinish, int DX = 10, int DY = 10)
    {
      if (!InVisibleMapArea(VisibleStart, VisibleFinish))
        return;
      //Вывод самого юнита
      Bitmap Tmp = Params[Direction, MovingPhase];
      //Высчитывание реальных координат отображения
      int RealX = DX + (int)(CanvaPos.X*Scaling - VisibleStart.X * 15);
      int RealY = DY + (int)(CanvaPos.Y*Scaling - VisibleStart.Y * 15);
      Canva.DrawImage(Tmp, (int)(RealX - (Tmp.Width / 2) * Scaling), (int)(RealY - (Tmp.Height / 2) * Scaling), (int)(Tmp.Width * Scaling), (int)(Tmp.Height * Scaling));
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
      #endregion
      //Вывод полоски жизней
      int HpLineLength = (int)((Math.Round((double)((CurrentBaseParams.HealthPoints * 100) / Params.Base.HealthPoints))) / 10);
      if (HpLineLength < 0)
        HpLineLength = 0;
      int PenWidth = 3;
      switch (Direction)
      {
        case MonsterDirection.Left:
        case MonsterDirection.Right:
          Canva.DrawLine(new Pen(Color.Black, PenWidth), RealX - 5, RealY, RealX + 5, RealY);
          if (HpLineLength == 0)
            break;
          else
          {
            Canva.DrawLine(new Pen(Color.Green, PenWidth), RealX - 5, RealY, RealX - 5 + HpLineLength, RealY);
          }
          break;
        case MonsterDirection.Up:
        case MonsterDirection.Down:
          Canva.DrawLine(new Pen(Color.Black, PenWidth), RealX, RealY + 5, RealX, RealY - 5);
          if (HpLineLength == 0)
            break;
          else
          {
            Canva.DrawLine(new Pen(Color.Green, PenWidth), RealX, RealY - 5, RealX, RealY - 5 + HpLineLength);
          }
          break;
      }
    }

    private bool InVisibleMapArea(Point VisibleStart, Point VisibleFinish)
    {
      Check CheckHorizontal = delegate
      {
        /*if ((ArrayPos.X >= VisibleStart.X) && ((ArrayPos.X < VisibleFinish.X)))//Если 100% видно по горизонтали
          return true;
        else//Проверим, а вдруг видно какой-нибудь кусочек справа или слева(моделька настолько большая что выпирает)
        {*/
        if (((int)(CanvaPos.X + Params[MonsterDirection.Down, 0].Width / 2) >= (VisibleStart.X * 15)) ||
          ((int)(CanvaPos.X - Params[MonsterDirection.Down, 0].Width / 2) <= (VisibleFinish.X * 15)))
          return true;
        else
          return false;
        //}
      };
      Check CheckVertical = delegate
      {
        /*if ((ArrayPos.Y >= VisibleStart.Y) && ((ArrayPos.Y < VisibleFinish.Y)))//Если 100% видно по вертикали
          return true;
        else//Проверим, а вдруг видно какой-нибудь кусочек сверху или снизу(моделька настолько большая что выпирает)
        {*/
        if (((int)(CanvaPos.Y + Params[MonsterDirection.Down, 0].Height / 2) >= (VisibleStart.Y * 15)) ||
          ((int)(CanvaPos.Y - Params[MonsterDirection.Down, 0].Height / 2) <= (VisibleFinish.Y * 15)))
          return true;
        else
          return false;
        //}
      };
      switch (Direction)
      {
        case MonsterDirection.Up:
          if (((int)(CanvaPos.Y - Params[MonsterDirection.Down, 0].Height / 2)) <= (VisibleFinish.Y * 15))//если "видно" по вертикали
          {
            return CheckHorizontal();
          }
          break;
        case MonsterDirection.Down:
          if (((int)(CanvaPos.Y + Params[MonsterDirection.Down, 0].Height / 2)) >= (VisibleStart.Y * 15))//если "видно" по вертикали
          {
            return CheckHorizontal();
          }
          break;
        case MonsterDirection.Right:
          if (((int)(CanvaPos.X + Params[MonsterDirection.Down, 0].Width / 2)) >= (VisibleStart.X * 15))//если "видно" по горизонтали
          {
            return CheckVertical();
          }
          break;
        case MonsterDirection.Left:
          if (((int)(CanvaPos.X + Params[MonsterDirection.Down, 0].Width / 2)) >= (VisibleStart.X * 15))//если "видно" по горизонтали
          {
            return CheckVertical();
          }
          break;
      }
      return false;
    }

    public void GetDamadge(int Damadge, eModificatorName Modificator = eModificatorName.NoEffect, bool Reduceable = true/*может уменьшаться броней*/)
    {
      if (Reduceable)
      {
        Damadge = Damadge * (1 - CurrentBaseParams.Armor / 100);
      }
      CurrentBaseParams.HealthPoints -= Damadge;
      if (CurrentBaseParams.HealthPoints <= 0)
      {
        CurrentBaseParams.HealthPoints = 0;
        DestroyMe = true;
      }
    }

  }
}
