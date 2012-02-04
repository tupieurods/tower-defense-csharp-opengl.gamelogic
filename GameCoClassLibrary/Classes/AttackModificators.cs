using System;

namespace GameCoClassLibrary
{
  public enum eModificatorName { NoEffect, Freeze, Burn, Posion };

  abstract public class TAttackModificators
  {
    //Коэффициент уменьшения скорости
    //И величиниа на которую будет изменено здоровье юнита
    //И величина на которую будет изменена броня
    protected int DSpeed = 1;
    protected int DHealth = 0;
    protected int DArmor = 0;
    //Сколько ещё будет действовать эффект и максимальная
    //его длительность в игровых тактах
    protected int CurrentDuration = 50;
    protected int MaxDuration = 50;
    public bool DestroyMe
    {
      get;
      private set;
    }

    public void Reset()//Если ещё раз наложили эффект, когда он ещё действует
    {
      CurrentDuration = MaxDuration;
    }

    public static TAttackModificators CreateEffectByID(eModificatorName Name)//Создание эффекта
    {
      switch (Name)
      {
        case eModificatorName.NoEffect:
          return null;
        case eModificatorName.Freeze:
          return new TFreezeModificator(2);
        case eModificatorName.Burn:
          return new TBurningModificator(2);
        case eModificatorName.Posion:
          return new TPosionModificator(2, 10);
      }
      return null;
    }

    abstract public void DoEffect(ref int Speed, ref int Health, ref int Armor);//Воздействие эффекта
  }

  public class TFreezeModificator : TAttackModificators
  {
    public TFreezeModificator(int FreezeMultiple)
    {
      this.DSpeed = FreezeMultiple;
    }
    public override void DoEffect(ref int Speed, ref int Health, ref int Armor)
    {
      Speed = Speed / DSpeed;
    }
  }

  public class TBurningModificator : TAttackModificators
  {
    public TBurningModificator(int BurnDamadge)
    {
      this.DHealth = BurnDamadge;
    }
    public override void DoEffect(ref int Speed, ref int Health, ref int Armor)
    {
      Speed = Speed / DSpeed;
    }
  }

  public class TPosionModificator : TAttackModificators
  {
    public TPosionModificator(int FreezeMultiple, int PosionDamadge)
    {
      this.DSpeed = FreezeMultiple;
      this.DHealth = PosionDamadge;
    }
    public override void DoEffect(ref int Speed, ref int Health, ref int Armor)
    {
      Speed = Speed / DSpeed;
    }
  }
}