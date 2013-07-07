using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Structures;
using GraphicLib.Interfaces;

namespace GameCoClassLibrary.Classes
{
  internal class Tower
  {
    #region Private

    /// <summary>
    /// Tower params from file, which game loaded
    /// </summary>
    private readonly TowerParam _params;

    /// <summary>
    /// Scaled HiRes tower picture, not implemented yet
    /// </summary>
    //private Bitmap ScaledTowerPict;
    /// <summary>
    /// Hash of tower configuration
    /// </summary>
    private readonly string _confHash;

    /// <summary>
    /// Current tower params
    /// </summary>
    private sMainTowerParam _currentTowerParams;

    /// <summary>
    /// Canva X,Y tower center position
    /// </summary>
    private readonly Point _towerCenterPos;

    /// <summary>
    /// Showing to player that hit was critical
    /// </summary>
    private int _wasCrit;

    /// <summary>
    /// Current level maximal attack cooldown
    /// </summary>
    private int _currentMaxCooldown;

    #endregion Private

    #region Internal

    /// <summary>
    /// Gets the array pos.
    /// 
    /// </summary>
    internal Point ArrayPos { get; private set; }

    /// <summary>
    /// Gets the current tower params.
    /// </summary>
    internal sMainTowerParam CurrentTowerParams
    {
      get { return _currentTowerParams; }
    }

    /// <summary>
    /// Gets the tower icon.
    /// </summary>
    internal Bitmap Icon
    {
      get { return new Bitmap(_params.Icon); }
    }

