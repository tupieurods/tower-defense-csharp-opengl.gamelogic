namespace GameCoClassLibrary.Enums
{
  public enum ProcAction { Show, Select };
  public enum RectBuilder { NewLevelEnabled, NewLevelDisabled, Destroy, Upgrade };
  public enum MonsterDirection { Up, Right, Down, Left };
  public enum eTowerType { Simple, Splash };
  public enum MapElemStatus { CanMove, CanBuild, BusyByUnit, BusyByTower };
  public enum eModificatorName { NoEffect, Freeze, Burn, Posion };
  public enum FormType { GameConfiguration, Load };
  public enum FactoryAct { Create, Load };
  public enum GraphicEngineType { WinForms, OpenGL, SharpDX };
}
