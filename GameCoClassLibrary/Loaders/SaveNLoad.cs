using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace GameCoClassLibrary
{
  public static class SaveNLoad
  {
    public static void SaveMainGameConfig(BinaryWriter BWToSave, List<int> NumberOfMonstersAtLevel,
        List<int> GoldForSuccessfulLevelFinish, List<int> GoldForKillMonster, params object[] Args)
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
      #endregion
      BWToSave.Write(Convert.ToString(Args[0]));//Имя файла карты
      BWToSave.Write(Convert.ToString(Args[1]));//Имя папки с описанием башен
      BWToSave.Write(Convert.ToInt32(Args[2]));//Число уровней
      BWToSave.Write(Convert.ToInt32(Args[3]));//Запись числа опций, если в будущем опции появятся, то они должны будут быть записаны далее
      BWToSave.Write(1);//Тип опции 1-число монстров на каждом уровне
      if (NumberOfMonstersAtLevel != null)
        foreach (int CountMonsters in NumberOfMonstersAtLevel)
        {
          BWToSave.Write(CountMonsters);
        }
      else
        throw new ArgumentNullException("Пустой параметр NumberOfMonstersAtLevel");
      BWToSave.Write(2);//Тип опции 2-Вознаграждение за каждый пройденый уровень
      if (NumberOfMonstersAtLevel != null)
        foreach (int MoneyForSuccess in GoldForSuccessfulLevelFinish)
        {
          BWToSave.Write(MoneyForSuccess);
        }
      else
        throw new ArgumentNullException("Пустой параметр MoneyForSuccess");
      BWToSave.Write(3);//Тип опции 3-Число денег при старте игры
      BWToSave.Write(Convert.ToInt32(Args[4]));
      BWToSave.Write(4);//Тип опции 4-Число жизней при старте игры
      BWToSave.Write(Convert.ToInt32(Args[5]));
      BWToSave.Write(5);//Тип опции 5-Количество денег за убийство монстра на уровне
      if (GoldForKillMonster != null)
        foreach (int MoneyForKill in GoldForKillMonster)
        {
          BWToSave.Write(MoneyForKill);
        }
      else
        throw new ArgumentNullException("Пустой параметр GoldForKillMonster");
    }

    public static void LoadMainGameConf(BinaryReader BRToLoad, out List<int> NumberOfMonstersAtLevel,
        out List<int> GoldForSuccessfulLevelFinish, out List<int> GoldForKillMonster, out object[] OutParams)
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
      #endregion
      OutParams = new object[6];
      OutParams[0] = BRToLoad.ReadString();//Имя карты
      //TBTowerFolder.Text = BRToLoad.ReadString();
      OutParams[1] = BRToLoad.ReadString();//Папка с конфигурацией башен
      OutParams[2] = BRToLoad.ReadInt32();//Число уровней
      //Считываем число опций и читаем сами опции, в текущей версии 0 опций
      OutParams[3] = BRToLoad.ReadInt32();//Число опций-Не выкидывается, т.к возможно понадобится в будущем
      //программе, которая будет загружать файл
      NumberOfMonstersAtLevel = new List<int>();
      GoldForSuccessfulLevelFinish = new List<int>();
      GoldForKillMonster = new List<int>();
      for (int i = 0; i < (int)OutParams[3]; i++)
      {
        int OptionNumber = BRToLoad.ReadInt32();
        #region Разбор опций
        switch (OptionNumber)
        {
          case 1://Число монстров на уровне
            for (int j = 0; j < (int)OutParams[2]; j++)
            {
              NumberOfMonstersAtLevel.Add(BRToLoad.ReadInt32());
            }
            break;
          case 2://Количество денег за успешное прохождение уровня
            for (int j = 0; j < (int)OutParams[2]; j++)
            {
              GoldForSuccessfulLevelFinish.Add(BRToLoad.ReadInt32());
            }
            break;
          case 3://Количество денег при старте
            OutParams[4] = BRToLoad.ReadInt32();
            break;
          case 4://Количество жизней у игрока
            OutParams[5] = BRToLoad.ReadInt32();
            break;
          case 5://Количество денег за убийство монстра на уровне
            for (int j = 0; j < (int)OutParams[2]; j++)
            {
              GoldForKillMonster.Add(BRToLoad.ReadInt32());
            }
            break;
        }
        #endregion
      }
      #region Если нам попалась старая версия файла
      if (NumberOfMonstersAtLevel.Count() < (int)OutParams[2])
      {
        for (int i = NumberOfMonstersAtLevel.Count(); i < (int)OutParams[2]; i++)
          NumberOfMonstersAtLevel.Add(20);
      }
      if (GoldForSuccessfulLevelFinish.Count() < (int)OutParams[2])
      {
        for (int i = GoldForSuccessfulLevelFinish.Count(); i < (int)OutParams[2]; i++)
          GoldForSuccessfulLevelFinish.Add(40);
      }
      if (GoldForKillMonster.Count() < (int)OutParams[2])
      {
        for (int i = GoldForKillMonster.Count(); i < (int)OutParams[2]; i++)
          GoldForKillMonster.Add(10);
      }
      #endregion
    }
  }
}