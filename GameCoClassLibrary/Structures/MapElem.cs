using System;
using GameCoClassLibrary.Enums;

namespace GameCoClassLibrary.Structures
{
  [Serializable]
  public struct MapElem
  {
    public int PictNumber;//Картинка для рисования
    public int AngleOfRotate;//и угол её поворота
    public MapElemStatus Status;
    public MapElem(int PictNumber, int AngleOfRotate, MapElemStatus Status)
    {
      this.PictNumber = PictNumber;
      this.AngleOfRotate = AngleOfRotate;
      this.Status = Status;
    }
  }
}
