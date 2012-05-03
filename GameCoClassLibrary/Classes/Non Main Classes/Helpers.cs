using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Loaders;

namespace GameCoClassLibrary.Classes
{

  internal static class Helpers
  {
    /// <summary>
    /// Rectangle Building for shop page menu
    /// </summary>
    internal static Func<Game, int, int, Rectangle> LambdaBuildRectPageSelector = (gameObj, x, dy) =>
        new Rectangle(
            Convert.ToInt32((Settings.MapAreaSize + 10 + (x % 3) * ("Page " + (x + 1).ToString(CultureInfo.InvariantCulture)).Length * 12 + Settings.DeltaX * 2) * gameObj.Scaling),
            Convert.ToInt32((Res.MoneyPict.Height + 35 * (dy + 1)) * gameObj.Scaling),
            Convert.ToInt32(("Page " + (x + 1).ToString(CultureInfo.InvariantCulture)).Length * 11 * gameObj.Scaling), Convert.ToInt32(24 * gameObj.Scaling)
                      );
    /// <summary>
    /// Rectangle Building for shop page element
    /// </summary>
    internal static Func<Game, int, int, Rectangle> LambdaBuildRectPage = (gameObj, x, y) =>
      new Rectangle(Convert.ToInt32((Settings.MapAreaSize + 10 + x * 42 + Settings.DeltaX * 2) * gameObj.Scaling),
                    Convert.ToInt32((60 + Res.MoneyPict.Height + y * 42 + 40) * gameObj.Scaling),
                    Convert.ToInt32(32 * gameObj.Scaling), Convert.ToInt32(32 * gameObj.Scaling));

    internal static Random RandomForCrit = new Random();//Для вычисления шанса на критический удар

    /// <summary>
    /// Black pen Chache
    /// </summary>
    internal static Pen BlackPen;
    /// <summary>
    /// Green pen Chache
    /// </summary>
    internal static Pen GreenPen;


    /// <summary>
    /// Builds the rect for buttons. Why? DRY.
    /// </summary>
    /// <param name="rectType">Type of the rect.</param>
    /// <param name="scaling">The scaling.</param>
    /// <returns>Rectangle object</returns>
    internal static Rectangle BuildRect(RectBuilder rectType, float scaling)
    {
      switch (rectType)
      {
        case RectBuilder.Destroy:
          return new Rectangle(Convert.ToInt32((730 - Res.BDestroyTower.Width) * scaling), Convert.ToInt32(335 * scaling),
          Convert.ToInt32(Res.BDestroyTower.Width * scaling), Convert.ToInt32(Res.BDestroyTower.Height * scaling));
        case RectBuilder.Upgrade:
          return new Rectangle(Convert.ToInt32((730 - Res.BUpgradeTower.Width) * scaling), Convert.ToInt32((325 - Res.BDestroyTower.Height) * scaling),
          Convert.ToInt32(Res.BUpgradeTower.Width * scaling), Convert.ToInt32(Res.BUpgradeTower.Height * scaling));
        case RectBuilder.NewLevelEnabled:
          return new Rectangle(Convert.ToInt32((Settings.DeltaX + (Settings.MapAreaSize / 2) - (Res.BStartLevelDisabled.Width / 2)) * scaling),
          Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * scaling),
          Convert.ToInt32(Res.BStartLevelDisabled.Width * scaling), Convert.ToInt32(Res.BStartLevelDisabled.Height * scaling));
        case RectBuilder.NewLevelDisabled:
          return new Rectangle(Convert.ToInt32((Settings.DeltaX + (Settings.MapAreaSize / 2) - (Res.BStartLevelEnabled.Width / 2)) * scaling),
          Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * scaling),
          Convert.ToInt32(Res.BStartLevelEnabled.Width * scaling), Convert.ToInt32(Res.BStartLevelEnabled.Height * scaling));
      }
      return new Rectangle();
    }

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