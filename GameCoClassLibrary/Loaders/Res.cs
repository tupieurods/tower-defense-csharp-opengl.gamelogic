using System;
using System.Drawing;
using GameCoClassLibrary.Enums;
using System.Collections.Generic;

namespace GameCoClassLibrary.Loaders
{
  internal static class Res
  {
    /// <summary>
    /// Bitmaps for game
    /// B before variable name means Button
    /// </summary>
    static internal Bitmap MoneyPict, MenuBackground/*, BStartLevelEnabled, BStartLevelDisabled, BDestroyTower, BUpgradeTower,
      BBigScale, BNormalScale, BSmallScale, BExit, BLoadGame, BSaveGame, BPause, BUnpause, BNewGame*/;

    internal static Dictionary<Button, Bitmap> Buttons;

    /// <summary>
    /// Initializes the <see cref="Res"/> class. Loads pictures
    /// </summary>
    static Res()
    {
      Buttons = new Dictionary<Button, Bitmap>(Enum.GetNames(typeof(Button)).Length);
      try
      {
        for (Button i = 0; i < (Button)Enum.GetNames(typeof(Button)).Length; i++)
        {
          Buttons.Add(i, new Bitmap(string.Format(Environment.CurrentDirectory + "\\Data\\Images\\{0}.png", i)));
          Buttons[i].MakeTransparent();
        }
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
