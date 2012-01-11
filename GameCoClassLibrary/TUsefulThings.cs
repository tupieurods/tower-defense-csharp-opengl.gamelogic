using System;

namespace GameCoClassLibrary
{
  static public class TUsefulThings
  {
    static public void Check(string InValue, int DefaultValue, out int CheckResult)
    {
      CheckResult = Convert.ToInt32(InValue.Replace(" ", string.Empty)) == 0 ?
          DefaultValue : Convert.ToInt32(InValue.Replace(" ", string.Empty));
    }
  }
}