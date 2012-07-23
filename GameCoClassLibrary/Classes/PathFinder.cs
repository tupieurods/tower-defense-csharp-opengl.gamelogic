using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Structures;

namespace GameCoClassLibrary.Classes
{
  class PathFinder
  {

    private static int GetDistance(Point from, Point to)
    {
      return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
    }

    internal static List<Point> AStar(MapElem[,] field, Point startPos, Point endPos, Point size)
    {
      List<Point> result = null;
      //До тех пока не возникнет необходимости окружать карту снаружи непроходимыми клетками будет эта проверка
      Func<int, int, bool> InRange = (int value, int top) => (value >= 0) && (value < top);
      var comparer = new AStarVertexComparer();
      List<AStarVertex> OpenList = new List<AStarVertex>();
      List<AStarVertex> ClosedList = new List<AStarVertex>();
      bool found = false;
      Dictionary<MonsterDirection, Point> actions = new Dictionary<MonsterDirection, Point>();
      actions[MonsterDirection.Up] = new Point(0, -1);
      actions[MonsterDirection.Right] = new Point(1, 0);
      actions[MonsterDirection.Down] = new Point(0, 1);
      actions[MonsterDirection.Left] = new Point(-1, 0);
      int id = 0;
      Action<int, int, int, Point> addToOpenList =
        (int g, int h, int parentID, Point position) =>
        {
          OpenList.Add(new AStarVertex { F = g + h, G = g, H = h, ID = id, ParentID = parentID, Position = position });
          id++;
        };
      addToOpenList(0, 0, -1, startPos);
      //Цикл поиска пути
      while (true)
      {
        if (OpenList.Count == 0)//Невозможно найти путь
          break;
        if (OpenList.Any(x => x.Position.X == endPos.X && x.Position.Y == endPos.Y))//Если путь найден
        {
          found = true;
          break;
        }
        OpenList.Sort(comparer);
        AStarVertex current = OpenList[0];
        OpenList.Remove(current);
        ClosedList.Add(current);
        for (MonsterDirection i = MonsterDirection.Up; i <= MonsterDirection.Left; i++)
        {
          Point tmp = new Point(current.Position.X + actions[i].X, current.Position.Y + actions[i].Y);
          if (!InRange(tmp.X, size.X)
            || !InRange(tmp.Y, size.Y)
            || field[tmp.Y, tmp.X].Status != MapElemStatus.CanMove
            || ClosedList.Any(value => value.Position.X == tmp.X && value.Position.Y == tmp.Y))
            continue;
          int openPos = OpenList.FindIndex(value => value.Position.X == tmp.X && value.Position.Y == tmp.Y);
          if (openPos == -1)//Ещё не в открытом списке
          {
            int g = 10 + current.G;//Если можно ходить в диагональные точки, то сделать проверку +10 или +14
            int h = GetDistance(tmp, endPos) * 10;
            addToOpenList(g, h, current.ID, tmp);
          }
          else
          {
            if (OpenList[openPos].G > current.G + 10)//Если можно ходить в диагональные точки, то сделать проверку +10 или +14
            {
              var tmpVertex = OpenList[openPos];
              tmpVertex.F -= tmpVertex.G;
              tmpVertex.G = current.G + 10;
              tmpVertex.F += tmpVertex.G;
            }
          }
        }
      }
      if (found)
      {
        result = new List<Point>();
        var currentElem = OpenList.Find(value => value.Position == endPos);
        do
        {
          result.Add(currentElem.Position);
          currentElem = ClosedList.Find(value => value.ID == currentElem.ParentID);
        } while (currentElem.ParentID != -1);
        result.Add(startPos);
        result.Reverse();
      }
      return result;
    }
  }
}
