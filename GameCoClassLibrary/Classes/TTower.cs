using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GameCoClassLibrary;

namespace GameCoClassLibrary
{
  class TTower
  {
    #region Private
    private readonly TowerParam Params;//Параметры, получаемые от игры
    private Bitmap ScaledTowerPict;//Хранит перемасштабированное изображение башни на карте
    private sMainTowerParam _CurrentTowerParams;//Отображает текущее состояние вышки
    private int WasCrit = 0;//Показывает что был совершён критический выстрел и нужно показать
    //Это игроку, полностью работает, но код реализующий этот функционал
    //отключён за ненадобностью(Нет проверки на возможность вышкой выстрелить в несколько целей за раз)
    #endregion

    #region Public
    public float Scaling//О правильности масштабирования позаботится класс TGame
    {
      get;
      set;
    }
    public Point ArrayPos//Позиция на карте(левая верхняя клетка башни на карте)
    {
      get;
      private set;
    }
    public sMainTowerParam CurrentTowerParams//Текущие параметры вышки
    {
      get
      {
        return _CurrentTowerParams;
      }
    }
    public Bitmap Icon
    {
      get
      {
        return new Bitmap(Params.Icon);
      }
    }
    public int Level
    {
      get;
      private set;
    }
    public bool CanUpgrade
    {
      get;
      private set;
    }
    public string GetUpgradeCost
    {
      get
      {
        if (Params.UnlimitedUp)//Бесконечное обновление
        {
          return Params.UpgradeParams[1].Cost.ToString();
        }
        else
        {
          return Params.UpgradeParams[Level].Cost.ToString();
        }
      }
    }
    #endregion

    public TTower(TowerParam Params, Point ArrayPos, float Scaling = 1F)
    {
      this.Params = Params;
      this.ArrayPos = new Point(ArrayPos.X, ArrayPos.Y);
      this.Scaling = Scaling;
      Level = 1;
      _CurrentTowerParams = this.Params.UpgradeParams[Level - 1];
      CanUpgrade = this.Params.UpgradeParams.Count > 1;
      _CurrentTowerParams.Cooldown = 0;
      _CurrentTowerParams.Picture.MakeTransparent(Color.FromArgb(255, 0, 255));
    }

    public void ShowTower(Graphics Canva, Point VisibleStart, Point VisibleFinish, int DX = 10, int DY = 10)
    {
      //Проверка, видима ли вышка
      bool Flag = true;
      //if ((ArrayPos.Y >= VisibleFinish.Y) || (ArrayPos.X >= VisibleFinish.X))
      if (!(((ArrayPos.X + 1) * 15/* - CurrentTowerParams.AttackRadius */< VisibleFinish.X * 15) ||
        ((ArrayPos.Y + 1) * 15/* - CurrentTowerParams.AttackRadius */< VisibleFinish.Y * 15)))//Если не видна логически, но видна графически
        Flag = false;
      //if ((Flag)&&((ArrayPos.X < (VisibleStart.X-1)) || (ArrayPos.Y < (VisibleStart.Y-1))))
      if ((Flag) && (!(((ArrayPos.X + 1) * 15/* + CurrentTowerParams.AttackRadius */> VisibleStart.X * 15) ||
        ((ArrayPos.Y + 1) * 15/* + CurrentTowerParams.AttackRadius */> VisibleStart.Y * 15))))
        Flag = false;
      if (Flag)
      {
        Canva.DrawImage(CurrentTowerParams.Picture, (-(CurrentTowerParams.Picture.Width / 2) + ((ArrayPos.X + 1 - VisibleStart.X) * 15)) * Scaling + DX,
          (-(CurrentTowerParams.Picture.Height / 2) + ((ArrayPos.Y + 1 - VisibleStart.Y) * 15)) * Scaling + DY,
          CurrentTowerParams.Picture.Width * Scaling, CurrentTowerParams.Picture.Height * Scaling);
        if (WasCrit!=0)
        {
          WasCrit--;
          Canva.DrawString((CurrentTowerParams.Damage * CurrentTowerParams.CritMultiple).ToString(),
            new Font("Arial", 20), new SolidBrush(Color.Red),
            (-(CurrentTowerParams.Picture.Width / 2) + ((ArrayPos.X + 1 - VisibleStart.X) * 15)) * Scaling + DX,
          (-(CurrentTowerParams.Picture.Height / 2) + ((ArrayPos.Y + 1 - VisibleStart.Y) * 15)) * Scaling + DY);
        }
      }
    }

