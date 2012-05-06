using System;
using System.Drawing;
using GameCoClassLibrary.Enums;
using System.Collections.Generic;
using GameCoClassLibrary.Classes;

namespace GameCoClassLibrary.Loaders
{
  internal static class Res
  {
    /// <summary>
    /// Bitmaps for game
    /// </summary>
    static internal Bitmap MoneyPict, MenuBackground;

    /// <summary>
    /// Game buttons
    /// </summary>
    internal static Dictionary<Button, Bitmap> Buttons;

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
        MenuBackground = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\MenuBackground.png");
      }
      catch
      {
        System.Windows.Forms.MessageBox.Show("Game files damadged!", "Fatal error");
        Environment.Exit(1);
      }
    }
  }
}
