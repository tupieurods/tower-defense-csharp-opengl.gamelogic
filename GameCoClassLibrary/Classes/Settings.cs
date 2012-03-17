using System;


namespace GameCoClassLibrary
{
  static public class Settings
  {
    public const int ElemSize = 16;
    static internal int MapAreaSize = Settings.ElemSize * 30;
    static internal int DeltaX = 10;//Отступы от левого верхнего края для карты
    static internal int DeltaY = 10;
  }
}
