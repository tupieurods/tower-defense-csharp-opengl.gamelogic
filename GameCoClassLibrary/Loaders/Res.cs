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
    static internal Bitmap MoneyPict;

    private static readonly Dictionary<float, Bitmap> MenuBack;

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
        MenuBack = new Dictionary<float, Bitmap> { { 1.0F, new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\MenuBackground.png") } };
      }
      catch
      {
        System.Windows.Forms.MessageBox.Show("Game files damadged!", "Fatal error");
        Environment.Exit(1);
      }
    }

    internal static Bitmap MenuBackground(float scaling)
    {
      if (!MenuBack.ContainsKey(scaling))
      {
        var tmpBitmap = new Bitmap((int)(MenuBack[1.0F].Width * scaling), (int)(MenuBack[1.0F].Height * scaling));
        var tmp = Graphics.FromImage(tmpBitmap);
        tmp.DrawImage(MenuBack[1.0F], 0, 0, (int)(MenuBack[1.0F].Width * scaling), (int)(MenuBack[1.0F].Height * scaling));
        MenuBack.Add(scaling, tmpBitmap);
      }
      return MenuBack[scaling];
    }
  }
}