    /// <summary>
    /// Gets the tower level.
    /// </summary>
    internal int Level { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance can upgrade.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance can upgrade; otherwise, <c>false</c>.
    /// </value>
    internal bool CanUpgrade { get; private set; }

    /// <summary>
    /// Gets the get upgrade cost.
    /// </summary>
    internal string GetUpgradeCost
    {
      get
      {
        return _params.UnlimitedUp
                 ? _params.UpgradeParams[1].Cost.ToString(CultureInfo.InvariantCulture)
                 : _params.UpgradeParams[Level].Cost.ToString(CultureInfo.InvariantCulture);
      }
    }

    /// <summary>
    /// Gets a value indicating whether true sight.
    /// </summary>
    /// <value>
    ///   <c>true</c> if true sight tower can see invisible units; otherwise, <c>false</c>.
    /// </value>
    internal bool TrueSight
    {
      get { return _params.TrueSight; }
    }

    #endregion Internal

    /// <summary>
    /// Gets or sets the scaling.
    /// </summary>
    /// <value>
    /// The scaling.
    /// </value>
    internal static float Scaling { private get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tower"/> class.
    /// </summary>
    /// <param name="Params">The params.</param>
    /// <param name="arrayPos">The array pos.</param>
    /// <param name="confHash">The conf hash.</param>
    /// <param name="scaling">The scaling.</param>
    private Tower(TowerParam Params, Point arrayPos, string confHash, float scaling = 1F)
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

    /// <summary>
    /// Loads tower from the file
    /// </summary>
    /// <param name="loadStream">The load stream.</param>
    private void Load(BinaryReader loadStream)
    {
      //_confHash loads in Game.Load
      //Array position loads in Game.Load
      _currentTowerParams.Cooldown = loadStream.ReadInt32();
      _wasCrit = loadStream.ReadInt32();
      int goToLevel = loadStream.ReadInt32();
      for(int i = 1; i < goToLevel; i++)
      {
        Upgrade();
      }
    }

    /// <summary>
    /// Factories the specified act.
    /// </summary>
    /// <param name="act">The act.</param>
    /// <param name="Params">The params.</param>
    /// <param name="arrayPos">The array pos.</param>
    /// <param name="confHash">The conf hash.</param>
    /// <param name="scaling">The scaling.</param>
    /// <param name="loadStream">The load stream.</param>
    /// <returns></returns>
    internal static Tower Factory(FactoryAct act, TowerParam Params, Point arrayPos, string confHash, float scaling = 1F,
                                  BinaryReader loadStream = null)
    {
      try
      {
        Tower result = new Tower(Params, arrayPos, confHash, scaling);
        switch(act)
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
      catch(Exception)
      {
        //TODO add NLog
        throw;
      }
    }

    /// <summary>
    /// Shows the tower.
    /// </summary>
    /// <param name="canva">The canva.</param>
    /// <param name="visibleStart">The visible start.</param>
    /// <param name="visibleFinish">The visible finish.</param>
    internal void ShowTower(IGraphic canva, Point visibleStart, Point visibleFinish)
    {
      //Checking, is tower visible map area or not
      bool flag = (((ArrayPos.X + 1) * Settings.ElemSize /* - CurrentTowerParams.AttackRadius */
                    < visibleFinish.X * Settings.ElemSize) ||
                   ((ArrayPos.Y + 1) * Settings.ElemSize /* - CurrentTowerParams.AttackRadius */
                    < visibleFinish.Y * Settings.ElemSize));
      //if ((ArrayPos.Y >= VisibleFinish.Y) || (ArrayPos.X >= VisibleFinish.X))
      //if ((Flag)&&((ArrayPos.X < (VisibleStart.X-1)) || (ArrayPos.Y < (VisibleStart.Y-1))))
      if((flag)
         &&
         (!(((ArrayPos.X + 1) * Settings.ElemSize /* + CurrentTowerParams.AttackRadius */
             > visibleStart.X * Settings.ElemSize) ||
            ((ArrayPos.Y + 1) * Settings.ElemSize /* + CurrentTowerParams.AttackRadius */
             > visibleStart.Y * Settings.ElemSize))))
      {
        flag = false;
      }
      if(!flag)
      {
        return;
      }
      canva.DrawImage(CurrentTowerParams.Picture,
                      Convert.ToInt32((-(CurrentTowerParams.Picture.Width / 2)
                                       + (ArrayPos.X + 1 - visibleStart.X) * Settings.ElemSize + Settings.DeltaX)
                                      * Scaling),
                      Convert.ToInt32((-(CurrentTowerParams.Picture.Height / 2)
                                       + (ArrayPos.Y + 1 - visibleStart.Y) * Settings.ElemSize + Settings.DeltaY)
                                      * Scaling),
                      Convert.ToInt32(CurrentTowerParams.Picture.Width * Scaling),
                      Convert.ToInt32(CurrentTowerParams.Picture.Height * Scaling));
      if(_wasCrit == 0)
      {
        return;
      }
      _wasCrit--;
      //Critical strike notification
      canva.DrawString(
        string.Format("{0}!", CurrentTowerParams.Damage * CurrentTowerParams.CritMultiple),
        new Font("Arial", 20), new SolidBrush(Color.Red),
        new PointF(
          (-(CurrentTowerParams.Picture.Width / 2) + (ArrayPos.X + 1 - visibleStart.X) * Settings.ElemSize
           + Settings.DeltaX) * Scaling,
          (-(CurrentTowerParams.Picture.Height / 2) + (ArrayPos.Y + 1 - visibleStart.Y) * Settings.ElemSize
           + Settings.DeltaY) * Scaling));
    }

    /// <summary>
    /// Checks, is arrPos a left top square of tower or not
    /// </summary>
    /// <param name="arrPos">The array pos.</param>
    /// <returns>Checking result </returns>
    internal bool Contain(Point arrPos)
    {
      return Helpers.TowerSquareCycle(1, (dx, dy) => ((ArrayPos.X + dx) == arrPos.X) && ((ArrayPos.Y + dy) == arrPos.Y));
    }

    /// <summary>
    /// Tower upgrading
    /// </summary>
    /// <returns>upgrading cost</returns>
    internal int Upgrade()
    {
      int upCost;
      Level++;
      if(_params.UnlimitedUp) //unlimited update
      {
        _currentTowerParams.AttackRadius += _params.UpgradeParams[1].AttackRadius;
        _currentMaxCooldown -= _params.UpgradeParams[1].Cooldown;
        if(_currentMaxCooldown < 0)
        {
          _currentMaxCooldown = 0;
        }
        _currentTowerParams.Damage += _params.UpgradeParams[1].Damage;
        _currentTowerParams.Cost = _params.UpgradeParams[1].Cost;
        upCost = _currentTowerParams.Cost;
      }
      else
      {
        if(Level == _params.UpgradeParams.Count) //limited upgrade
        {
          CanUpgrade = false;
        }
        int tmp = _currentTowerParams.Cooldown;
        _currentTowerParams = _params.UpgradeParams[Level - 1];
        _currentTowerParams.Cooldown = tmp;
        _currentMaxCooldown = _params.UpgradeParams[Level - 1].Cooldown;
        //[0].Picture till no picture for every tower level
        _currentTowerParams.Picture = _params.UpgradeParams[0].Picture;
        upCost = _currentTowerParams.Cost;
      }
      return upCost;
    }

    /// <summary>
    /// Gets the aims.
    /// </summary>
    /// <param name="monsters">The monsters.</param>
    /// <returns>
    /// Missles IEnumerable
    /// </returns>
    internal IEnumerable<Missle> GetAims(IEnumerable<Monster> monsters)
    {
      _currentTowerParams.Cooldown = _currentTowerParams.Cooldown == 0 ? 0 : --_currentTowerParams.Cooldown;
      if((CurrentTowerParams.Cooldown) == 0)
      {
        List<int> alreadyAdded = new List<int>(CurrentTowerParams.NumberOfTargets + 1);
        foreach(Monster monster in monsters)
        {
          PointF monsterPos = monster.GetCanvaPos;
          if((alreadyAdded.Contains(monster.ID)) ||
             !Helpers.UnitInRadius(monsterPos.X, monsterPos.Y, _towerCenterPos.X, _towerCenterPos.Y,
                                   CurrentTowerParams.AttackRadius))
          {
            continue;
          }
          //critical strike
          int damadgeWithCritical = Helpers.RandomForCrit.Next(1, 100) <= CurrentTowerParams.CritChance
                                      ? (int)(CurrentTowerParams.Damage * CurrentTowerParams.CritMultiple)
                                      : CurrentTowerParams.Damage;
          //For prevent spaming for one unit
          alreadyAdded.Add(monster.ID);
          if(damadgeWithCritical != CurrentTowerParams.Damage)
          {
            _wasCrit = 10;
            yield return
              Missle.Factory(FactoryAct.Create, monster.ID, damadgeWithCritical,
                             _params.TowerType,
                             _params.MissleBrushColor, _params.MisslePenColor,
                             _params.Modificator, _towerCenterPos.X, _towerCenterPos.Y);
          }
          else
          {
            yield return
              Missle.Factory(FactoryAct.Create, monster.ID, damadgeWithCritical,
                             _params.TowerType,
                             _params.MisslePenColor, _params.MissleBrushColor,
                             //All difference between this and previous yield return
                             _params.Modificator, _towerCenterPos.X, _towerCenterPos.Y);
          }
          if(alreadyAdded.Count < CurrentTowerParams.NumberOfTargets)
          {
            continue;
          }
          _currentTowerParams.Cooldown = _currentMaxCooldown;
          yield break;
        }
        if(alreadyAdded.Count != 0)
        {
          _currentTowerParams.Cooldown = _currentMaxCooldown;
        }
      }
    }

    /// <summary>
    /// Check if unit with x,y pos in the attack radius.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    internal bool InAttackRadius(float x, float y)
    {
      return Helpers.UnitInRadius(x, y, _towerCenterPos.X, _towerCenterPos.Y, CurrentTowerParams.AttackRadius);
    }

    /// <summary>
    /// Saves tower to file
    /// </summary>
    /// <param name="saveStream">The save stream.</param>
    internal void Save(BinaryWriter saveStream)
    {
      saveStream.Write(_confHash); //tower hash
      //Position in map array
      saveStream.Write(ArrayPos.X);
      saveStream.Write(ArrayPos.Y);
      saveStream.Write(_currentTowerParams.Cooldown);
      saveStream.Write(_wasCrit);
      saveStream.Write(Level);
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      return _params + CurrentTowerParams.ToString();
    }
  }
}