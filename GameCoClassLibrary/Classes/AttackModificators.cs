﻿using System.IO;
using GameCoClassLibrary.Enums;

namespace GameCoClassLibrary.Classes
{
  //TODO:The baddest part of project, refactor it when OpenGl implementation will be finished
  /// <summary>
  /// Attack modificator class
  /// </summary>
  abstract public class AttackModificators
  {
    /// <summary>
    /// EffectAct delegate. DRY.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="z">The z.</param>
    protected delegate void EffectAct(ref float x, ref int y, ref int z);

    /// <summary>
    /// Delat speed
    /// </summary>
    protected int DSpeed = 1;
    /// <summary>
    /// Delta health
    /// </summary>
    protected int DHealth;
    /// <summary>
    /// Delta armor
    /// </summary>
    protected int DArmor;
    /// <summary>
    /// Current duration in game ticks
    /// </summary>
    protected int CurrentDuration = 50;
    /// <summary>
    /// Maximal duration in game ticks
    /// </summary>
    protected int MaxDuration = 50;
    /// <summary>
    /// Effect Act every CurrentDuration % WorkEverry == 0 ticks
    /// </summary>
    protected int WorkEvery = 1;
    /// <summary>
    /// Gets or sets the color of the effect.
    /// </summary>
    /// <value>
    /// The color of the effect.
    /// </value>
    public System.Drawing.Color EffectColor
    {
      get;
      protected set;
    }
    /// <summary>
    /// Gets or sets a value indicating whether object "should be destroyed".
    /// </summary>
    /// <value>
    ///   <c>true</c> if "should be destroyed"; otherwise, <c>false</c>.
    /// </value>
    public bool DestroyMe
    {
      get;
      protected set;
    }
    /// <summary>
    /// Gets or sets the type of modification.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    public eModificatorName Type
    {
      get;
      protected set;
    }

    /// <summary>
    /// If effect added again, while duration not finished;
    /// </summary>
    public void Reset()
    {
      CurrentDuration = MaxDuration;
    }

    /// <summary>
    /// Reals the do effect.
    /// </summary>
    /// <param name="act">The act.</param>
    /// <param name="speed">The Dspeed.</param>
    /// <param name="health">The Dhealth.</param>
    /// <param name="armor">The Darmor.</param>
    protected void RealDoEffect(EffectAct act, ref float speed, ref int health, ref int armor)
    {
      if (CurrentDuration % WorkEvery == 0)
      {
        act(ref speed, ref health, ref armor);
      }
      CurrentDuration--;
      if (CurrentDuration == 0)
      {
        DestroyMe = true;
      }
    }

    /// <summary>
    /// Factory
    /// </summary>
    /// <param name="name">The name of effect</param>
    /// <param name="duration">Current duration</param>
    /// <returns>Effect object</returns>
    public static AttackModificators CreateEffectByID(eModificatorName name, int duration = 50)
    {
      switch (name)
      {
        case eModificatorName.NoEffect:
          return null;
        case eModificatorName.Freeze:
          return new FreezeModificator(2, duration);
        case eModificatorName.Burn:
          return new BurningModificator(2, 5, duration);
        case eModificatorName.Posion:
          return new PosionModificator(2, 10, 7, duration);
      }
      return null;
    }

    public void Save(BinaryWriter saveStream)
    {
      saveStream.Write((int)Type);
      saveStream.Write(CurrentDuration);
    }

    /// <summary>
    /// Effect impact.
    /// </summary>
    /// <param name="speed">The Dspeed.</param>
    /// <param name="health">The Dhealth.</param>
    /// <param name="armor">The Darmor.</param>
    abstract public void DoEffect(ref float speed, ref int health, ref int armor);
  }

  /// <summary>
  /// Freeze modificator class
  /// </summary>
  public class FreezeModificator : AttackModificators
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="FreezeModificator"/> class.
    /// </summary>
    /// <param name="freezeMultiple">The freeze multiple.</param>
    public FreezeModificator(int freezeMultiple, int duration)
    {
      DSpeed = freezeMultiple;
      EffectColor = System.Drawing.Color.Blue;
      WorkEvery = 1;
      Type = eModificatorName.Freeze;
    }
    /// <summary>
    /// Effect impact.
    /// </summary>
    /// <param name="speed">The Dspeed.</param>
    /// <param name="health">The Dhealth.</param>
    /// <param name="armor">The Darmor.</param>
    public override void DoEffect(ref float speed, ref int health, ref int armor)
    {
      // ReSharper disable InconsistentNaming
      RealDoEffect(delegate(ref float Speed, ref int Health, ref int Armor)
                     // ReSharper restore InconsistentNaming
                     {
                       Speed = Speed / DSpeed;
                     }, ref speed, ref health, ref armor);
    }
  }

  /// <summary>
  /// Burning modificator class
  /// </summary>
  public class BurningModificator : AttackModificators
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="BurningModificator"/> class.
    /// </summary>
    /// <param name="burnDamadge">The burn damadge.</param>
    /// <param name="period">The period.</param>
    public BurningModificator(int burnDamadge, int period, int duration)
    {
      DHealth = burnDamadge;
      EffectColor = System.Drawing.Color.Red;
      WorkEvery = period;
      Type = eModificatorName.Burn;
    }
    /// <summary>
    /// Effect impact.
    /// </summary>
    /// <param name="speed">The Dspeed.</param>
    /// <param name="health">The Dhealth.</param>
    /// <param name="armor">The Darmor.</param>
    public override void DoEffect(ref float speed, ref int health, ref int armor)
    {
      // ReSharper disable InconsistentNaming
      RealDoEffect(delegate(ref float Speed, ref int Health, ref int Armor)
      // ReSharper restore InconsistentNaming
      {
        Health -= DHealth;
      }, ref speed, ref health, ref armor);
    }
  }

  /// <summary>
  /// Posion modificator class
  /// </summary>
  public class PosionModificator : AttackModificators
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PosionModificator"/> class.
    /// </summary>
    /// <param name="freezeMultiple">The freeze multiple.</param>
    /// <param name="posionDamadge">The posion damadge.</param>
    /// <param name="period">The period.</param>
    public PosionModificator(int freezeMultiple, int posionDamadge, int period, int duration)
    {
      DSpeed = freezeMultiple;
      DHealth = posionDamadge;
      EffectColor = System.Drawing.Color.Lime;
      WorkEvery = period;
      Type = eModificatorName.Posion;
    }
    /// <summary>
    /// Effect impact.
    /// </summary>
    /// <param name="speed">The Dspeed.</param>
    /// <param name="health">The Dhealth.</param>
    /// <param name="armor">The Darmor.</param>
    public override void DoEffect(ref float speed, ref int health, ref int armor)
    {
      // ReSharper disable InconsistentNaming
      RealDoEffect(delegate(ref float Speed, ref int Health, ref int Armor)
      // ReSharper restore InconsistentNaming
      {
        Health -= DHealth;
      }, ref speed, ref health, ref armor);
      speed = speed / DSpeed;//TODO один из костылей, проблема в архитектуре эффектов, после перехода на OpenGl убрать
    }
  }
}