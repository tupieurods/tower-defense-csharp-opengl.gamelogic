using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using GameCoClassLibrary.Classes;
using Button = GameCoClassLibrary.Enums.Button;

namespace GameCoClassLibrary.Loaders
{
  internal static class Res
  {
    /// <summary>
    /// Bitmaps for game
    /// </summary>
    static internal readonly Bitmap MoneyPict;

    /// <summary>
    /// Main menu background cache
    /// </summary>
    private static readonly Dictionary<float, Bitmap> MainMenuBackground;

    /// <summary>
    /// Game buttons
    /// </summary>
    internal static readonly Dictionary<Button, Bitmap> Buttons;

    /// <summary>
    /// Background for pause menu
    /// </summary>
    internal static readonly Bitmap PauseMenuBackground;

    /// <summary>
    /// Initializes the <see cref="Res"/> class. Loads pictures
    /// </summary>
    static Res()
    {
      Buttons = new Dictionary<Button, Bitmap>(Enum.GetNames(typeof(Button)).Length);
      try
      {
        Helpers.ButtonCycle((Button i) =>
        {
          Buttons.Add(i, new Bitmap(string.Format(Environment.CurrentDirectory + "\\Data\\Images\\{0}.png", i)));
          Buttons[i].MakeTransparent();
          return false;
        });
        MoneyPict = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\Money.png");
        MoneyPict.MakeTransparent();
        PauseMenuBackground = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\PauseMenuBackground.png");
        MainMenuBackground = new Dictionary<float, Bitmap> { { 1.0F, new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\MenuBackground.png") } };
      }
      catch
      {
        MessageBox.Show("Game files damadged!", "Fatal error");
        Environment.Exit(1);
      }
    }

    /// <summary>
    /// Background of main menu getting
    /// </summary>
    /// <param name="scaling">The scaling.</param>
    /// <returns>Scaled main menu background</returns>
    internal static Bitmap MenuBackground(float scaling)
    {
      if (!MainMenuBackground.ContainsKey(scaling))
      {
        var tmpBitmap = new Bitmap((int)(MainMenuBackground[1.0F].Width * scaling), (int)(MainMenuBackground[1.0F].Height * scaling));
        var tmp = Graphics.FromImage(tmpBitmap);
        tmp.DrawImage(MainMenuBackground[1.0F], 0, 0, (int)(MainMenuBackground[1.0F].Width * scaling), (int)(MainMenuBackground[1.0F].Height * scaling));
        MainMenuBackground.Add(scaling, tmpBitmap);
      }
      return MainMenuBackground[scaling];
    }
  }
}
