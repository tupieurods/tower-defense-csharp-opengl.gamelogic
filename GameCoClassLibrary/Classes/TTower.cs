using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GameCoClassLibrary;

namespace GameCoClassLibrary
{
  class TTower
  {
    #region Private
    private readonly TowerParam Params;//Параметры, получаемые от игры
    private sMainTowerParam CurrentTowerParams;//Текущие параметры вышки
    private Point ArrayPos;//Позиция на карте(левая верхняя клетка башни на карте)
    private Bitmap ScaledTowerPict;//Хранит перемасштабированное изображение башни на карте
    #endregion

    #region Public
    public float Scaling//О правильности масштабирования позаботится класс TGame
    {
      get;
      set;
    }
    #endregion

    public TTower(TowerParam Params, Point ArrayPos,float Scaling = 1F)
    {
      this.Params = Params;
      this.ArrayPos = new Point(ArrayPos.X, ArrayPos.Y);
      this.Scaling = Scaling;
      CurrentTowerParams = this.Params.UpgradeParams[0];
      CurrentTowerParams.Picture.MakeTransparent();
    }

    public void ShowTower(Graphics Canva, Point VisibleStart, Point VisibleFinish, int DX = 10, int DY = 10)
    {
      //Проверка, видима ли вышка
      bool Flag = true;
      //if ((ArrayPos.Y >= VisibleFinish.Y) || (ArrayPos.X >= VisibleFinish.X))
        if (!(((ArrayPos.X + 1) * 15 - CurrentTowerParams.AttackRadius < VisibleFinish.X * 15) ||
          ((ArrayPos.Y + 1) * 15 - CurrentTowerParams.AttackRadius < VisibleFinish.Y * 15)))//Если не видна логически, но видна графически
        Flag = false;
      //if ((Flag)&&((ArrayPos.X < (VisibleStart.X-1)) || (ArrayPos.Y < (VisibleStart.Y-1))))
        if ((Flag)&&(!(((ArrayPos.X + 1) * 15 + CurrentTowerParams.AttackRadius > VisibleStart.X * 15) || 
          ((ArrayPos.Y + 1) * 15 + CurrentTowerParams.AttackRadius > VisibleStart.Y * 15))))
        Flag = false;
      if (Flag)
        Canva.DrawImage(CurrentTowerParams.Picture, DX - (CurrentTowerParams.Picture.Width / 2) + ((ArrayPos.X + 1-VisibleStart.X) * 15),
          DY - (CurrentTowerParams.Picture.Height / 2) + ((ArrayPos.Y + 1-VisibleStart.Y) * 15), 
          CurrentTowerParams.Picture.Width*Scaling, CurrentTowerParams.Picture.Height*Scaling);
    }
  }
}
