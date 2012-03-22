using System;
using System.Drawing;
using System.Globalization;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Loaders;

namespace GameCoClassLibrary.Classes
{
  /*delegate bool PageAction(int x, int y, int offset, int XMouse = 0, int YMouse = 0);//Делегат для страницы
  delegate bool PageSelectorAction(int x, int DY, int XMouse = 0, int YMouse = 0);//Делегат для Page Selector*/
  //-Устарело

  internal static class Helpers
  {
    /// <summary>
    /// Построение прямоугольника для элемента Page Selector'а
    /// </summary>
    internal static Func<Game, int, int, Rectangle> LambdaBuildRectPageSelector = (gameObj, x, dy) =>
        new Rectangle(Convert.ToInt32((Settings.MapAreaSize + 10 + (x % 3) * ("Page " + (x + 1).ToString(CultureInfo.InvariantCulture)).Length * 12 + Settings.DeltaX * 2) * gameObj.Scaling),
             Convert.ToInt32((Res.MoneyPict.Height + 35 * (dy + 1)) * gameObj.Scaling),
             Convert.ToInt32(("Page " + (x + 1).ToString(CultureInfo.InvariantCulture)).Length * 11 * gameObj.Scaling), Convert.ToInt32(24 * gameObj.Scaling));

    /// <summary>
    /// Построение прямоугольника для элемента Страницы Магазина
    /// </summary>
    internal static Func<Game, int, int, Rectangle> LambdaBuildRectPage = (gameObj, x, y) =>
      new Rectangle(Convert.ToInt32((Settings.MapAreaSize + 10 + x * 42 + Settings.DeltaX * 2) * gameObj.Scaling),
                    Convert.ToInt32((60 + Res.MoneyPict.Height + y * 42 + 40) * gameObj.Scaling),
                    Convert.ToInt32(32 * gameObj.Scaling), Convert.ToInt32(32 * gameObj.Scaling));

    internal static Random RandomForCrit = new Random();//Для вычисления шанса на критический удар

    internal static Pen BlackPen;
    internal static Pen GreenPen;

    /// <summary>
    /// Построитель областей
    /// А зачем вообще это? Если захочется изменить положение кнопок, чтобы переписать в одном месте и для вывода на экран и для проверки
    /// попадает ли курсор на кнопку при нажатии
    /// Проверки на попали ли вообще в область магазина башен(к примеру) делается в одном месте и выносить оттуда проверку смысла не
    /// </summary>
    /// <param name="rectType">Для какой области строится прямоугольник</param>
    /// <param name="scaling">Масштабирование</param>
    /// <returns>Прямоугольная область</returns>
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
  }
}