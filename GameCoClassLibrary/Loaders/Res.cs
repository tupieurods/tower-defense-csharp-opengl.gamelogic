using System;
using System.Drawing;

namespace GameCoClassLibrary.Loaders
{
  internal static class Res
  {
    /// <summary>
    /// Bitmaps for game
    /// B before variable name means Button
    /// </summary>
    static internal Bitmap MoneyPict, BStartLevelEnabled, BStartLevelDisabled, BDestroyTower, BUpgradeTower;

    /// <summary>
    /// Initializes the <see cref="Res"/> class. Loads pictures
    /// </summary>
    static Res()
    {
      MoneyPict = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\Money.png");
      MoneyPict.MakeTransparent();
      BStartLevelEnabled = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\StartLevelEnabled.png");
      BStartLevelEnabled.MakeTransparent();
      BStartLevelDisabled = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\StartLevelDisabled.png");
      BStartLevelDisabled.MakeTransparent();
      BDestroyTower = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\Destroy.png");
      BDestroyTower.MakeTransparent();
      BUpgradeTower = new Bitmap(Environment.CurrentDirectory + "\\Data\\Images\\Up.png");
      BUpgradeTower.MakeTransparent();
    }
  }
}
