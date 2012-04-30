﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Structures;
using GameCoClassLibrary.Interfaces;

namespace GameCoClassLibrary.Classes
{
  internal class Tower
  {
    #region Private

    private readonly TowerParam _params;//Параметры, получаемые от игры
    //private Bitmap ScaledTowerPict;//Хранит перемасштабированное изображение башни на карте

    private readonly string _confHash;

    private sMainTowerParam _currentTowerParams;//Отображает текущее состояние вышки
    private readonly Point _towerCenterPos;//Small optimization and DRY
    private int _wasCrit;//Показывает что был совершён критический выстрел и нужно показать
    //Это игроку, полностью работает, но код реализующий этот функционал
    //отключён за ненадобностью(Нет проверки на возможность вышкой выстрелить в несколько целей за раз)
    private int _currentMaxCooldown;//Та часть кода, который бы лучше не было
    //дело в том, что первоначально брался Cooldown из Params, но для бесконечно обновляющейся вышки это не прокатывает
    //т.к возможно время сброса атаки уменьшается(повышается) с ростом уровня
    //В CurrentTowerParams хранится реальный Cooldown(зависящий от времени последнего выстрела)
    //Так что и получается что где-то нужно хранить Cooldown, придётся подпирать костылём(если есть решение лучше с удовольствием приму его)

