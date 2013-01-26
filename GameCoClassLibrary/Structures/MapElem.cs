using System;
using GameCoClassLibrary.Enums;

namespace GameCoClassLibrary.Structures
{
  /// <summary>
  /// Map element description
  /// </summary>
  [Serializable]
  public struct MapElem
  {
    /// <summary>
    /// Number of picture to drawing
    /// </summary>
    public int PictNumber; //Картинка для рисования

    /// <summary>
    /// Angle of picture rotation
    /// </summary>
    public int AngleOfRotate; //и угол её поворота

    /// <summary>
    /// Status of map element
    /// </summary>
    public MapElemStatus Status;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapElem"/> struct.
    /// </summary>
    /// <param name="pictNumber">The pict number.</param>
    /// <param name="angleOfRotate">The angle of rotate.</param>
    /// <param name="status">The status.</param>
    public MapElem(int pictNumber, int angleOfRotate, MapElemStatus status)
    {
      PictNumber = pictNumber;
      AngleOfRotate = angleOfRotate;
      Status = status;
    }
  }
}