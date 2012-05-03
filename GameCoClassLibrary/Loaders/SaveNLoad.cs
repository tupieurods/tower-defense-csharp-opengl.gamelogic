using System;
using System.Collections.Generic;
using System.IO;

namespace GameCoClassLibrary.Loaders
{
  public static class SaveNLoad
  {
    public static void SaveMainGameConfig(BinaryWriter bwToSave, List<int> numberOfMonstersAtLevel,
        List<int> goldForSuccessfulLevelFinish, List<int> goldForKillMonster, params object[] args)
    {
      #region description

      /*Args array:
     * 0-string MapName
     * 1-string TowerFolderName
     * 2-int NumberOfLevels
     * 3-int NumberOfOptions
     * 4-int GoldAtStart
     * 5-int NumberOfLives
     * WARNING
     * Update this description if you changing this method
     * WARNING
     */

      #endregion description

      bwToSave.Write(Convert.ToString(args[0]));//Map name
      bwToSave.Write(Convert.ToString(args[1]));//tower folder name
      bwToSave.Write(Convert.ToInt32(args[2]));//number of levels
      bwToSave.Write(Convert.ToInt32(args[3]));//number of options
      bwToSave.Write(1);//Option type 1-number of monsters at every level
      if (numberOfMonstersAtLevel != null)
        foreach (int countMonsters in numberOfMonstersAtLevel)
        {
          bwToSave.Write(countMonsters);
        }
      else
        throw new ArgumentNullException("Null parametr NumberOfMonstersAtLevel");
      bwToSave.Write(2);//Option type 2-money for every finished level
      foreach (int moneyForSuccess in goldForSuccessfulLevelFinish)
      {
        bwToSave.Write(moneyForSuccess);
      }
      bwToSave.Write(3);//Option type 3- Money at start
      bwToSave.Write(Convert.ToInt32(args[4]));
      bwToSave.Write(4);//Option type 4- Number of lives at start
      bwToSave.Write(Convert.ToInt32(args[5]));
      bwToSave.Write(5);//ТOption type 5-Money for moster killing at level
      if (goldForKillMonster != null)
        foreach (int moneyForKill in goldForKillMonster)
        {
          bwToSave.Write(moneyForKill);
        }
      else
        throw new ArgumentNullException("Пустой параметр GoldForKillMonster");
    }

    /// <summary>
    /// Loads the main game conf.
    /// </summary>
    /// <param name="brToLoad">The br to load.</param>
    /// <param name="numberOfMonstersAtLevel">The number of monsters at level.</param>
    /// <param name="goldForSuccessfulLevelFinish">The gold for successful level finish.</param>
    /// <param name="goldForKillMonster">The gold for kill monster.</param>
    /// <param name="outParams">The out params.</param>
    public static void LoadMainGameConf(BinaryReader brToLoad, out List<int> numberOfMonstersAtLevel,
        out List<int> goldForSuccessfulLevelFinish, out List<int> goldForKillMonster, out object[] outParams)
    {
      #region description

      /*outParams array:
     * 0-string MapName
     * 1-string TowerFolderName
     * 2-int NumberOfLevels
     * 3-int NumberOfOptions
     * 4-int GoldAtStart
     * 5-int NumberOfLives
     * WARNING
     * Update this description if you changing this method
     * WARNING
     */

      #endregion description

      outParams = new object[6];
      outParams[0] = brToLoad.ReadString();//Map name
      //TBTowerFolder.Text = BRToLoad.ReadString();
      outParams[1] = brToLoad.ReadString();//tower folder name
      outParams[2] = brToLoad.ReadInt32();//number of levels
      outParams[3] = brToLoad.ReadInt32();//number of options
      numberOfMonstersAtLevel = new List<int>();
      goldForSuccessfulLevelFinish = new List<int>();
      goldForKillMonster = new List<int>();
      for (int i = 0; i < (int)outParams[3]; i++)
      {
        int optionNumber = brToLoad.ReadInt32();

        #region options parsing

        switch (optionNumber)
        {
          case 1://number of monsters at every level
            for (int j = 0; j < (int)outParams[2]; j++)
            {
              numberOfMonstersAtLevel.Add(brToLoad.ReadInt32());
            }
            break;
          case 2://money for every finished level
            for (int j = 0; j < (int)outParams[2]; j++)
            {
              goldForSuccessfulLevelFinish.Add(brToLoad.ReadInt32());
            }
            break;
          case 3://Money at start
            outParams[4] = brToLoad.ReadInt32();
            break;
          case 4://Number of lives at start
            outParams[5] = brToLoad.ReadInt32();
            break;
          case 5://Money for moster killing at level
            for (int j = 0; j < (int)outParams[2]; j++)
            {
              goldForKillMonster.Add(brToLoad.ReadInt32());
            }
            break;
        }

        #endregion options parsing
      }

      #region Old file version

      /*if (numberOfMonstersAtLevel.Count() < (int)outParams[2])
      {
        for (int i = numberOfMonstersAtLevel.Count(); i < (int)outParams[2]; i++)
          numberOfMonstersAtLevel.Add(20);
      }
      if (goldForSuccessfulLevelFinish.Count() < (int)outParams[2])
      {
        for (int i = goldForSuccessfulLevelFinish.Count(); i < (int)outParams[2]; i++)
          goldForSuccessfulLevelFinish.Add(40);
      }
      if (goldForKillMonster.Count() < (int)outParams[2])
      {
        for (int i = goldForKillMonster.Count(); i < (int)outParams[2]; i++)
          goldForKillMonster.Add(10);
      }*/

      #endregion Old file version
    }
  }
}