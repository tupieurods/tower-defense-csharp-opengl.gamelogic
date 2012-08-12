using System;
using System.Collections.Generic;
using System.Drawing;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Loaders;
using GameCoClassLibrary.Structures;
using GraphicLib.Interfaces;

namespace GameCoClassLibrary.Classes
{
  internal sealed class GameUIMenu : Menu
  {
    internal GameUIMenu(IGraphic graphicObject)
      : base(graphicObject)
    {
      Buttons = new Dictionary<Button, ButtonParams>
                  {
                    {
                      Button.StartLevelEnabled, new ButtonParams
                                        {
                                          Image = Res.Buttons[Button.StartLevelEnabled],
                                          Area = BuildButtonRect(Button.StartLevelEnabled),
                                          Render = true
                                        }
                      },
                      {
                      Button.StartLevelDisabled, new ButtonParams
                                        {
                                          Image = Res.Buttons[Button.StartLevelDisabled],
                                          Area = BuildButtonRect(Button.StartLevelDisabled),
                                          Render = false
                                        }
                      },
                      {
                      Button.DestroyTower, new ButtonParams
                                        {
                                          Image = Res.Buttons[Button.DestroyTower],
                                          Area = BuildButtonRect(Button.DestroyTower),
                                          Render = false
                                        }
                      },
                      {
                      Button.UpgradeTower, new ButtonParams
                                        {
                                          Image = Res.Buttons[Button.UpgradeTower],
                                          Area = BuildButtonRect(Button.UpgradeTower),
                                          Render = false
                                        }
                      },
                      {
                      Button.SmallScale, new ButtonParams
                                           {
                                             Image = Res.Buttons[Button.SmallScale],
                                             Area = BuildButtonRect(Button.SmallScale),
                                             Render = true
                                           }
                      },
                    {
                      Button.NormalScale, new ButtonParams
                                            {
                                              Image = Res.Buttons[Button.NormalScale],
                                              Area = BuildButtonRect(Button.NormalScale),
                                              Render = true
                                            }
                      },
                    {
                      Button.BigScale, new ButtonParams
                                         {
                                           Image = Res.Buttons[Button.BigScale],
                                           Area = BuildButtonRect(Button.BigScale),
                                           Render = true
                                         }
                      },
                      {
                      Button.Pause, new ButtonParams
                                        {
                                          Image = Res.Buttons[Button.Pause],
                                          Area = BuildButtonRect(Button.Pause),
                                          Render = true
                                        }
                      },
                      {
                      Button.Unpause, new ButtonParams
                                        {
                                          Image = Res.Buttons[Button.Unpause],
                                          Area = BuildButtonRect(Button.Unpause),
                                          Render = false
                                        }
                      },
                      {Button.Menu, new ButtonParams
                                        {
                                          Image = Res.Buttons[Button.Menu],
                                          Area = BuildButtonRect(Button.Menu),
                                          Render = true
                                        }
                      }
                  };
    }

    /// <summary>
    /// Shows the menu(interface).
    /// </summary>
    public override void Show()
    {
      RealShow(null);
    }

    protected override Rectangle BuildButtonRect(Button buttonType)
    {
      return RealBuildButtonRect(
        buttonType,
        delegate(out Point location, ref Size size)
        {
          switch (buttonType)
          {
            case Button.StartLevelEnabled:
              location = new Point(
                Convert.ToInt32((Settings.BreakupLineXPosition - Settings.DeltaX - Res.Buttons[Button.StartLevelDisabled].Width) * Scaling),
                Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize + Res.Buttons[Button.SmallScale].Height) * Scaling));
              break;
            case Button.StartLevelDisabled:
              location = new Point(
                Convert.ToInt32((Settings.BreakupLineXPosition - Settings.DeltaX - Res.Buttons[Button.StartLevelDisabled].Width) * Scaling),
                Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize + Res.Buttons[Button.SmallScale].Height) * Scaling));
              break;
            case Button.DestroyTower:
              location = new Point(
                Convert.ToInt32((730 - Res.Buttons[Button.DestroyTower].Width) * Scaling),
                Convert.ToInt32(335 * Scaling));
              break;
            case Button.UpgradeTower:
              location = new Point(
                Convert.ToInt32((730 - Res.Buttons[Button.UpgradeTower].Width) * Scaling),
                Convert.ToInt32((325 - Res.Buttons[Button.DestroyTower].Height) * Scaling));
              break;
            case Button.BigScale:
              location = new Point(
                Convert.ToInt32(Settings.DeltaX * Scaling),
                Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * Scaling));
              break;
            case Button.NormalScale:
              location = new Point(
                Convert.ToInt32((Settings.DeltaX + Res.Buttons[Button.BigScale].Width) * Scaling),
                Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * Scaling));
              break;
            case Button.SmallScale:
              location = new Point(
                Convert.ToInt32((Settings.DeltaX + Res.Buttons[Button.BigScale].Width + Res.Buttons[Button.NormalScale].Width) * Scaling),
                Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * Scaling));
              break;
            case Button.Pause:
              location = new Point(
                Convert.ToInt32((Settings.DeltaX + Res.Buttons[Button.BigScale].Width + Res.Buttons[Button.SmallScale].Width
                                  + Res.Buttons[Button.NormalScale].Width) * Scaling),
                Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * Scaling));
              break;
            case Button.Unpause:
              location = new Point(
                Convert.ToInt32((Settings.DeltaX + Res.Buttons[Button.BigScale].Width + Res.Buttons[Button.SmallScale].Width
                                  + Res.Buttons[Button.NormalScale].Width) * Scaling),
                Convert.ToInt32((Settings.DeltaY * 2 + Settings.MapAreaSize) * Scaling));
              break;
            case Button.Menu:
              location = new Point(
                Convert.ToInt32(Settings.DeltaX * Scaling), Convert.ToInt32((Settings.WindowHeight - Res.Buttons[Button.Menu].Height - 5) * Scaling));
              break;
            default:
              throw new ArgumentOutOfRangeException("buttonType");
          }
        });
    }
  }
}
