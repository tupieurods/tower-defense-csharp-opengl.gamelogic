using System;
using System.Drawing;
using System.Linq;
using System.Globalization;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Interfaces;
using GameCoClassLibrary.Loaders;

namespace GameCoClassLibrary.Classes
{
  //По поводу графических констант, +40 допустим, после прекращения разработки я сам еле вспомню почему именно столько
  //необходимо на отдельно листе нарисовать(Уже нарисовано) и везде показать почему именно столько

  //Графическая часть полностью в это регионе
  //КРОМЕ ОДНОЙ!!!
  //Вывод Page Selector'а и самой страницы магазина башен производится в ShopPage*Action
  //Если кто предложит лучший вариант передалю
  internal sealed class GraphicEngine
  {
    private readonly IGraphic _graphObject;
    private Bitmap _constantMapImage;//Постоянное изображение карты для данного масштаба(+ к производительности и решение проблемы утечки памяти)
    private readonly Color _backgroundColor = Color.Silver;//Цвет заднего фона

    internal bool RepaintConstImage { get; set; }

    internal GraphicEngine(IGraphic graphObject)
    {
      _graphObject = graphObject;
    }

    internal void SetNewGraphBuffer(BufferedGraphics graphicalBuffer)
    {
      WinFormsGraphic winFormsGraphic = _graphObject as WinFormsGraphic;
      if (winFormsGraphic != null)
        winFormsGraphic.SetNewGraphBuffer(graphicalBuffer);
    }

    /// <summary>
    /// Создание нового изображения поля
    /// </summary>
    /// <param name="gameObj"></param>
    /// <param name="scale"></param>
    internal void RecreateConstantImage(Game gameObj, float scale)
    {
      _constantMapImage = new Bitmap(Convert.ToInt32((gameObj.Map.VisibleXFinish - gameObj.Map.VisibleXStart) * Settings.ElemSize * scale),
            Convert.ToInt32((gameObj.Map.VisibleYFinish - gameObj.Map.VisibleYStart) * Settings.ElemSize * scale));
      RepaintConstImage = true;
    }

