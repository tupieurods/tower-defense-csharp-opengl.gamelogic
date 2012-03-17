using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using GameCoClassLibrary.Enums;

namespace GameCoClassLibrary.Classes
{
  internal static class THelpers
  {
    /*Построитель областей
     * А зачем вообще это? Если захочется изменить положение кнопок, чтобы переписать в одном месте и для вывода на экран и для проверки
     * попадает ли курсор на кнопку при нажатии
     * Проверки на попали ли вообще в область магазина башен(к примеру) делается в одном месте и выносить оттуда проверку смысла не имеет
     */
    internal static Rectangle BuildRect(RectBuilder RectType, float Scaling)
    {
      switch (RectType)
      {
        case RectBuilder.Destroy:
          return new Rectangle(Convert.ToInt32((730 - Res.BDestroyTower.Width) * Scaling), Convert.ToInt32(335 * Scaling),
          Convert.ToInt32(Res.BDestroyTower.Width * Scaling), Convert.ToInt32(Res.BDestroyTower.Height * Scaling));
        case RectBuilder.Upgrade:
          return new Rectangle(Convert.ToInt32((730 - Res.BUpgradeTower.Width) * Scaling), Convert.ToInt32((325 - Res.BDestroyTower.Height) * Scaling),
          Convert.ToInt32(Res.BUpgradeTower.Width * Scaling), Convert.ToInt32(Res.BUpgradeTower.Height * Scaling));
        case RectBuilder.NewLevelEnabled:
          return new Rectangle(Convert.ToInt32((Settings.DeltaX + (Settings.MapAreaSize / 2) - (Res.BStartLevelDisabled.Width / 2)) * Scaling),
          Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * Scaling),
          Convert.ToInt32(Res.BStartLevelDisabled.Width * Scaling), Convert.ToInt32(Res.BStartLevelDisabled.Height * Scaling));
        case RectBuilder.NewLevelDisabled:
          return new Rectangle(Convert.ToInt32((Settings.DeltaX + (Settings.MapAreaSize / 2) - (Res.BStartLevelEnabled.Width / 2)) * Scaling),
          Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * Scaling),
          Convert.ToInt32(Res.BStartLevelEnabled.Width * Scaling), Convert.ToInt32(Res.BStartLevelEnabled.Height * Scaling));
      }
      return new Rectangle();
    }
  }
}
