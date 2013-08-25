using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Structures;
using GameCoClassLibrary.Third_Party.PriorityQueue;

namespace GameCoClassLibrary.Classes
{
  internal static class PathFinder
  {
    private static int GetDistance(Point from, Point to)
    {
      return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
    }

    private static int Detector(Point first, Point second)
    {
      return Math.Abs(first.X - second.X) != 0 ? 5 : 0;
    }

    [Obsolete("Learning version. Very slow. Use AStarFast")]
    internal static List<Point> AStar(MapElem[,] field, Point startPos, Point endPos, Point size)
    {
      List<Point> result = null;
      //До тех пока не возникнет необходимости окружать карту снаружи непроходимыми клетками будет эта проверка
      Func<int, int, bool> inRange = (int value, int top) => (value >= 0) && (value < top);
      var comparer = new AStarVertexComparer();
      List<AStarVertex> openList = new List<AStarVertex>();
      List<AStarVertex> closedList = new List<AStarVertex>();
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
            openList.Add(new AStarVertex {F = g + h, G = g, H = h, ID = id, ParentID = parentID, Position = position});
            id++;
          };
      addToOpenList(0, 0, -1, startPos);
      //Цикл поиска пути
      while(true)
      {
        if(openList.Count == 0) //Невозможно найти путь
        {
          break;
        }
        if(openList.Any(x => x.Position.X == endPos.X && x.Position.Y == endPos.Y)) //Если путь найден
        {
          found = true;
          break;
        }
        openList.Sort(comparer);
        AStarVertex current = openList[0];
        openList.Remove(current);
        closedList.Add(current);
        for(MonsterDirection i = MonsterDirection.Up; i <= MonsterDirection.Left; i++)
        {
          Point tmp = new Point(current.Position.X + actions[i].X, current.Position.Y + actions[i].Y);
          if(!inRange(tmp.X, size.X)
             || !inRange(tmp.Y, size.Y)
             || field[tmp.Y, tmp.X].Status != MapElemStatus.CanMove
             || closedList.Any(value => value.Position.X == tmp.X && value.Position.Y == tmp.Y))
          {
            continue;
          }
          int openPos = openList.FindIndex(value => value.Position.X == tmp.X && value.Position.Y == tmp.Y);
          if(openPos == -1) //Ещё не в открытом списке
          {
            int g = 10 + current.G + Detector(tmp, current.Position);
              //Если можно ходить в диагональные точки, то сделать проверку +10 или +14
            int h = GetDistance(tmp, endPos) * 10;
            addToOpenList(g, h, current.ID, tmp);
          }
          else
          {
            if(openList[openPos].G > current.G + 10 + Detector(tmp, current.Position))
              //Если можно ходить в диагональные точки, то сделать проверку +10 или +14
            {
              var tmpVertex = openList[openPos];
              tmpVertex.G = current.G + 10 + Detector(tmp, current.Position);
              tmpVertex.F = tmpVertex.H + tmpVertex.G;
              tmpVertex.ParentID = current.ID;
              openList[openPos] = tmpVertex;
            }
          }
        }
      }
      if(found)
      {
        result = new List<Point>();
        var currentElem = openList.Find(value => value.Position == endPos);
        do
        {
          result.Add(currentElem.Position);
          currentElem = closedList.Find(value => value.ID == currentElem.ParentID);
        } while(currentElem.ParentID != -1);
        result.Add(startPos);
        result.Reverse();
      }
      return result;
    }

