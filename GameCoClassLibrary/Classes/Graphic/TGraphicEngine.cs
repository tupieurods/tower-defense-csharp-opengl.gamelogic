using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GameCoClassLibrary.Loaders;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Interfaces;

namespace GameCoClassLibrary.Classes
{
  //По поводу графических констант, +40 допустим, после прекращения разработки я сам еле вспомню почему именно столько
  //необходимо на отдельно листе нарисовать(Уже нарисовано) и везде показать почему именно столько

  //Графическая часть полностью в это регионе
  //КРОМЕ ОДНОЙ!!!
  //Вывод Page Selector'а и самой страницы магазина башен производится в ShopPage*Action
  //Если кто предложит лучший вариант передалю
  internal sealed class TGraphicEngine : IGraphic
  {
    private BufferedGraphics GraphicalBuffer;
    private Bitmap ConstantMapImage;//Постоянное изображение карты для данного масштаба(+ к производительности и решение проблемы утечки памяти)
    private Color BackgroundColor = Color.Silver;//Цвет заднего фона
    internal bool RepaintConstImage { get; set; }

    internal TGraphicEngine(/*BufferedGraphics GraphicalBuffer*/)
    {
      //this.GraphicalBuffer = GraphicalBuffer;
      //RepaintConstImage = true;
    }

    internal void SetNewGraphBuffer(BufferedGraphics GraphicalBuffer)
    {
      this.GraphicalBuffer = GraphicalBuffer;
    }

    internal void RecreateConstantImage(TGame GameObj, float Scale)
    {
      ConstantMapImage = new Bitmap(Convert.ToInt32((GameObj.Map.VisibleXFinish - GameObj.Map.VisibleXStart) * Settings.ElemSize * Scale),
            Convert.ToInt32((GameObj.Map.VisibleYFinish - GameObj.Map.VisibleYStart) * Settings.ElemSize * Scale));
      RepaintConstImage = true;
    }

