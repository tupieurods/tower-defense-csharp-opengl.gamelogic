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
    private readonly TowerParam Params;//Параметры, получаемые от игры
    private sMainTowerParam CurrentTowerParams;//Текущие параметры вышки
    private Point ArrayPos;//Позиция на карте(левая верхняя клетка башни на карте)

    public TTower(TowerParam Params,Point ArrayPos)
    {
      this.Params = Params;
      this.ArrayPos=new Point(ArrayPos.X,ArrayPos.Y);
      CurrentTowerParams = this.Params.UpgradeParams[0];
      CurrentTowerParams.Picture.MakeTransparent();
    }

    public void ShowTower(Graphics Canva, int DX=10, int DY=10)
    {
      Canva.DrawImage(CurrentTowerParams.Picture, DX - (CurrentTowerParams.Picture.Width/2)+ ((ArrayPos.X+1) * 15),
        DY - (CurrentTowerParams.Picture.Height / 2) + ((ArrayPos.Y+1) * 15), CurrentTowerParams.Picture.Width, CurrentTowerParams.Picture.Height);
    }
  }
}