    public bool Contain(Point ArrPos)
    {
      //Предполагается что башня занимает квадрат 2x2
      for (int dx = 0; dx < 2; dx++)
        for (int dy = 0; dy < 2; dy++)
          if (((ArrayPos.X + dx) == ArrPos.X) && ((ArrayPos.Y + dy) == ArrPos.Y))
            return true;
      return false;
    }

    public override string ToString()
    {
      return Params.ToString() + CurrentTowerParams.ToString();
    }

    //Функция улучшения башни, вызывается только если башню можно улучшить ещё
    public int Upgrade()
    {
      int UpCost = 0;
      Level++;
      if (Params.UnlimitedUp)//Бесконечное обновление
      {
        _CurrentTowerParams.AttackRadius += Params.UpgradeParams[1].AttackRadius;
        //_CurrentTowerParams.Cooldown -= Params.UpgradeParams[1].Cooldown;
        if (_CurrentTowerParams.Cooldown < 0)
          _CurrentTowerParams.Cooldown = 0;
        _CurrentTowerParams.Damage += Params.UpgradeParams[1].Damage;
        _CurrentTowerParams.Cost = Params.UpgradeParams[1].Cost;
        UpCost = _CurrentTowerParams.Cost;
      }
      else
      {
        if (Level == Params.UpgradeParams.Count)//Ограниченное обновление
          CanUpgrade = false;
        int Tmp = _CurrentTowerParams.Cooldown;//Чтобы не сбрасывался откат атаки при обновлении
        _CurrentTowerParams = Params.UpgradeParams[Level - 1];
        _CurrentTowerParams.Cooldown = Tmp;
        //Так будет до тех пор, пока не будет сделана своя картинка для каждого уровня
        _CurrentTowerParams.Picture = Params.UpgradeParams[0].Picture;
        UpCost = _CurrentTowerParams.Cost;
      }
      return UpCost;
    }

    public IEnumerable<TMissle> GetAims(IEnumerable<TMonster> Monsters)
    {
      //List<TMissle> Result = new List<TMissle>();
      _CurrentTowerParams.Cooldown = _CurrentTowerParams.Cooldown == 0 ? 0 : --_CurrentTowerParams.Cooldown;
      if ((_CurrentTowerParams.Cooldown) == 0)
      {
        //_CurrentTowerParams.Cooldown = Params.UpgradeParams[Level - 1].Cooldown;
        PointF TowerCenterPos = new PointF((ArrayPos.X + 1) * 15, (ArrayPos.Y + 1) * 15);
        int Count = 0;
        foreach (TMonster Monster in Monsters)
        {
          PointF MonsterPos = Monster.GetCanvaPos;
          if (Math.Sqrt(Math.Pow(MonsterPos.X - TowerCenterPos.X, 2) + Math.Pow(MonsterPos.Y - TowerCenterPos.Y, 2)) <= _CurrentTowerParams.AttackRadius)
          {
            int DamadgeWithCritical = new Random().Next(1, 100) < CurrentTowerParams.CritChance ?
              (int)(CurrentTowerParams.Damage * CurrentTowerParams.CritMultiple) : CurrentTowerParams.Damage;
            if (DamadgeWithCritical != CurrentTowerParams.Damage)
              WasCrit = 10;
            yield return
              new TMissle(Monster.ID, DamadgeWithCritical, Params.TowerType,
                Params.MisslePenColor, Params.MissleBrushColor, Params.Modificator, TowerCenterPos);
            Count++;
            if (Count >= _CurrentTowerParams.NumberOfTargets)
            {
              _CurrentTowerParams.Cooldown = Params.UpgradeParams[Level - 1].Cooldown;
              yield break;
            }
          }
        }
        //if (Count != 0)
      }
      //return Result;
    }
  }
}
