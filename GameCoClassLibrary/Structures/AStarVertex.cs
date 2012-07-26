using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GameCoClassLibrary.Structures
{
  internal class AStarVertexComparer : IComparer<AStarVertex>
  {
    /// <summary>
    /// Сравнивает два объекта и возвращает значение, показывающее, что один объект меньше или больше другого или равен ему.
    /// </summary>
    /// <returns>
    /// Знаковое целое число, которое определяет относительные значения <paramref name="x"/> и <paramref name="y"/>, как показано в следующей таблице.Значение Описание Меньше нуляЗначение параметра <paramref name="x"/> меньше значения параметра <paramref name="y"/>.НульЗначение параметра <paramref name="x"/> равно значению параметра <paramref name="y"/>.Больше нуляЗначение параметра <paramref name="x"/> больше, чем значение параметра <paramref name="y"/>.
    /// </returns>
    /// <param name="x">Первый сравниваемый объект.</param><param name="y">Второй сравниваемый объект.</param>
    public int Compare(AStarVertex x, AStarVertex y)
    {
      return x.F.CompareTo(y.F);
    }
  }

  internal struct AStarVertex
  {
    internal int F;
    internal int G;
    internal int H;
    internal int ID;
    internal int ParentID;
    internal Point Position;
  }

  internal struct AStarVertexFast
  {
    internal int G;
    internal int H;
    internal Point ParentPosition;
    internal int Status;//0-don't visited. 1-in open list. 2-in closed list
  }
}
