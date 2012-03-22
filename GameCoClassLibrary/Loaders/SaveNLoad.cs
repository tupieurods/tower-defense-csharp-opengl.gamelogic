using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameCoClassLibrary.Loaders
{
  public static class SaveNLoad
  {
    public static void SaveMainGameConfig(BinaryWriter bwToSave, List<int> numberOfMonstersAtLevel,
        List<int> goldForSuccessfulLevelFinish, List<int> goldForKillMonster, params object[] args)
    {
      #region description

      /*Предполагается, что массив Args будет сформирован(передача в таком порядке) таким образом:
     * 0-string MapName
     * 1-string TowerFolderName
     * 2-int NumberOfLevels
     * 3-int NumberOfOptions
     * 4-int GoldAtStart
     * 5-int NumberOfLives
     * WARNING
     * Если массив будет сформирован не так, получим exception(или ещё хуже неправильно сформированный файл),
     * это описание необходимо обновлять каждый раз при изменении метода
     * WARNING
     */

      #endregion description

      bwToSave.Write(Convert.ToString(args[0]));//Имя файла карты
      bwToSave.Write(Convert.ToString(args[1]));//Имя папки с описанием башен
      bwToSave.Write(Convert.ToInt32(args[2]));//Число уровней
      bwToSave.Write(Convert.ToInt32(args[3]));//Запись числа опций, если в будущем опции появятся, то они должны будут быть записаны далее
      bwToSave.Write(1);//Тип опции 1-число монстров на каждом уровне
      if (numberOfMonstersAtLevel != null)
        foreach (int countMonsters in numberOfMonstersAtLevel)
        {
          bwToSave.Write(countMonsters);
        }
      else
        throw new ArgumentNullException("Пустой параметр NumberOfMonstersAtLevel");
      bwToSave.Write(2);//Тип опции 2-Вознаграждение за каждый пройденый уровень
      foreach (int moneyForSuccess in goldForSuccessfulLevelFinish)
      {
        bwToSave.Write(moneyForSuccess);
      }
      bwToSave.Write(3);//Тип опции 3-Число денег при старте игры
      bwToSave.Write(Convert.ToInt32(args[4]));
      bwToSave.Write(4);//Тип опции 4-Число жизней при старте игры
      bwToSave.Write(Convert.ToInt32(args[5]));
      bwToSave.Write(5);//Тип опции 5-Количество денег за убийство монстра на уровне
      if (goldForKillMonster != null)
        foreach (int moneyForKill in goldForKillMonster)
        {
          bwToSave.Write(moneyForKill);
        }
      else
        throw new ArgumentNullException("Пустой параметр GoldForKillMonster");
    }

    public static void LoadMainGameConf(BinaryReader brToLoad, out List<int> numberOfMonstersAtLevel,
        out List<int> goldForSuccessfulLevelFinish, out List<int> goldForKillMonster, out object[] outParams)
    {
      #region description

      /*Формирование массива OutParams:
     * 0-string MapName
     * 1-string TowerFolderName
     * 2-int NumberOfLevels
     * 3-int NumberOfOptions
     * 4-int GoldAtStart
     * 5-int NumberOfLives
     * WARNING
     * Если массив будет сформирован не так, получим exception(или ещё хуже неправильно сформированный файл),
     * это описание необходимо обновлять каждый раз при изменении метода
     * WARNING
     */

      #endregion description

      outParams = new object[6];
      outParams[0] = brToLoad.ReadString();//Имя карты
      //TBTowerFolder.Text = BRToLoad.ReadString();
      outParams[1] = brToLoad.ReadString();//Папка с конфигурацией башен
      outParams[2] = brToLoad.ReadInt32();//Число уровней
      //Считываем число опций и читаем сами опции, в текущей версии 0 опций
      outParams[3] = brToLoad.ReadInt32();//Число опций-Не выкидывается, т.к возможно понадобится в будущем
      //программе, которая будет загружать файл
      numberOfMonstersAtLevel = new List<int>();
      goldForSuccessfulLevelFinish = new List<int>();
      goldForKillMonster = new List<int>();
      for (int i = 0; i < (int)outParams[3]; i++)
      {
        int optionNumber = brToLoad.ReadInt32();

        #region Разбор опций

        switch (optionNumber)
        {
          case 1://Число монстров на уровне
            for (int j = 0; j < (int)outParams[2]; j++)
            {
              numberOfMonstersAtLevel.Add(brToLoad.ReadInt32());
            }
            break;
          case 2://Количество денег за успешное прохождение уровня
            for (int j = 0; j < (int)outParams[2]; j++)
            {
              goldForSuccessfulLevelFinish.Add(brToLoad.ReadInt32());
            }
            break;
          case 3://Количество денег при старте
            outParams[4] = brToLoad.ReadInt32();
            break;
          case 4://Количество жизней у игрока
            outParams[5] = brToLoad.ReadInt32();
            break;
          case 5://Количество денег за убийство монстра на уровне
            for (int j = 0; j < (int)outParams[2]; j++)
            {
              goldForKillMonster.Add(brToLoad.ReadInt32());
            }
            break;
        }

        #endregion Разбор опций
      }

      #region Если нам попалась старая версия файла

      if (numberOfMonstersAtLevel.Count() < (int)outParams[2])
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
      }

      #endregion Если нам попалась старая версия файла
    }
  }
}