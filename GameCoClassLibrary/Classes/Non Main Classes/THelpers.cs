using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Loaders;

namespace GameCoClassLibrary.Classes
{
  /*delegate bool PageAction(int x, int y, int offset, int XMouse = 0, int YMouse = 0);//Делегат для страницы
  delegate bool PageSelectorAction(int x, int DY, int XMouse = 0, int YMouse = 0);//Делегат для Page Selector*/
  //-Устарело

  internal static class THelpers
  {
    /// <summary>
    /// Построение прямоугольника для элемента Page Selector'а
    /// </summary>
    internal static Func<TGame, int, int, Rectangle> LambdaBuildRectPageSelector = (GameObj, x, DY) =>
        new Rectangle(Convert.ToInt32((Settings.MapAreaSize + 10 + (x % 3) * ("Page " + (x + 1).ToString()).Length * 12 + Settings.DeltaX * 2) * GameObj.Scaling),
             Convert.ToInt32((Res.MoneyPict.Height + 35 * (DY + 1)) * GameObj.Scaling),
             Convert.ToInt32(("Page " + (x + 1).ToString()).Length * 11 * GameObj.Scaling), Convert.ToInt32(24 * GameObj.Scaling));

    /// <summary>
    /// Построение прямоугольника для элемента Страницы Магазина
    /// </summary>
    internal static Func<TGame, int, int, Rectangle> LambdaBuildRectPage = (GameObj, x, y) =>
      new Rectangle(Convert.ToInt32((Settings.MapAreaSize + 10 + x * 42 + Settings.DeltaX * 2) * GameObj.Scaling),
                    Convert.ToInt32((60 + Res.MoneyPict.Height + y * 42 + 40) * GameObj.Scaling),
                    Convert.ToInt32(32 * GameObj.Scaling), Convert.ToInt32(32 * GameObj.Scaling));

    internal static Random RandomForCrit = new Random();//Для вычисления шанса на критический удар

    internal static Pen BlackPen;
    internal static Pen GreenPen;

    /// <summary>
    /// Построитель областей
    /// А зачем вообще это? Если захочется изменить положение кнопок, чтобы переписать в одном месте и для вывода на экран и для проверки
    /// попадает ли курсор на кнопку при нажатии
    /// Проверки на попали ли вообще в область магазина башен(к примеру) делается в одном месте и выносить оттуда проверку смысла не
    /// </summary>
    /// <param name="RectType">Для какой области строится прямоугольник</param>
    /// <param name="Scaling">Масштабирование</param>
    /// <returns>Прямоугольная область</returns>
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
