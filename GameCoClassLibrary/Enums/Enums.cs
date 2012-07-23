namespace GameCoClassLibrary.Enums
{
  //public enum ProcAction { Show, Select };
  internal enum ShopActStatus
  {
    Normal, //Nothing to do
    ShopActFinish,
    MapActFinish
  };

  public enum MonsterDirection { Up, Right, Down, Left };
  public enum eTowerType { Simple, Splash };
  public enum MapElemStatus { CanMove, CanBuild, BusyByUnit, BusyByTower };
  public enum eModificatorName { NoEffect, Freeze, Burn, Posion };
  public enum FormType { GameConfiguration, Load };
  public enum FactoryAct { Create, Load };
  //public enum GraphicEngineType { WinForms, OpenGL, SharpDX };

  public enum Button
  {
    Empty,
    StartLevelEnabled,
    StartLevelDisabled,
    DestroyTower,
    UpgradeTower,
    BigScale,
    NormalScale,
    SmallScale,
    Exit,
    LoadGame,
    SaveGame,
    Pause,
    Unpause,
    NewGame,
    Menu,
    Back
  };
}
