using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;

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
    internal static readonly Func<Func<int, int, bool>, int, bool> TowerSquareCycle =
      (act, enoughForTrue) =>
      {
        int countTrue = 0;
        for (int dx = 0; dx <= 1; dx++)
          for (int dy = 0; dy <= 1; dy++)
          {
            if (act(dx, dy))
              countTrue++;
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

    /// <summary>
    /// Checks if units x1y1 int the circle with center in x2y2 and raduis.
    /// </summary>
    /// <param name="x1">The x1.</param>
    /// <param name="y1">The y1.</param>
    /// <param name="x2">The x2.</param>
    /// <param name="y2">The y2.</param>
    /// <param name="radius">The radius.</param>
    /// <returns>Checking result</returns>
    internal static bool UnitInRadius(float x1, float y1, float x2, float y2, float radius)
    {
      return (Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2)) - radius < 0.1);
    }

    /// <summary>
    /// Gets the MD5 for file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>Hash</returns>
    internal static string GetMD5ForFile(string fileName)
    {
      using (FileStream fs = File.OpenRead(fileName))
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