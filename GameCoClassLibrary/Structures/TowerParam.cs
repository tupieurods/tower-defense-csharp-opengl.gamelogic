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

    public override string ToString()
    {
      string Tmp = "\nCost: " + Cost.ToString() + "\nDamadge: " + Damage.ToString()
        + "\nAttack Radius: " + AttackRadius.ToString() + "\nAttack Cooldown: " + Cooldown.ToString() + "\nNumber of Targets: " + NumberOfTargets.ToString();
      if (CritMultiple != 0)
        Tmp = Tmp + "\nCritical Strike Multiple: " + CritMultiple.ToString() + "\nCritical Strike Chance: " + CritChance.ToString();
      return Tmp;
    }
  }

  [Serializable]
  public sealed class TowerParam
  {
    #region Graphics
    public Bitmap Icon;//Иконка для магазина, апгрейдов
    public Color MisslePenColor;
    public Color MissleBrushColor;
    #endregion
    public eTowerType TowerType { get; set; }
    //нулевой элемент- состояние при покупке, если только нулевой элемент обновлять невозможно
    public List<sMainTowerParam> UpgradeParams { get; set; }
    //Если обновление бесконечное
    public bool UnlimitedUp { get; set; }
    public bool TrueSight { get; set; }
    //Эффекты
    public eModificatorName Modificator { get; set; }

    public TowerParam()
    {
      this.Icon = null;
      this.MisslePenColor = Color.Black;
      this.MissleBrushColor = Color.Black;
      this.TowerType = eTowerType.Simple;
      this.UnlimitedUp = false;
      this.TrueSight = false;
      this.UpgradeParams = new List<sMainTowerParam>();
      this.Modificator = eModificatorName.NoEffect;
      this.UpgradeParams.Add(new sMainTowerParam());
      this.UpgradeParams[0] = sMainTowerParam.CreateDefault();
    }

    public override string ToString()
    {
      string Tmp = "Tower Type: " + TowerType.ToString();
      if (TrueSight)//Видит ли невидимых
        Tmp = Tmp + "\nTrue Sight: Yes";
      else
        Tmp = Tmp + "\nTrue Sight: No";
      //Секция модернизации башни
      if (UpgradeParams.Count > 1)
      {
        if (UnlimitedUp)
          Tmp = Tmp + "\nCan be upgraded:\nYes, Unlimited";
        else
          Tmp = Tmp + "\nCan be upgraded:\nYes, Limited(" + (UpgradeParams.Count - 1).ToString() + " Levels)";
        //не буду выпендриваться с форматом, ибо нету смысла
      }
      else
        Tmp = Tmp + "\nCan be upgraded: No";
      //Эффекты
      if (Modificator != eModificatorName.NoEffect)
        Tmp = Tmp + "\nAttack modificator:\n" + Modificator.ToString();
      else
        Tmp = Tmp + "\nAttack modificator:\nNo modifications";

      return Tmp;
    }
  }
}