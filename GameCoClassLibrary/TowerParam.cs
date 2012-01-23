using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GameCoClassLibrary
{
  public enum eTowerType { Simple, Splash };

  [Serializable]
  public struct sMainTowerParam//Почему не TowerUpParam
  //Чтобы в классе TTower не городить отдельных членов
  {
    public int Cost;
    public int Damage;
    public int AttackRadius;
    public int Cooldown;
    public int NumberOfTargets;//Заключено сюда, если в будущем понадобится 
    //изменение числа целей при улучшении не пришлось терять предыдущие наработки
    //Critical strike добавлен сюда по той же причине.
    public double CritMultiple;//если 0, то нет возможность критовать
    public byte CritChance;//если 0, то нет возможность критовать
    public Bitmap Picture;//Изображение на поле
    //Введено сюда, чтобы если в будущем у уровней могут быть разные изображения на поле
    //пришлось переписывать меньше кода

    public static sMainTowerParam CreateDefault()
    {
      sMainTowerParam Result;
      Result.Cost = 40;
      Result.Damage = 50;
      Result.AttackRadius = 100;
      Result.NumberOfTargets = 1;
      Result.CritMultiple = 0;
      Result.CritChance = 0;
      Result.Cooldown = 45;
      Result.Picture = null;
      return Result;
    }
  }

  [Serializable]
  public struct sTowerParam
  {
    #region Graphics
    public Bitmap Icon;//Иконка для магазина, апгрейдов
    public Color MisslePenColor;
    public Color MissleBrushColor;
    #endregion
    public eTowerType TowerType;
    //нулевой элемент- состояние при покупке, если только нулевой элемент обновлять невозможно
    public List<sMainTowerParam> UpgradeParams;
    //Если обновление бесконечное
    public bool UnlimitedUp;
    public bool TrueSight;
    public int TowerLevel;
    //Эффекты
    public eModificatorName Modificator;

    public static sTowerParam CreateDefault()
    {
      sTowerParam Result;
      Result.Icon = null;
      Result.MisslePenColor = Color.Black;
      Result.MissleBrushColor = Color.Black;
      Result.TowerType = eTowerType.Simple;
      Result.UnlimitedUp = false;
      Result.TrueSight = false;
      Result.TowerLevel = 1;
      Result.UpgradeParams = new List<sMainTowerParam>();
      Result.Modificator = eModificatorName.NoEffect;
      Result.UpgradeParams.Add(new sMainTowerParam());
      Result.UpgradeParams[0] = sMainTowerParam.CreateDefault();
      return Result;
    }
  }
}