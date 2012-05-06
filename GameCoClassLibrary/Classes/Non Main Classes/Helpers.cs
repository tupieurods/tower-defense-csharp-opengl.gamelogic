using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Loaders;

namespace GameCoClassLibrary.Classes
{
  //About Magic numbers, they are all documented on the list of paper
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

    /// <summary>
    /// Cycle for buttons(for Button enum)
    /// Cycle works while <code>Func(Button, bool)</code>==false
    /// </summary>
    internal static Action<Func<Button, bool>> ButtonCycle =
      act =>
      {
        for (Button i = 0; i < (Button)Enum.GetNames(typeof(Button)).Length; i++)
        {
          if (act(i))//Continue cycle, if false
            break;
        }
      };

    /// <summary>
    /// random number generator, for critical strike
    /// </summary>
    internal static Random RandomForCrit = new Random();

    /// <summary>
    /// Black pen Chache
    /// </summary>
    internal static Pen BlackPen;
    /// <summary>
    /// Green pen Chache
    /// </summary>
    internal static Pen GreenPen;

    /// <summary>
    /// Builds the rect for buttons. Why? DRY(Used for drawing and click checking).
    /// </summary>
    /// <param name="buttonType">Type of the button.</param>
    /// <param name="scaling">The scaling.</param>
    /// <param name="gameStarted">if set to <c>true</c> scaling buttons renders in other place.</param>
    /// <returns></returns>
    internal static Rectangle BuildButtonRect(Button buttonType, float scaling, bool gameStarted = false)
    {
      Point location;
      Size size = new Size(Convert.ToInt32(Res.Buttons[buttonType].Width * scaling), Convert.ToInt32(Res.Buttons[buttonType].Height * scaling));
      switch (buttonType)
      {
        case Button.StartLevelEnabled:
          location = new Point(Convert.ToInt32((Settings.BreakipLineXPosition - Settings.DeltaX - Res.Buttons[Button.StartLevelDisabled].Width) * scaling),
          Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize + Res.Buttons[Button.SmallScale].Height) * scaling));
          break;
        case Button.StartLevelDisabled:
          location = new Point(Convert.ToInt32((Settings.BreakipLineXPosition - Settings.DeltaX - Res.Buttons[Button.StartLevelDisabled].Width) * scaling),
          Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize + Res.Buttons[Button.SmallScale].Height) * scaling));
          break;
        case Button.DestroyTower:
          location = new Point(Convert.ToInt32((730 - Res.Buttons[Button.DestroyTower].Width) * scaling), Convert.ToInt32(335 * scaling));
          break;
        case Button.UpgradeTower:
          location = new Point(Convert.ToInt32((730 - Res.Buttons[Button.UpgradeTower].Width) * scaling), Convert.ToInt32((325 - Res.Buttons[Button.DestroyTower].Height) * scaling));
          break;
        case Button.BigScale:
          if (!gameStarted)
          {
            location = new Point(Convert.ToInt32((Settings.WindowWidth - Res.Buttons[Button.NewGame].Width - Res.Buttons[Button.BigScale].Width) * scaling),
                Convert.ToInt32(100 * scaling));
          }
          else
          {
            location = new Point(Convert.ToInt32(Settings.DeltaX * scaling), Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * scaling));
          }
          break;
        case Button.NormalScale:
          if (!gameStarted)
          {
            location = new Point(Convert.ToInt32((Settings.WindowWidth - Res.Buttons[Button.NewGame].Width - Res.Buttons[Button.BigScale].Width
              - Res.Buttons[Button.NormalScale].Width) * scaling), Convert.ToInt32(100 * scaling));
          }
          else
          {
            location = new Point(Convert.ToInt32((Settings.DeltaX + Res.Buttons[Button.BigScale].Width) * scaling),
                Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * scaling));
          }
          break;
        case Button.SmallScale:
          if (!gameStarted)
          {
            location = new Point(Convert.ToInt32((Settings.WindowWidth - Res.Buttons[Button.NewGame].Width - Res.Buttons[Button.BigScale].Width -
                Res.Buttons[Button.NormalScale].Width - Res.Buttons[Button.SmallScale].Width) * scaling), Convert.ToInt32(100 * scaling));
          }
          else
          {
            location = new Point(Convert.ToInt32((Settings.DeltaX + Res.Buttons[Button.BigScale].Width + Res.Buttons[Button.NormalScale].Width) * scaling),
            Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * scaling));
          }
          break;
        case Button.Exit:
          location = new Point(Convert.ToInt32((Settings.WindowWidth - Res.Buttons[Button.Exit].Width) * scaling),
            Convert.ToInt32((100 + Res.Buttons[Button.NewGame].Height + Res.Buttons[Button.LoadGame].Height) * scaling)/* + 5*/);
          break;
        case Button.LoadGame:
          if (!gameStarted)
          {
            location = new Point(Convert.ToInt32((Settings.WindowWidth - Res.Buttons[Button.LoadGame].Width) * scaling),
                                 Convert.ToInt32((100 + Res.Buttons[Button.NewGame].Height) * scaling) /* + 5*/);
          }
          else
          {
            location = new Point(Convert.ToInt32((Settings.WindowWidth - Res.Buttons[Button.LoadGame].Width * 0.5) * scaling),
                                 Convert.ToInt32((1 + Res.Buttons[Button.NewGame].Height * 0.5) * scaling));
            size.Width = (int)(size.Width * 0.5);
            size.Height = (int)(size.Height * 0.5);
          }
          break;
        case Button.SaveGame:
          location = new Point(Convert.ToInt32(Settings.DeltaX * scaling),
            Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize + Res.Buttons[Button.BigScale].Height) * scaling));
          break;
        case Button.Pause:
          location = new Point(Convert.ToInt32((Settings.DeltaX + Res.Buttons[Button.BigScale].Width + Res.Buttons[Button.SmallScale].Width
            + Res.Buttons[Button.NormalScale].Width) * scaling), Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * scaling));
          break;
        case Button.Unpause:
          location = new Point(Convert.ToInt32((Settings.DeltaX + Res.Buttons[Button.BigScale].Width + Res.Buttons[Button.SmallScale].Width
            + Res.Buttons[Button.NormalScale].Width) * scaling), Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * scaling));
          break;
        case Button.NewGame:
          if (!gameStarted)
          {
            location = new Point(Convert.ToInt32((Settings.WindowWidth - Res.Buttons[Button.NewGame].Width) * scaling),
                                 Convert.ToInt32(100 * scaling));
          }
          else
          {
            location = new Point(Convert.ToInt32((Settings.WindowWidth - Res.Buttons[Button.NewGame].Width * 0.5) * scaling),
                                 Convert.ToInt32(1 * scaling));
            size.Width = (int)(size.Width * 0.5);
            size.Height = (int)(size.Height * 0.5);
          }
          break;
        default:
          throw new ArgumentOutOfRangeException("buttonType");
      }
      return new Rectangle(location, size);
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