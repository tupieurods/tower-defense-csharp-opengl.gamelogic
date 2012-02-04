using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameCoClassLibrary
{
  class TMonster
  {
    #region Private Vars
    private MonsterParam Params;
    private List<Point> Way;
    #endregion

    #region Public Vars
    #endregion

    public TMonster(MonsterParam Params, List<Point> Way)
    {
      this.Params = Params;
      this.Way = Way;
    }
  }
}
