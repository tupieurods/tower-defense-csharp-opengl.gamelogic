using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using GameCoClassLibrary.Enums;

namespace GameCoClassLibrary.Structures
{
  /// <summary>
  /// Main tower params, mutable
  /// </summary>
  [Serializable]
  public struct sMainTowerParam
  {
    /// <summary>
    /// Cost
    /// </summary>
    public int Cost;
    /// <summary>
    /// Damage
    /// </summary>
    public int Damage;
    /// <summary>
    /// Attack Radius
    /// </summary>
    public int AttackRadius;
    /// <summary>
    /// Delay between attacks
    /// </summary>
    public int Cooldown;
    /// <summary>
    /// Number of tower targets, may be in future that will change with level changing 
    /// </summary>
    public int NumberOfTargets;
    /// <summary>
    /// Critical strike multiplie, may be in future that will change with level changing
    /// no crit if zero
    /// </summary>
    public double CritMultiple;
    /// <summary>
    /// Critical strike chance, may be in future that will change with level changing 
    /// no crit if zero
    /// </summary>
    public byte CritChance;
    /// <summary>
    /// Image to be shown on the map, may be in future that will change with level changing 
    /// </summary>
    public Bitmap Picture;

    /// <summary>
    /// Creates the default.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      string tmp = "\nDamadge: " + Damage.ToString(CultureInfo.InvariantCulture)
        + "\nAttack Radius: " + AttackRadius.ToString(CultureInfo.InvariantCulture) + "\nAttack Cooldown: " +
        Cooldown.ToString(CultureInfo.InvariantCulture) + "\nNumber of Targets: " + NumberOfTargets.ToString(CultureInfo.InvariantCulture);
      if (CritMultiple > 0.001)
        tmp = tmp + "\nCritical Strike Multiple: " + CritMultiple.ToString(CultureInfo.InvariantCulture) +
          "\nCritical Strike Chance: " + CritChance.ToString(CultureInfo.InvariantCulture);
      return tmp;
    }
  }

  /// <summary>
  /// Tower Params, constant.
  /// </summary>
  [Serializable]
  public sealed class TowerParam
  {
    #region Graphics
    /// <summary>
    /// Icon for shop
    /// </summary>
    public Bitmap Icon;
    /// <summary>
    /// Missle pen color
    /// </summary>
    public Color MisslePenColor;
    /// <summary>
    /// Missle brush color
    /// </summary>
    public Color MissleBrushColor;
    #endregion
    /// <summary>
    /// Gets or sets the type of the tower.
    /// </summary>
    /// <value>
    /// The type of the tower.
    /// </value>
    public eTowerType TowerType { get; set; }
    /// <summary>
    /// Gets or sets the upgrade params.
    /// [0]-after construction
    /// if count==1 No upgrading
    /// if count>1 and UnlimitedUp==false, it's Limited upgrading
    /// </summary>
    /// <value>
    /// The upgrade params.
    /// </value>
    public List<sMainTowerParam> UpgradeParams { get; set; }
    /// <summary>
    /// Can player or not upgrade this tower forever.
    /// </summary>
    /// <value>
    ///   <c>true</c> if can; otherwise, <c>false</c>.
    /// </value>
    public bool UnlimitedUp { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether true sight.
    /// </summary>
    /// <value>
    ///   <c>true</c> if have true sight; otherwise, <c>false</c>.
    /// </value>
    public bool TrueSight { get; set; }
    /// <summary>
    /// Gets or sets the modificator.
    /// </summary>
    /// <value>
    /// The modificator.
    /// </value>
    public eModificatorName Modificator { get; set; }

    public TowerParam()
    {
      Icon = null;
      MisslePenColor = Color.Black;
      MissleBrushColor = Color.Black;
      TowerType = eTowerType.Simple;
      UnlimitedUp = false;
      TrueSight = false;
      UpgradeParams = new List<sMainTowerParam>();
      Modificator = eModificatorName.NoEffect;
      UpgradeParams.Add(new sMainTowerParam());
      UpgradeParams[0] = sMainTowerParam.CreateDefault();
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      string Tmp = "Tower Type: " + TowerType.ToString();
      if (TrueSight)
        Tmp = Tmp + "\nTrue Sight: Yes";
      else
        Tmp = Tmp + "\nTrue Sight: No";
      //Tower upgrading
      if (UpgradeParams.Count > 1)
      {
        if (UnlimitedUp)
          Tmp = Tmp + "\nCan be upgraded:\nYes, Unlimited";
        else
          Tmp = Tmp + "\nCan be upgraded:\nYes, Limited(" + (UpgradeParams.Count - 1).ToString(CultureInfo.InvariantCulture) + " Levels)";
      }
      else
        Tmp = Tmp + "\nCan be upgraded: No";
      //attack modificators
      if (Modificator != eModificatorName.NoEffect)
        Tmp = Tmp + "\nAttack modificator:\n" + Modificator;
      else
        Tmp = Tmp + "\nAttack modificator:\nNo modifications";
      return Tmp;
    }
  }
}