    internal static List<Point> AStarFast(MapElem[,] field, Point startPos, Point endPos, Point size)
    {
      // ReSharper disable InconsistentNaming
      AStarVertexFast[,] AStarField = new AStarVertexFast[size.Y,size.X];
      // ReSharper restore InconsistentNaming
      for(int i = 0; i < size.Y; i++)
      {
        for(int j = 0; j < size.X; j++)
        {
          AStarField[i, j] = new AStarVertexFast {G = 0, H = 0, ParentPosition = new Point(), Status = 0};
        }
      }
      List<Point> result = null;
      //До тех пока не возникнет необходимости окружать карту снаружи непроходимыми клетками будет эта проверка
      Func<int, int, bool> inRange = (int value, int top) => (value >= 0) && (value < top);
      PriorityQueue<int, Point> openList = new PriorityQueue<int, Point>();
      bool found = false;
      Dictionary<MonsterDirection, Point> actions = new Dictionary<MonsterDirection, Point>();
      actions[MonsterDirection.Up] = new Point(0, -1);
      actions[MonsterDirection.Right] = new Point(1, 0);
      actions[MonsterDirection.Down] = new Point(0, 1);
      actions[MonsterDirection.Left] = new Point(-1, 0);
      Action<int, int, Point, Point> addToOpenList =
        (int g, int h, Point position, Point parentPosition) =>
          {
            AStarField[position.Y, position.X].Status = 1;
            AStarField[position.Y, position.X].G = g;
            AStarField[position.Y, position.X].H = h;
            AStarField[position.Y, position.X].ParentPosition = parentPosition;
            openList.Enqueue(g + h, position);
          };
      addToOpenList(0, 0, startPos, startPos);
      //Цикл поиска пути
      while(true)
      {
        if(openList.Count == 0) //Невозможно найти путь
        {
          break;
        }
        if(openList.Any(x => x.Value == endPos)) //Если путь найден
        {
          found = true;
          break;
        }
        Point current = openList.DequeueValue();
        bool vertical = (current.X == AStarField[current.Y, current.X].ParentPosition.X);
        AStarField[current.Y, current.X].Status = 2;
        for(MonsterDirection i = MonsterDirection.Up; i <= MonsterDirection.Left; i++)
        {
          Point tmp = new Point(current.X + actions[i].X, current.Y + actions[i].Y);
          if(!inRange(tmp.X, size.X)
             || !inRange(tmp.Y, size.Y)
             || field[tmp.Y, tmp.X].Status != MapElemStatus.CanMove
             || AStarField[tmp.Y, tmp.X].Status == 2)
          {
            continue;
          }
          int penalty = 0;
          if(vertical) //если X одинаковые, значит движемся по вертикали
          {
            if(current.X != tmp.X)
            {
              penalty = 50;
            }
          }
          else //По горизонтали
          {
            if(current.Y != tmp.Y)
            {
              penalty = 50;
            }
          }
          if(AStarField[tmp.Y, tmp.X].Status == 0) //Ещё не в открытом списке
          {
            int g = 10 + AStarField[current.Y, current.X].G + penalty;
              //Если можно ходить в диагональные точки, то сделать проверку +10 или +14
            int h = GetDistance(tmp, endPos) * 10;
            addToOpenList(g, h, tmp, current);
          }
          else
          {
            if(AStarField[tmp.Y, tmp.X].G > AStarField[current.Y, current.X].G + 10 + penalty)
              //Если можно ходить в диагональные точки, то сделать проверку +10 или +14
            {
              openList.Remove(new KeyValuePair<int, Point>(AStarField[tmp.Y, tmp.X].G + AStarField[tmp.Y, tmp.X].H, tmp));
              AStarField[tmp.Y, tmp.X].G = AStarField[current.Y, current.X].G + 10 + penalty;
              openList.Enqueue(AStarField[tmp.Y, tmp.X].G + AStarField[tmp.Y, tmp.X].H, tmp);
            }
          }
        }
      }
      if(found)
      {
        result = new List<Point>();
        var currentElem = endPos;
        do
        {
          result.Add(currentElem);
          currentElem = AStarField[currentElem.Y, currentElem.X].ParentPosition;
        } while(currentElem != startPos);
        result.Add(startPos);
        result.Reverse();
      }
      return result;
    }
  }
}