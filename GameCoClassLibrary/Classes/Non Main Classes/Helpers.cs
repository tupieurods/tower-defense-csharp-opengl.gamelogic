using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using GameCoClassLibrary.Enums;

namespace GameCoClassLibrary.Classes
{
  //About Magic numbers, they are all documented on the list of paper
  internal static class Helpers
  {
    /// <summary>
    /// Cycle for towers.
    /// In map array - tower it's square 2x2 with status=MapElemStatus.BusyByTower
    /// Needs to check or change this square in cycle. Thats lamda for cycle copy/paste prevention
    /// </summary>
    internal static readonly Func<int, Func<int, int, bool>, bool> TowerSquareCycle =
      (enoughForTrue, act) =>
        {
          int countTrue = 0;
          for(int dx = 0; dx <= 1; dx++)
          {
            for(int dy = 0; dy <= 1; dy++)
            {
              if(act(dx, dy))
              {
                countTrue++;
              }
            }
          }
          return countTrue >= enoughForTrue;
        };

    /// <summary>
    /// random number generator, for critical strike
    /// </summary>
    internal static readonly Random RandomForCrit = new Random();

    /// <summary>
    /// Black pen cache
    /// </summary>
    internal static Pen BlackPen;

    /// <summary>
    /// Green pen cache
    /// </summary>
    internal static Pen GreenPen;

    internal static readonly SolidBrush BlackBrush = new SolidBrush(Color.Black);

    /// <summary>
    /// Checks if units x1y1 int the circle with center in x2y2 and radius.
    /// </summary>
    /// <param name="x1">The x1.</param>
    /// <param name="y1">The y1.</param>
    /// <param name="x2">The x2.</param>
    /// <param name="y2">The y2.</param>
    /// <param name="radius">The radius.</param>
    /// <returns>Checking result</returns>
    internal static bool UnitInRadius(float x1, float y1, float x2, float y2, float radius)
    {
      return (Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2) - Math.Pow(radius, 2) < 0.1);
    }

    /// <summary>
    /// Gets deltaX and deltaY from MonsterDirection enum
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <param name="dx">The dx.</param>
    /// <param name="dy">The dy.</param>
    internal static void DirectionToDxDy(MonsterDirection direction, out int dx, out int dy)
    {
      dx = 0;
      dy = 0;
      switch(direction)
      {
        case MonsterDirection.Up:
          dy = -1;
          break;
        case MonsterDirection.Right:
          dx = 1;
          break;
        case MonsterDirection.Down:
          dy = 1;
          break;
        case MonsterDirection.Left:
          dx = -1;
          break;
      }
    }

    /// <summary>
    /// Gets the MD5 for file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>Hash</returns>
    internal static string GetMD5ForFile(string fileName)
    {
      using(FileStream fs = File.OpenRead(fileName))
      {
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] fileData = new byte[fs.Length];
        fs.Read(fileData, 0, (int)fs.Length);
        byte[] checkSum = md5.ComputeHash(fileData);
        string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
        return result;
      }
    }
  }
}