    /// <summary>
    /// Вызывает все процедуры вывода
    /// Основная процедура, перерисовывает весь игровой экран
    /// </summary>
    /// <param name="gameObj"> </param>
    internal void Show(Game gameObj)
    {
      //Залили одним цветом
      _graphObject.FillRectangle(new SolidBrush(_backgroundColor), 0, 0, Convert.ToInt32(Settings.WindowWidth * gameObj.Scaling),
        Convert.ToInt32(Settings.WindowHeight * gameObj.Scaling));
      //Вывели область карты
      MapAreaShowing(gameObj);
      //StartLevelButton
      BStartLevelShow(gameObj);
      _graphObject.DrawString(gameObj.Monsters.Count.ToString(CultureInfo.InvariantCulture), new Font("Arial", Settings.ElemSize),
        new SolidBrush(Color.Black), new Point(0, 0));

      #region Вывод GUI

      {
        //Вывели линию разделения
        _graphObject.DrawLine(new Pen(new SolidBrush(Color.White), 3 * gameObj.Scaling),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling), 0),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling), Convert.ToInt32(Settings.WindowHeight * gameObj.Scaling)));
        ShowMoney(gameObj);//Деньги
        ShowLives(gameObj);//жизни
        ShowPageSelector(gameObj);//Меню магазина
        ShowTowerShopPage(gameObj);//Страница магазина
        if ((gameObj.TowerConfSelectedID != -1) || (gameObj.TowerMapSelectedID != -1))//Нужно ли выводить параметры
          ShowTowerParams(gameObj);
      }

      #endregion Вывод GUI

        _graphObject.Render();
    }

    /// <summary>
    /// Перерисовка области карты
    /// </summary>
    /// <param name="gameObj"> </param>
    private void MapAreaShowing(Game gameObj)
    {
      //Если нужно изменить "неизменяемую" область карты
      if (RepaintConstImage)
      {
        gameObj.Map.GetConstantBitmap(_constantMapImage, Convert.ToInt32(Settings.MapAreaSize * gameObj.Scaling),
                                      Convert.ToInt32(Settings.MapAreaSize * gameObj.Scaling));
        RepaintConstImage = false;
        //Если не вызывать вручную, можно поиметь утечку в 100 и более мегабайт памяти
        GC.Collect();
      }
      //Ограничиваем область для рисования
      _graphObject.Clip = new Region(new Rectangle(Settings.DeltaX, Settings.DeltaY,
                                            Convert.ToInt32((gameObj.Map.VisibleXFinish - gameObj.Map.VisibleXStart) *
                                                            Settings.ElemSize * gameObj.Scaling),
                                            Convert.ToInt32((gameObj.Map.VisibleYFinish - gameObj.Map.VisibleYStart) *
                                                            Settings.ElemSize * gameObj.Scaling)));
      //Выводим карту
      _graphObject.DrawImage(_constantMapImage, Settings.DeltaX, Settings.DeltaY, _constantMapImage.Width,
                      _constantMapImage.Height);
      Point visibleStart = new Point(gameObj.Map.VisibleXStart, gameObj.Map.VisibleYStart);
      Point visibleFinish = new Point(gameObj.Map.VisibleXFinish, gameObj.Map.VisibleYFinish);

      #region Вывод изображений башен

      foreach (Tower tower in gameObj.Towers)
      {
        tower.ShowTower(_graphObject, visibleStart, visibleFinish);
      }

      #endregion Вывод изображений башен

      #region Вывод изображений монстров

      foreach (Monster monster in gameObj.Monsters.Where(monster => !monster.DestroyMe))
      {
        monster.ShowMonster(_graphObject, visibleStart, visibleFinish);
      }

      #endregion Вывод изображений монстров

      //Следующий далее region переделать, чтобы был один вариант вызова, просто от нескольких переменных(если получится)

      #region Вывод таких вещей как попытка постановки башни или выделение поставленой

      if (gameObj.ArrayPosForTowerStanding.X != -1)
      {
        ShowSquareAndCircleAtTower(gameObj, gameObj.ArrayPosForTowerStanding,
                                   gameObj.TowerParamsForBuilding[gameObj.TowerConfSelectedID].UpgradeParams[0].
                                     AttackRadius,
                                   gameObj.Check(gameObj.ArrayPosForTowerStanding) ? Color.White : Color.Red);
      }
      else if (gameObj.TowerMapSelectedID != -1)
      {
        ShowSquareAndCircleAtTower(gameObj, new Point(gameObj.Towers[gameObj.TowerMapSelectedID].ArrayPos.X - gameObj.Map.VisibleXStart,
          gameObj.Towers[gameObj.TowerMapSelectedID].ArrayPos.Y - gameObj.Map.VisibleYStart),
          gameObj.Towers[gameObj.TowerMapSelectedID].CurrentTowerParams.AttackRadius, Color.White);
      }

      #endregion Вывод таких вещей как попытка постановки башни или выделение поставленой

      #region Вывод снарядов

      foreach (Missle missle in gameObj.Missels.Where(missle => !missle.DestroyMe))
      {
        missle.Show(_graphObject, visibleStart, visibleFinish, gameObj.Monsters);
      }

      #endregion Вывод снарядов

      _graphObject.Clip = new Region();
    }

    #region Сектор башен

    /// <summary>
    /// Вывод Selector'а страниц в магазине башен
    /// </summary>
    /// <param name="gameObj"> </param>
    private void ShowPageSelector(Game gameObj)
    {
      if (gameObj.TowerParamsForBuilding.Count > Settings.ElemSize)
      {
        // ReSharper disable InconsistentNaming
        gameObj.ShopPageSelectorAction((int i, int dy, int XMouse, int YMouse) =>
        // ReSharper restore InconsistentNaming
        {
          //Строка
          _graphObject.DrawString("Page " + (i + 1).ToString(CultureInfo.InvariantCulture), new Font("Arial", 14 * gameObj.Scaling), new SolidBrush(Color.Black),
            new Point(
              Convert.ToInt32((Settings.MapAreaSize + 10 + (i % 3) * ("Page " + (i + 1).ToString(CultureInfo.InvariantCulture)).Length * 12 + Settings.DeltaX * 2) * gameObj.Scaling),
              Convert.ToInt32((Res.MoneyPict.Height + 35 * (dy + 1)) * gameObj.Scaling)));//Эта часть с new Point сильно раздражает
          //но как убрать и сделать красивее пока не знаю
          //Вывод рамки
          Color penColor = ((i + 1) == gameObj.CurrentShopPage) ? Color.Red : Color.White;
          _graphObject.DrawRectangle(new Pen(penColor, 2 * gameObj.Scaling), Helpers.LambdaBuildRectPageSelector(gameObj, i, dy));
          return false;
        });
      }
    }

    /// <summary>
    /// Показ страницы магазина
    /// </summary>
    /// <param name="gameObj"> Ссылка на TGame </param>
    private void ShowTowerShopPage(Game gameObj)
    {
      // ReSharper disable InconsistentNaming
      gameObj.ShopPageAction((int i, int j, int offset, int XMouse, int YMouse) =>
      // ReSharper restore InconsistentNaming
      {
        _graphObject.DrawImage(gameObj.TowerParamsForBuilding[(gameObj.CurrentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset].Icon,
          Helpers.LambdaBuildRectPage(gameObj, i, j));
        if (gameObj.TowerConfSelectedID == (gameObj.CurrentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset)//Если эта башня выбрана в магазине
          //обозначим это графически
          _graphObject.DrawRectangle(new Pen(Color.Red, 3 * gameObj.Scaling), Helpers.LambdaBuildRectPage(gameObj, i, j));
        return false;
      });
    }

    /// <summary>
    /// Вывод параметров выделенной(в магазине или на карте) пушки
    /// </summary>
    /// <param name="gameObj"> </param>
    private void ShowTowerParams(Game gameObj)
    {
      string strToShow = "";
      if (gameObj.TowerConfSelectedID != -1)//Выводим информацию о покупаемой башне
      {
        strToShow = gameObj.TowerParamsForBuilding[gameObj.TowerConfSelectedID]
          + gameObj.TowerParamsForBuilding[gameObj.TowerConfSelectedID].UpgradeParams[0].ToString();
        //Т.к эта башня ещё не куплена, то надо вывести ещё стоимость
        _graphObject.DrawString("Cost: " + gameObj.TowerParamsForBuilding[gameObj.TowerConfSelectedID].UpgradeParams[0].Cost,
          new Font("Arial", Settings.ElemSize * gameObj.Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling) + 5, Convert.ToInt32(390 * gameObj.Scaling)));
      }
      if (gameObj.TowerMapSelectedID != -1)//Если выводим информацию о поставленной башне
      {
        strToShow = gameObj.Towers[gameObj.TowerMapSelectedID].ToString();//Строка вывода
        //Иконка башни
        _graphObject.DrawImage(gameObj.Towers[gameObj.TowerMapSelectedID].Icon,
          Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling) + 5, Convert.ToInt32(375 * gameObj.Scaling),
          Convert.ToInt32(gameObj.Towers[gameObj.TowerMapSelectedID].Icon.Width * gameObj.Scaling),
           Convert.ToInt32(gameObj.Towers[gameObj.TowerMapSelectedID].Icon.Height * gameObj.Scaling));
        //Выводим текущий уровень башни
        _graphObject.DrawString("Level: " + gameObj.Towers[gameObj.TowerMapSelectedID].Level.ToString(CultureInfo.InvariantCulture),
          new Font("Arial", Settings.ElemSize * gameObj.Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + gameObj.Towers[gameObj.TowerMapSelectedID].Icon.Width + Settings.DeltaX * 2) * gameObj.Scaling) + 5,
            Convert.ToInt32((375 + gameObj.Towers[gameObj.TowerMapSelectedID].Icon.Height / 2) * gameObj.Scaling)));
        //Кнопки Destroy и Upgrade
        BDestroyShow(gameObj);
        BUpgradeShow(gameObj);
      }
      //Характеристики
      _graphObject.DrawString(strToShow,
          new Font("Arial", 10 * gameObj.Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling) + 5, Convert.ToInt32(415 * gameObj.Scaling)));
      //Рамка для красоты
      _graphObject.DrawRectangle(new Pen(Color.Black), Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling) + 5,
        Convert.ToInt32(415 * gameObj.Scaling), Convert.ToInt32((200 - Settings.DeltaX * 2) * gameObj.Scaling),
        Convert.ToInt32((184) * gameObj.Scaling));
    }

    /// <summary>
    /// Вывод квадрата и радиуса атаки вокруг установленой/пытающейся установиться башни
    /// </summary>
    /// <param name="gameObj"> </param>
    /// <param name="position">Левый верхний квадрат для башни</param>
    /// <param name="radius">Радиус атаки</param>
    /// <param name="circleColor">Цвет круга</param>
    private void ShowSquareAndCircleAtTower(Game gameObj, Point position, int radius, Color circleColor)
    {
      //Квадрат
      _graphObject.DrawRectangle(new Pen(Color.Black),
          Convert.ToInt32(position.X * Settings.ElemSize * gameObj.Scaling + Settings.DeltaX),
          Convert.ToInt32(position.Y * Settings.ElemSize * gameObj.Scaling + Settings.DeltaY),
          Convert.ToInt32(Settings.ElemSize * 2 * gameObj.Scaling),
          Convert.ToInt32(Settings.ElemSize * 2 * gameObj.Scaling));
      //Радиус атаки
      _graphObject.DrawEllipse(new Pen(circleColor), ((position.X + 1) * Settings.ElemSize - radius) * gameObj.Scaling + Settings.DeltaX,
          ((position.Y + 1) * Settings.ElemSize - radius) * gameObj.Scaling + Settings.DeltaY, radius * 2 * gameObj.Scaling, radius * 2 * gameObj.Scaling);
    }

    #endregion Сектор башен

    #region Вывод кнопочек

    /// <summary>
    /// Показ кнопки начать новый уровень
    /// </summary>
    /// <param name="gameObj"> </param>
    private void BStartLevelShow(Game gameObj)
    {
      if (gameObj.LevelStarted)
      {
        _graphObject.DrawImage(Res.BStartLevelDisabled, Helpers.BuildRect(RectBuilder.NewLevelDisabled, gameObj.Scaling));
      }
      else
      {
        _graphObject.DrawImage(Res.BStartLevelEnabled, Helpers.BuildRect(RectBuilder.NewLevelEnabled, gameObj.Scaling));
      }
    }

    /// <summary>
    /// Уничтожить башню
    /// </summary>
    /// <param name="gameObj"> </param>
    private void BDestroyShow(Game gameObj)
    {
      _graphObject.DrawImage(Res.BDestroyTower, Helpers.BuildRect(RectBuilder.Destroy, gameObj.Scaling));
    }

    /// <summary>
    /// Показ кнопки улучшить башню
    /// </summary>
    /// <param name="gameObj"> </param>
    private void BUpgradeShow(Game gameObj)
    {
      if (!gameObj.Towers[gameObj.TowerMapSelectedID].CanUpgrade) return;
      //Вводится Tmp, т.к этот прямоугольник будет использоваться три раза
      Rectangle tmp = Helpers.BuildRect(RectBuilder.Upgrade, gameObj.Scaling);
      _graphObject.DrawImage(Res.BUpgradeTower, tmp);
      _graphObject.DrawString("Upgrade cost: " + gameObj.Towers[gameObj.TowerMapSelectedID].GetUpgradeCost,
                              new Font("Arial", Settings.ElemSize * gameObj.Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
                              new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * gameObj.Scaling) + 3, tmp.Y - Convert.ToInt32(25 * gameObj.Scaling)));
    }

    #endregion Вывод кнопочек

    #region Информация для игрока

    /// <summary>
    /// Вывод числа жизней
    /// </summary>
    /// <param name="gameObj"> </param>
    private void ShowLives(Game gameObj)
    {
      _graphObject.DrawString("Lives: " + gameObj.NumberOfLives.ToString(CultureInfo.InvariantCulture), new Font("Arial", 14 * gameObj.Scaling), new SolidBrush(Color.Black),
        new Point(Convert.ToInt32((Settings.MapAreaSize + 10 + Settings.DeltaX * 2) * gameObj.Scaling), Convert.ToInt32((Res.MoneyPict.Height + 10) * gameObj.Scaling)));
    }

    /// <summary>
    /// Вывод количества денег
    /// </summary>
    /// <param name="gameObj"> </param>
    private void ShowMoney(Game gameObj)
    {
      //Изображение монеты
      _graphObject.DrawImage(Res.MoneyPict, Convert.ToInt32((Settings.MapAreaSize + 10 + Settings.DeltaX * 2) * gameObj.Scaling),
        Convert.ToInt32(5 * gameObj.Scaling),
        Convert.ToInt32(Res.MoneyPict.Width * gameObj.Scaling),
        Convert.ToInt32(Res.MoneyPict.Height * gameObj.Scaling));
      //Вывод числа денег
      _graphObject.DrawString(gameObj.Gold.ToString(CultureInfo.InvariantCulture), new Font("Arial", 14 * gameObj.Scaling), new SolidBrush(Color.Black),
        new Point(Convert.ToInt32((Settings.MapAreaSize + 10 + Res.MoneyPict.Width + Settings.DeltaX * 2) * gameObj.Scaling), Convert.ToInt32(9 * gameObj.Scaling)));
    }

    #endregion Информация для игрока
  }
}