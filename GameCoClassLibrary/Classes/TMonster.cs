using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameCoClassLibrary
{
  class TMonster
  {
    #region Private Vars
    private MonsterParam Params;//Параметры при создании
    private BaseMonsterParams CurrentBaseParams;//Текущие базовые параметры
    private List<Point> Way;//Путь
    private MonsterDirection Direction;//Направление
    private Point ArrayPos;//позиция в массиве карты
    private FloatPoint CanvaPos;//позиция на экране
    private int WayPos;//Позиция в списке пути
    private int MovingPhase;
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
    public FloatPoint GetCanvaPos
    {
      get
      {
        return new FloatPoint(CanvaPos.X, CanvaPos.Y);
      }
    }
    public float Scaling//О правильности масштабирования позаботится класс TGame
    {
      get;
      set;
    }
    #endregion

    //Constructor
    public TMonster(MonsterParam Params, List<Point> Way, float Scaling = 1F)
    {
      this.Params = Params;
      this.Way = Way;
      this.Scaling = Scaling;
      CurrentBaseParams = Params.Base;
      NewLap = false;
      ArrayPos = new Point(Way[0].X, Way[0].Y);
      WayPos = 0;
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
      #endregion
      #region If need change postion
      if (Flag)
        switch (Direction)
        {
          case MonsterDirection.Down:
          case MonsterDirection.Up:
            CanvaPos.Y = (ArrayPos.Y * 15) * Scaling;
            CanvaPos.X = (ArrayPos.X * 15+8) * Scaling;
            break;
          case MonsterDirection.Left:
          case MonsterDirection.Right:
            CanvaPos.X = (ArrayPos.X * 15) * Scaling;
            CanvaPos.Y = (ArrayPos.Y * 15+8) * Scaling;
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
        if (WayPos == Way.Count-1)
        {
          WayPos = 0;
          NewLap = true;
        }
        ArrayPos = new Point(Way[WayPos].X, Way[WayPos].Y);//Новая точка
        if (WayPos == 0)
          SetCanvaDirectionAndPosition(true);//Направление и позиция
        else
          SetCanvaDirectionAndPosition(false);//Только направлениеS
      }
    }

    //перемещение монстра по канве
    private bool CanvasMove(bool Flag)
    {
      //Добавить здесь ещё воздействие эффектов
      bool Result = false;
      if (Flag)
      {
        switch (Direction)
        {
          case MonsterDirection.Down:
            CanvaPos.Y += CurrentBaseParams.CanvasSpeed;
            if (CanvaPos.Y >= ((Way[WayPos+1].Y * 15 + 8) * Scaling))
              Result = true;
            break;
          case MonsterDirection.Up:
            CanvaPos.Y -= CurrentBaseParams.CanvasSpeed;
            if (CanvaPos.Y <= ((Way[WayPos+1].Y * 15 + 8) * Scaling))
              Result = true;
            break;
          case MonsterDirection.Left:
            CanvaPos.X -= CurrentBaseParams.CanvasSpeed;
            if (CanvaPos.X <= ((Way[WayPos+1].X * 15 + 8) * Scaling))
              Result = true;
            break;
          case MonsterDirection.Right:
            CanvaPos.X += CurrentBaseParams.CanvasSpeed;
            if (CanvaPos.X >= ((Way[WayPos+1].X * 15 + 8) * Scaling))
              Result = true;
            break;
        }
      }
      MovingPhase = (MovingPhase == (Params.NumberOfPhases - 1)) ? 0 : MovingPhase + 1;
      return Result;
    }

    //отрисовка монстра на канве
    public void ShowMonster(Graphics Canva, int DX = 30, int DY = 30)
    {
      #region Delphi
      Bitmap Tmp = Params[Direction, MovingPhase];
      Canva.DrawImage(Tmp, (DX + (int)CanvaPos.X) - (Tmp.Width / 2), (DY + (int)CanvaPos.Y) - (Tmp.Height / 2), (int)(Tmp.Width * Scaling), (int)(Tmp.Height * Scaling));
      /*//Сам юнит
  GameCanv.Draw(FcanvX-(Image[MovingStages*GetDirection+MovingPhase].Width div 2),
    FCanvY-(Image[MovingStages*GetDirection+MovingPhase].Height div 2),Image[MovingStages*GetDirection+MovingPhase]);
  If Length(FEffects)<>0 then//Визуальное воздействие эффектов
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
  end;
  //Полоска жизней
  Tmp:=GetDirection;
  TmpWidth:=GameCanv.Pen.Width;
  GameCanv.Pen.Width:=3;
  //Вычисляем длину полоски жизней
  HpLineLength:=(Round((FCurrentHealth*100)/FMaxHealth)) Div 10;
  If HpLineLength<0 then
    HpLineLength:=0;
  If Tmp<=1 then//горизонтально
  Begin
    GameCanv.pen.Color:=ClBlack;
    GameCanv.MoveTo(FcanvX-5,FcanvY);
    GameCanv.LineTo(FcanvX+5,FcanvY);
    If HpLineLength=0 then
    begin
      GameCanv.Pen.Width:=TmpWidth;
      exit;
    end;
    GameCanv.pen.Color:=ClGreen;
    GameCanv.MoveTo(FcanvX-5,FcanvY);
    GameCanv.LineTo(FcanvX-5+HpLineLength,FcanvY);
  end
  else//Вертикально
  begin
    GameCanv.pen.Color:=ClBlack;
    GameCanv.MoveTo(FcanvX,FcanvY-5);
    GameCanv.LineTo(FcanvX,FcanvY+5);
    If HpLineLength=0 then
    begin
      GameCanv.Pen.Width:=TmpWidth;
      exit;
    end;
    GameCanv.pen.Color:=ClGreen;
    GameCanv.MoveTo(FcanvX,FcanvY-5);
    GameCanv.LineTo(FcanvX,FcanvY-5+HpLineLength);
  end;
  GameCanv.Pen.Width:=TmpWidth;*/
      #endregion
    }
  }
}
