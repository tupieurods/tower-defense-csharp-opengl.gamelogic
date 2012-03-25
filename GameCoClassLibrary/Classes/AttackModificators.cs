using GameCoClassLibrary.Enums;

namespace GameCoClassLibrary.Classes
{
  //TODO:The baddest part of project, refactor it when OpenGl implementation will be finished
  abstract public class AttackModificators
  {
    protected delegate void EffectAct(ref float x, ref int y, ref int z);

    //Коэффициент уменьшения скорости
    //И величиниа на которую будет изменено здоровье юнита
    //И величина на которую будет изменена броня
    protected int DSpeed = 1;
    protected int DHealth;
    protected int DArmor;
    //Сколько ещё будет действовать эффект и максимальная
    //его длительность в игровых тактах
    protected int CurrentDuration = 50;
    protected int MaxDuration = 50;
    //Срабатывать каждый кратный такт
    protected int WorkEvery = 1;
    public System.Drawing.Color EffectColor
    {
      get;
      protected set;
    }
    public bool DestroyMe
    {
      get;
      protected set;
    }

    public eModificatorName Type
    {
      get;
      protected set;
    }

    public void Reset()//Если ещё раз наложили эффект, когда он ещё действует
    {
      CurrentDuration = MaxDuration;
    }

    protected void RealDoEffect(EffectAct act, ref float speed, ref int health, ref int armor)//DRY
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

    public static AttackModificators CreateEffectByID(eModificatorName name)//Создание эффекта
    {
      switch (name)
      {
        case eModificatorName.NoEffect:
          return null;
        case eModificatorName.Freeze:
          return new FreezeModificator(2);
        case eModificatorName.Burn:
          return new BurningModificator(2, 5);
        case eModificatorName.Posion:
          return new PosionModificator(2, 10, 7);
      }
      return null;
    }

    abstract public void DoEffect(ref float speed, ref int health, ref int armor);//Воздействие эффекта
  }

  public class FreezeModificator : AttackModificators
  {
    public FreezeModificator(int freezeMultiple)
    {
      DSpeed = freezeMultiple;
      EffectColor = System.Drawing.Color.Blue;
      WorkEvery = 1;
      Type = eModificatorName.Freeze;
    }
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

  public class BurningModificator : AttackModificators
  {
    public BurningModificator(int burnDamadge, int period)
    {
      DHealth = burnDamadge;
      EffectColor = System.Drawing.Color.Red;
      WorkEvery = period;
      Type = eModificatorName.Burn;
    }
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

  public class PosionModificator : AttackModificators
  {
    public PosionModificator(int freezeMultiple, int posionDamadge, int period)
    {
      DSpeed = freezeMultiple;
      DHealth = posionDamadge;
      EffectColor = System.Drawing.Color.Lime;
      WorkEvery = period;
      Type = eModificatorName.Posion;
    }
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