    /// <summary>
    /// Вызывает все процедуры вывода
    /// Основная процедура, перерисовывает весь игровой экран
    /// </summary>
    /// <param name="LinkToImage">Нужно ли делать постоянным для Picture Box'а или использовать более быстрый вывод</param>
    internal void Show(TGame GameObj, System.Windows.Forms.PictureBox GameDrawingSpace = null)
    {
      Graphics Canva;
      Bitmap DrawingBitmap = null;
      if (GameDrawingSpace != null)
      {
        DrawingBitmap = new Bitmap(Convert.ToInt32(730 * GameObj.Scaling), Convert.ToInt32(600 * GameObj.Scaling));
        Canva = Graphics.FromImage(DrawingBitmap);
      }
      else
      {
        Canva = GraphicalBuffer.Graphics;
      }
      //Залили одним цветом
      Canva.FillRectangle(new SolidBrush(BackgroundColor), 0, 0, Convert.ToInt32(730 * GameObj.Scaling), Convert.ToInt32(600 * GameObj.Scaling));
      //Вывели карту
      MapAreaShowing(GameObj, Canva);
      //StartLevelButton
      BStartLevelShow(GameObj, Canva);
      GraphicalBuffer.Graphics.DrawString(GameObj.Monsters.Count.ToString(), new Font("Arial", Settings.ElemSize), new SolidBrush(Color.Black), new Point(0, 0));
      #region Вывод GUI
      {
        //Вывели линию разделения
        Canva.DrawLine(new Pen(new SolidBrush(Color.White), 3 * GameObj.Scaling), new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * GameObj.Scaling), 0),
              new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * GameObj.Scaling), Convert.ToInt32(600 * GameObj.Scaling)));
        ShowMoney(GameObj, Canva);//Деньги
        ShowLives(GameObj, Canva);//жизни
        ShowPageSelector(GameObj, Canva);//Меню магазина
        ShowTowerShopPage(GameObj, Canva);//Страница магазина
        if ((GameObj.TowerConfSelectedID != -1) || (GameObj.TowerMapSelectedID != -1))//Нужно ли выводить параметры
          ShowTowerParams(GameObj, Canva);
      }
      #endregion
      if (GameDrawingSpace != null)
      {
        GameDrawingSpace.Image = DrawingBitmap;
        GraphicalBuffer.Graphics.DrawImage(DrawingBitmap, 0, 0);
      }
      else
      {
        GraphicalBuffer.Render();
      }
    }

    /// <summary>
    /// Перерисовка области карты
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void MapAreaShowing(TGame GameObj, Graphics Canva)
    {
      //Если нужно изменить "неизменяемую" область карты
      if (RepaintConstImage)
      {
        GameObj.Map.GetConstantBitmap(ConstantMapImage, (int)(Settings.MapAreaSize * GameObj.Scaling), (int)(Settings.MapAreaSize * GameObj.Scaling));
        RepaintConstImage = false;
        //Если не вызывать вручную, можно поиметь утечку в 100 и более мегабайт памяти
        GC.Collect();
      }
      //Ограничиваем область для рисования
      Canva.Clip = new Region(new Rectangle(Settings.DeltaX, Settings.DeltaY,
        Convert.ToInt32((GameObj.Map.VisibleXFinish - GameObj.Map.VisibleXStart) * Settings.ElemSize * GameObj.Scaling),
        Convert.ToInt32((GameObj.Map.VisibleYFinish - GameObj.Map.VisibleYStart) * Settings.ElemSize * GameObj.Scaling)));
      //Выводим карту
      Canva.DrawImage(ConstantMapImage, Settings.DeltaX, Settings.DeltaY, ConstantMapImage.Width, ConstantMapImage.Height);
      Point VisibleStart = new Point(GameObj.Map.VisibleXStart, GameObj.Map.VisibleYStart);
      Point VisibleFinish = new Point(GameObj.Map.VisibleXFinish, GameObj.Map.VisibleYFinish);
      #region Вывод изображений башен
      foreach (TTower Tower in GameObj.Towers)
      {
        Tower.ShowTower(Canva, VisibleStart, VisibleFinish, Settings.DeltaX, Settings.DeltaY);
      }
      #endregion
      #region Вывод изображений монстров
      foreach (TMonster Monster in GameObj.Monsters)
      {
        if (!Monster.DestroyMe)
          Monster.ShowMonster(Canva, VisibleStart, VisibleFinish);
      }
      #endregion
      //Следующий далее region переделать, чтобы был один вариант вызова, просто от нескольких переменных(если получится)
      #region Вывод таких вещей как попытка постановки башни или выделение поставленой
      if (GameObj.ArrayPosForTowerStanding.X != -1)
      {
        if (GameObj.Check(GameObj.ArrayPosForTowerStanding))
          ShowSquareAndCircleAtTower(GameObj, Canva, GameObj.ArrayPosForTowerStanding,
            GameObj.TowerParamsForBuilding[GameObj.TowerConfSelectedID].UpgradeParams[0].AttackRadius, Color.White);
        else
        {
          ShowSquareAndCircleAtTower(GameObj, Canva, GameObj.ArrayPosForTowerStanding,
            GameObj.TowerParamsForBuilding[GameObj.TowerConfSelectedID].UpgradeParams[0].AttackRadius, Color.Red);
        }
      }
      else if (GameObj.TowerMapSelectedID != -1)
      {
        ShowSquareAndCircleAtTower(GameObj, Canva, new Point(GameObj.Towers[GameObj.TowerMapSelectedID].ArrayPos.X - GameObj.Map.VisibleXStart,
          GameObj.Towers[GameObj.TowerMapSelectedID].ArrayPos.Y - GameObj.Map.VisibleYStart),
          GameObj.Towers[GameObj.TowerMapSelectedID].CurrentTowerParams.AttackRadius, Color.White);
      }
      #endregion
      #region Вывод снарядов
      foreach (TMissle Missle in GameObj.Missels)
      {
        if (!Missle.DestroyMe)
        {
          Missle.Move(GameObj.Monsters);
          Missle.Show(GraphicalBuffer.Graphics, VisibleStart, VisibleFinish, GameObj.Monsters);
        }
      }
      #endregion
      Canva.Clip = new Region();
    }

    #region Сектор башен
    /// <summary>
    /// Вывод Selector'а страниц в магазине башен
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void ShowPageSelector(TGame GameObj, Graphics Canva)
    {
      if (GameObj.TowerParamsForBuilding.Count > Settings.ElemSize)
      {
        GameObj.ShopPageSelectorAction((int i, int DY, int XMouse, int YMouse) =>
        {
          //Строка
          Canva.DrawString("Page " + (i + 1).ToString(), new Font("Arial", 14 * GameObj.Scaling), new SolidBrush(Color.Black),
            new Point(Convert.ToInt32((Settings.MapAreaSize + 10 + (i % 3) * ("Page " + (i + 1).ToString()).Length * 12 + Settings.DeltaX * 2) * GameObj.Scaling),
              Convert.ToInt32((Res.MoneyPict.Height + 35 * (DY + 1)) * GameObj.Scaling)));//Эта часть с new Point сильно раздражает
          //но как убрать и сделать красивее пока не знаю
          //Вывод рамки
          Color PenColor = ((i + 1) == GameObj.CurrentShopPage) ? Color.Red : Color.White;
          Canva.DrawRectangle(new Pen(PenColor, 2 * GameObj.Scaling), THelpers.LambdaBuildRectPageSelector(GameObj, i, DY));
          return false;
        },
        Canva);
      }
    }

    /// <summary>
    /// Показ страницы магазина
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void ShowTowerShopPage(TGame GameObj, Graphics Canva)
    {
      GameObj.ShopPageAction((int i, int j, int offset, int XMouse, int YMouse) =>
      {
        Canva.DrawImage(GameObj.TowerParamsForBuilding[(GameObj.CurrentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset].Icon,
          THelpers.LambdaBuildRectPage(GameObj, i, j));
        if (GameObj.TowerConfSelectedID == (GameObj.CurrentShopPage - 1) * (Settings.LinesInOnePage * Settings.MaxTowersInLine) + offset)//Если эта башня выбрана в магазине
          //обозначим это графически
          Canva.DrawRectangle(new Pen(Color.Red, 3 * GameObj.Scaling), THelpers.LambdaBuildRectPage(GameObj, i, j));
        return false;
      },
      Canva);
    }

    /// <summary>
    /// Вывод параметров выделенной(в магазине или на карте) пушки
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void ShowTowerParams(TGame GameObj, Graphics Canva)
    {
      string StrToShow = "";
      if (GameObj.TowerConfSelectedID != -1)//Выводим информацию о покупаемой башне
      {
        StrToShow = GameObj.TowerParamsForBuilding[GameObj.TowerConfSelectedID].ToString()
          + GameObj.TowerParamsForBuilding[GameObj.TowerConfSelectedID].UpgradeParams[0].ToString();
        //Т.к эта башня ещё не куплена, то надо вывести ещё стоимость
        Canva.DrawString("Cost: " + GameObj.TowerParamsForBuilding[GameObj.TowerConfSelectedID].UpgradeParams[0].Cost,
          new Font("Arial", Settings.ElemSize * GameObj.Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * GameObj.Scaling) + 5, Convert.ToInt32(390 * GameObj.Scaling)));
      }
      if (GameObj.TowerMapSelectedID != -1)//Если выводим информацию о поставленной башне
      {
        StrToShow = GameObj.Towers[GameObj.TowerMapSelectedID].ToString();//Строка вывода
        //Иконка башни
        Canva.DrawImage(GameObj.Towers[GameObj.TowerMapSelectedID].Icon,
          Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * GameObj.Scaling) + 5, Convert.ToInt32(375 * GameObj.Scaling),
          GameObj.Towers[GameObj.TowerMapSelectedID].Icon.Width * GameObj.Scaling, GameObj.Towers[GameObj.TowerMapSelectedID].Icon.Height * GameObj.Scaling);
        //Выводим текущий уровень башни
        Canva.DrawString("Level: " + GameObj.Towers[GameObj.TowerMapSelectedID].Level.ToString(),
          new Font("Arial", Settings.ElemSize * GameObj.Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + GameObj.Towers[GameObj.TowerMapSelectedID].Icon.Width + Settings.DeltaX * 2) * GameObj.Scaling) + 5,
            Convert.ToInt32((375 + GameObj.Towers[GameObj.TowerMapSelectedID].Icon.Height / 2) * GameObj.Scaling)));
        //Кнопки Destroy и Upgrade
        BDestroyShow(GameObj, Canva);
        BUpgradeShow(GameObj, Canva);
      }
      //Характеристики
      Canva.DrawString(StrToShow,
          new Font("Arial", 10 * GameObj.Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * GameObj.Scaling) + 5, Convert.ToInt32(415 * GameObj.Scaling)));
      //Рамка для красоты
      Canva.DrawRectangle(new Pen(Color.Black), Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * GameObj.Scaling) + 5,
        Convert.ToInt32(415 * GameObj.Scaling), Convert.ToInt32((200 - Settings.DeltaX * 2) * GameObj.Scaling),
        Convert.ToInt32((184) * GameObj.Scaling));
    }

    /// <summary>
    /// Вывод квадрата и радиуса атаки вокруг установленой/пытающейся установиться башни
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    /// <param name="Position">Левый верхний квадрат для башни</param>
    /// <param name="Radius">Радиус атаки</param>
    /// <param name="CircleColor">Цвет круга</param>
    private void ShowSquareAndCircleAtTower(TGame GameObj, Graphics Canva, Point Position, int Radius, Color CircleColor)
    {
      //Квадрат
      Canva.DrawRectangle(new Pen(Color.Black), ((Position.X) * Settings.ElemSize) * GameObj.Scaling + Settings.DeltaX,
          ((Position.Y) * Settings.ElemSize) * GameObj.Scaling + Settings.DeltaY, Settings.ElemSize * 2 * GameObj.Scaling, Settings.ElemSize * 2 * GameObj.Scaling);
      //Радиус атаки
      Canva.DrawEllipse(new Pen(CircleColor), ((Position.X + 1) * Settings.ElemSize - Radius) * GameObj.Scaling + Settings.DeltaX,
          ((Position.Y + 1) * Settings.ElemSize - Radius) * GameObj.Scaling + Settings.DeltaY, Radius * 2 * GameObj.Scaling, Radius * 2 * GameObj.Scaling);
    }
    #endregion

    #region Вывод кнопочек
    /// <summary>
    /// Показ кнопки начать новый уровень
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void BStartLevelShow(TGame GameObj, Graphics Canva)
    {
      if (GameObj.LevelStarted)
      {
        Canva.DrawImage(Res.BStartLevelDisabled, THelpers.BuildRect(RectBuilder.NewLevelDisabled, GameObj.Scaling));
      }
      else
      {
        Canva.DrawImage(Res.BStartLevelEnabled, THelpers.BuildRect(RectBuilder.NewLevelEnabled, GameObj.Scaling));
      }
    }

    /// <summary>
    /// Уничтожить башню
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void BDestroyShow(TGame GameObj, Graphics Canva)
    {
      Canva.DrawImage(Res.BDestroyTower, THelpers.BuildRect(RectBuilder.Destroy, GameObj.Scaling));
    }

    /// <summary>
    /// Показ кнопки улучшить башню
    /// </summary>
    /// <param name="Canva"></param>
    private void BUpgradeShow(TGame GameObj, Graphics Canva)
    {
      if (GameObj.Towers[GameObj.TowerMapSelectedID].CanUpgrade)
      {
        //Вводится Tmp, т.к этот прямоугольник будет использоваться три раза
        Rectangle Tmp = THelpers.BuildRect(RectBuilder.Upgrade, GameObj.Scaling);
        Canva.DrawImage(Res.BUpgradeTower, Tmp);
        Canva.DrawString("Upgrade cost: " + GameObj.Towers[GameObj.TowerMapSelectedID].GetUpgradeCost,
          new Font("Arial", Settings.ElemSize * GameObj.Scaling, FontStyle.Italic | FontStyle.Bold), new SolidBrush(Color.Black),
          new Point(Convert.ToInt32((Settings.MapAreaSize + Settings.DeltaX * 2) * GameObj.Scaling) + 3, Tmp.Y - Convert.ToInt32(25 * GameObj.Scaling)));
      }
    }
    #endregion

    #region Информация для игрока
    /// <summary>
    /// Вывод числа жизней
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void ShowLives(TGame GameObj, Graphics Canva)
    {
      Canva.DrawString("Lives: " + GameObj.NumberOfLives.ToString(), new Font("Arial", 14 * GameObj.Scaling), new SolidBrush(Color.Black),
        new Point(Convert.ToInt32((Settings.MapAreaSize + 10 + Settings.DeltaX * 2) * GameObj.Scaling), Convert.ToInt32((Res.MoneyPict.Height + 10) * GameObj.Scaling)));
    }
    /// <summary>
    /// Вывод количества денег 
    /// </summary>
    /// <param name="Canva">Область для рисования</param>
    private void ShowMoney(TGame GameObj, Graphics Canva)
    {
      //Изображение монеты
      Canva.DrawImage(Res.MoneyPict, Convert.ToInt32((Settings.MapAreaSize + 10 + Settings.DeltaX * 2) * GameObj.Scaling),
        Convert.ToInt32(5 * GameObj.Scaling), Res.MoneyPict.Width * GameObj.Scaling, Res.MoneyPict.Height * GameObj.Scaling);
      //Вывод числа денег
      Canva.DrawString(GameObj.Gold.ToString(), new Font("Arial", 14 * GameObj.Scaling), new SolidBrush(Color.Black),
        new Point(Convert.ToInt32((Settings.MapAreaSize + 10 + Res.MoneyPict.Width + Settings.DeltaX * 2) * GameObj.Scaling), Convert.ToInt32(9 * GameObj.Scaling)));
    }
    #endregion
  }
}
