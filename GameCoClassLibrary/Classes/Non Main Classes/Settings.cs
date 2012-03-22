namespace GameCoClassLibrary.Classes
{
  static public class Settings
  {
    public const int ElemSize = 16;
    static internal int MapAreaSize = ElemSize * 30;
    static internal int DeltaX = 10;//Отступы от левого верхнего края для карты
    static internal int DeltaY = 10;
    static internal int LinesInOnePage = 3;//Максимальное число строк в странице магазина
    static internal int MaxTowersInLine = 5;//Максимально число башен в строке магазина
    static internal int PenWidth = 3;//Толщина пера для полоски жизней
  }
}