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

    public TMonster(MonsterParam Params, List<Point> Way)
    {
      this.Params = Params;
      this.Way = Way;
      CurrentBaseParams = Params.Base;
      NewLap = false;
      ArrayPos = new Point(Way[0].X, Way[0].Y);
      WayPos = 0;
      SetCanvaPosition();
    }

    //Устнавливаем позицию монстра только при создании и прохождении нового круга
    //Или же при смене разрешения в игровое время
    //На данный момент смена разрешения реализована лишь в теории, т.е код её поддерживает, но на практике никто это не проверял
    //Это задаток на будущее, если разработка будет продолжена
    private void SetCanvaPosition()
    {
      if (WayPos != (Way.Count - 1))//Если не у конца пути
      {
        #region Постановка монстра
        if (Way[WayPos].X == Way[WayPos+1].X)//движемся вдоль X
        {
          CanvaPos.X = (ArrayPos.X * 15 + 8)*Scaling;
          CanvaPos.Y = (ArrayPos.Y * 15) * Scaling;
          return;
        }
        if (Way[WayPos].Y == Way[WayPos+1].Y)//вдоль Y
        {
          CanvaPos.Y = (ArrayPos.Y * 15 + 8) * Scaling;
          CanvaPos.X = (ArrayPos.X * 15)*Scaling;
          return;
        }
        #endregion
      }
      else//если у конца пути
      {
        #region Постановка монстра
        if (Way[WayPos - 1].X == Way[WayPos - 2].X)//движемся вдоль X
        {
          CanvaPos.X = (ArrayPos.X * 15 + 8)*Scaling;
          CanvaPos.Y = (ArrayPos.Y * 15) * Scaling;
          return;
        }
        if (Way[WayPos - 1].Y == Way[WayPos - 2].Y)//вдоль Y
        {
          CanvaPos.Y = (ArrayPos.Y * 15 + 8) * Scaling;
          CanvaPos.X = (ArrayPos.X * 15) * Scaling;
          return;
        }
        #endregion
      }
    }
  }
}