    #endregion Private

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
        return _currentTowerParams;
      }
    }

    public Bitmap Icon
    {
      get
      {
        return new Bitmap(_params.Icon);
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
        return _params.UnlimitedUp ?
          _params.UpgradeParams[1].Cost.ToString(CultureInfo.InvariantCulture) :
          _params.UpgradeParams[Level].Cost.ToString(CultureInfo.InvariantCulture);
      }
    }

    public bool TrueSight
    {
      get { return _params.TrueSight; }
    }

    #endregion Public

    public Tower(TowerParam Params, Point arrayPos, string confHash, float scaling = 1F)
    {
      _params = Params;
      ArrayPos = new Point(arrayPos.X, arrayPos.Y);
      Scaling = scaling;
      Level = 1;
      _confHash = string.Copy(confHash);
      _currentTowerParams = _params.UpgradeParams[Level - 1];
      CanUpgrade = _params.UpgradeParams.Count > 1;
      _currentTowerParams.Cooldown = 0;
      _currentMaxCooldown = _params.UpgradeParams[0].Cooldown;
      _towerCenterPos = new Point((ArrayPos.X + 1) * Settings.ElemSize, (ArrayPos.Y + 1) * Settings.ElemSize);
      _currentTowerParams.Picture.MakeTransparent(Color.FromArgb(255, 0, 255));
    }

    internal static Tower Factory(FactoryAct act, TowerParam Params, Point arrayPos, string confHash, float scaling = 1F,BinaryReader loadStream=null)
    {
      try
      {
        Tower result=new Tower(Params, arrayPos, confHash, scaling);
        switch (act)
        {
          case FactoryAct.Create:
            break;
          case FactoryAct.Load:
            result.Load(loadStream);
            break;
          default:
            throw new ArgumentOutOfRangeException("act");
        }
        return result;
      }
      catch(Exception exc)
      {
        throw;
      }
    }

    public void ShowTower(IGraphic canva, Point visibleStart, Point visibleFinish)
    {
      //Проверка, видима ли вышка
      bool flag = (((ArrayPos.X + 1) * Settings.ElemSize/* - CurrentTowerParams.AttackRadius */< visibleFinish.X * Settings.ElemSize) ||
                   ((ArrayPos.Y + 1) * Settings.ElemSize/* - CurrentTowerParams.AttackRadius */< visibleFinish.Y * Settings.ElemSize));
      //if ((ArrayPos.Y >= VisibleFinish.Y) || (ArrayPos.X >= VisibleFinish.X))
      //if ((Flag)&&((ArrayPos.X < (VisibleStart.X-1)) || (ArrayPos.Y < (VisibleStart.Y-1))))
      if ((flag) && (!(((ArrayPos.X + 1) * Settings.ElemSize/* + CurrentTowerParams.AttackRadius */> visibleStart.X * Settings.ElemSize) ||
        ((ArrayPos.Y + 1) * Settings.ElemSize/* + CurrentTowerParams.AttackRadius */> visibleStart.Y * Settings.ElemSize))))
        flag = false;
      if (!flag) return;
      canva.DrawImage(CurrentTowerParams.Picture,
        Convert.ToInt32((-(CurrentTowerParams.Picture.Width / 2) + (ArrayPos.X + 1 - visibleStart.X) * Settings.ElemSize) * Scaling + Settings.DeltaX),
        Convert.ToInt32((-(CurrentTowerParams.Picture.Height / 2) + (ArrayPos.Y + 1 - visibleStart.Y) * Settings.ElemSize) * Scaling + Settings.DeltaY),
        Convert.ToInt32(CurrentTowerParams.Picture.Width * Scaling), Convert.ToInt32(CurrentTowerParams.Picture.Height * Scaling));
      if (_wasCrit == 0) return;
      _wasCrit--;
      canva.DrawString(
        (CurrentTowerParams.Damage * CurrentTowerParams.CritMultiple).ToString(CultureInfo.InvariantCulture),
          new Font("Arial", 20), new SolidBrush(Color.Red),
          new PointF(
            (-(CurrentTowerParams.Picture.Width / 2) + (ArrayPos.X + 1 - visibleStart.X) * Settings.ElemSize) * Scaling + Settings.DeltaX,
            (-(CurrentTowerParams.Picture.Height / 2) + (ArrayPos.Y + 1 - visibleStart.Y) * Settings.ElemSize) * Scaling + Settings.DeltaY));
    }

    public bool Contain(Point arrPos)
    {
      //Предполагается что башня занимает квадрат 2x2
      for (int dx = 0; dx < 2; dx++)
        for (int dy = 0; dy < 2; dy++)
          if (((ArrayPos.X + dx) == arrPos.X) && ((ArrayPos.Y + dy) == arrPos.Y))
            return true;
      return false;
    }

    public override string ToString()
    {
      return _params + CurrentTowerParams.ToString();
    }

    //Функция улучшения башни, вызывается только если башню можно улучшить ещё
    public int Upgrade()
    {
      int upCost;
      Level++;
      if (_params.UnlimitedUp)//Бесконечное обновление
      {
        _currentTowerParams.AttackRadius += _params.UpgradeParams[1].AttackRadius;
        _currentMaxCooldown -= _params.UpgradeParams[1].Cooldown;
        if (_currentMaxCooldown < 0)
          _currentMaxCooldown = 0;
        _currentTowerParams.Damage += _params.UpgradeParams[1].Damage;
        _currentTowerParams.Cost = _params.UpgradeParams[1].Cost;
        upCost = _currentTowerParams.Cost;
      }
      else
      {
        if (Level == _params.UpgradeParams.Count)//Ограниченное обновление
          CanUpgrade = false;
        int tmp = _currentTowerParams.Cooldown;//Чтобы не сбрасывался откат атаки при обновлении
        _currentTowerParams = _params.UpgradeParams[Level - 1];
        _currentTowerParams.Cooldown = tmp;
        _currentMaxCooldown = _params.UpgradeParams[Level - 1].Cooldown;
        //Так будет до тех пор, пока не будет сделана своя картинка для каждого уровня
        _currentTowerParams.Picture = _params.UpgradeParams[0].Picture;
        upCost = _currentTowerParams.Cost;
      }
      return upCost;
    }

    //Выстрелы по целям
    public IEnumerable<Missle> GetAims(IEnumerable<Monster> monsters)
    {
      _currentTowerParams.Cooldown = _currentTowerParams.Cooldown == 0 ? 0 : --_currentTowerParams.Cooldown;
      if ((CurrentTowerParams.Cooldown) == 0)
      {

        List<int> alreadyAdded = new List<int>(CurrentTowerParams.NumberOfTargets + 1);
        foreach (Monster monster in monsters)
        {
          PointF monsterPos = monster.GetCanvaPos;
          if ((alreadyAdded.Contains(monster.ID)) ||
            !Helpers.UnitInRadius(monsterPos.X, monsterPos.Y, _towerCenterPos.X, _towerCenterPos.Y, CurrentTowerParams.AttackRadius)) continue;
          //Критический урон
          int damadgeWithCritical = Helpers.RandomForCrit.Next(1, 100) <= CurrentTowerParams.CritChance
                                      ? (int)(CurrentTowerParams.Damage * CurrentTowerParams.CritMultiple)
                                      : CurrentTowerParams.Damage;
          //Чтобы не закидать одного и того же юнита, если вышка может иметь несколько целей
          alreadyAdded.Add(monster.ID);
          if (damadgeWithCritical != CurrentTowerParams.Damage)
          {
            _wasCrit = 10;
            yield return
              Missle.Factory(FactoryAct.Create, monster.ID, damadgeWithCritical, _params.TowerType, _params.MissleBrushColor, _params.MisslePenColor, _params.Modificator, _towerCenterPos.X, _towerCenterPos.Y);
          }
          else
          {
            yield return
              Missle.Factory(FactoryAct.Create, monster.ID, damadgeWithCritical, _params.TowerType, _params.MisslePenColor, _params.MissleBrushColor, _params.Modificator, _towerCenterPos.X, _towerCenterPos.Y);
          }
          //Если ещё можно добавить цели
          if (alreadyAdded.Count < CurrentTowerParams.NumberOfTargets)
            continue;
          _currentTowerParams.Cooldown = _currentMaxCooldown;
          yield break;
        }
        if (alreadyAdded.Count != 0)
        {
          _currentTowerParams.Cooldown = _currentMaxCooldown;
        }
      }
    }

    /*[Obsolete("_towerCenterPos имеет индетификатор internal. Вызывайте UnitInRadius вручную, метод оставлен на будущее, для возможности расширения")]*/
    public bool InAttackRadius(float x, float y)
    {
      return Helpers.UnitInRadius(x, y, _towerCenterPos.X, _towerCenterPos.Y, CurrentTowerParams.AttackRadius);
    }

    public void Save(BinaryWriter saveStream)
    {
      saveStream.Write(_confHash);//хэш файла конфигурации
      //Позиция в массиве
      saveStream.Write(ArrayPos.X);
      saveStream.Write(ArrayPos.Y);
      saveStream.Write(_wasCrit);//был ли крит
      saveStream.Write(Level);
    }

    /// <summary>
    /// Непосредственно кроме загрузки ещё приводит вышку к требуемогу уровню
    /// </summary>
    /// <param name="loadStream"></param>
    private void Load(BinaryReader loadStream)
    {
      //_confHash должен быть загружен в Game.Load
      //Позиция в массиве загружается в Game.Load
      _wasCrit = loadStream.ReadInt32();
      int goToLevel = loadStream.ReadInt32();
      for(int i=1; i<goToLevel; i++)
      {
        Upgrade();
      }
    }
  }
}