using System;
using System.Collections.Generic;
using System.Drawing;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Loaders;
using GameCoClassLibrary.Structures;
using GraphicLib.Interfaces;

namespace GameCoClassLibrary.Classes
{
  internal sealed class PauseMenu: Menu
  {
    internal PauseMenu(IGraphic graphicObject)
      : base(graphicObject)
    {
      Buttons = new Dictionary<Button, ButtonParams>
                  {
                    {
                      Button.NewGame, new ButtonParams
                                        {
                                          Image = Res.Buttons[Button.NewGame],
                                          Area = BuildButtonRect(Button.NewGame),
                                          Render = true
                                        }
                      },
                    {
                      Button.SaveGame, new ButtonParams
                                         {
                                           Image = Res.Buttons[Button.SaveGame],
                                           Area = BuildButtonRect(Button.SaveGame),
                                           Render = true
                                         }
                      },
                    {
                      Button.LoadGame, new ButtonParams
                                         {
                                           Image = Res.Buttons[Button.LoadGame],
                                           Area = BuildButtonRect(Button.LoadGame),
                                           Render = true
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
                      Button.Back, new ButtonParams
                                     {
                                       Image = Res.Buttons[Button.Back],
                                       Area = BuildButtonRect(Button.Back),
                                       Render = true
                                     }
                      },
                    {
                      Button.Exit, new ButtonParams
                                     {
                                       Image = Res.Buttons[Button.Exit],
                                       Area = BuildButtonRect(Button.Exit),
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
      RealShow(() => GraphObject.DrawImage(Res.PauseMenuBackground,
                                           Convert.ToInt32(((Settings.WindowWidth - Res.PauseMenuBackground.Width) / 2.0)
                                                           * Scaling),
                                           Convert.ToInt32(((Settings.WindowHeight - Res.PauseMenuBackground.Height)
                                                            / 2.0) * Scaling),
                                           Convert.ToInt32(Res.PauseMenuBackground.Width * Scaling),
                                           Convert.ToInt32(Res.PauseMenuBackground.Height * Scaling)));
    }

    protected override Rectangle BuildButtonRect(Button buttonType)
    {
      return RealBuildButtonRect(
        buttonType,
        delegate(out Point location, ref Size size)
          {
            switch(buttonType)
            {
              case Button.NewGame:
                location = new Point(
                  Convert.ToInt32(((Settings.WindowWidth - Res.Buttons[buttonType].Width) / 2.0) * Scaling),
                  Convert.ToInt32((((Settings.WindowHeight - Res.PauseMenuBackground.Height) / 2.0)) * Scaling));
                break;
              case Button.SaveGame:
                location = new Point(
                  Convert.ToInt32(((Settings.WindowWidth - Res.Buttons[buttonType].Width) / 2.0) * Scaling),
                  Convert.ToInt32((((Settings.WindowHeight - Res.PauseMenuBackground.Height) / 2)
                                   + Res.Buttons[Button.NewGame].Height + 5) * Scaling));
                break;
              case Button.LoadGame:
                location = new Point(
                  Convert.ToInt32(((Settings.WindowWidth - Res.Buttons[buttonType].Width) / 2.0) * Scaling),
                  Convert.ToInt32((((Settings.WindowHeight - Res.PauseMenuBackground.Height) / 2)
                                   + Res.Buttons[Button.NewGame].Height
                                   + Res.Buttons[Button.SaveGame].Height + 5) * Scaling));
                break;
              case Button.BigScale:
                location = new Point(
                  Convert.ToInt32(((Settings.WindowWidth - Res.Buttons[buttonType].Width) / 2.0) * Scaling),
                  Convert.ToInt32((((Settings.WindowHeight - Res.PauseMenuBackground.Height) / 2)
                                   + Res.Buttons[Button.NewGame].Height
                                   + Res.Buttons[Button.SaveGame].Height
                                   + Res.Buttons[Button.LoadGame].Height + 5) * Scaling));
                break;
              case Button.NormalScale:
                location = new Point(
                  Convert.ToInt32(((Settings.WindowWidth - Res.Buttons[buttonType].Width) / 2.0) * Scaling),
                  Convert.ToInt32((((Settings.WindowHeight - Res.PauseMenuBackground.Height) / 2)
                                   + Res.Buttons[Button.NewGame].Height
                                   + Res.Buttons[Button.SaveGame].Height
                                   + Res.Buttons[Button.LoadGame].Height
                                   + Res.Buttons[Button.BigScale].Height + 5) * Scaling));
                break;
              case Button.SmallScale:
                location = new Point(
                  Convert.ToInt32(((Settings.WindowWidth - Res.Buttons[buttonType].Width) / 2.0) * Scaling),
                  Convert.ToInt32((((Settings.WindowHeight - Res.PauseMenuBackground.Height) / 2)
                                   + Res.Buttons[Button.NewGame].Height
                                   + Res.Buttons[Button.SaveGame].Height
                                   + Res.Buttons[Button.LoadGame].Height
                                   + Res.Buttons[Button.BigScale].Height
                                   + Res.Buttons[Button.NormalScale].Height + 5) * Scaling));
                break;
              case Button.Back:
                location = new Point(
                  Convert.ToInt32(((Settings.WindowWidth - Res.Buttons[buttonType].Width) / 2.0) * Scaling),
                  Convert.ToInt32((((Settings.WindowHeight - Res.PauseMenuBackground.Height) / 2)
                                   + Res.Buttons[Button.NewGame].Height
                                   + Res.Buttons[Button.SaveGame].Height
                                   + Res.Buttons[Button.LoadGame].Height
                                   + Res.Buttons[Button.BigScale].Height
                                   + Res.Buttons[Button.NormalScale].Height
                                   + Res.Buttons[Button.SmallScale].Height + 5) * Scaling));
                break;
              case Button.Exit:
                location = new Point(
                  Convert.ToInt32(((Settings.WindowWidth - Res.Buttons[buttonType].Width) / 2.0) * Scaling),
                  Convert.ToInt32((((Settings.WindowHeight - Res.PauseMenuBackground.Height) / 2)
                                   + Res.Buttons[Button.NewGame].Height
                                   + Res.Buttons[Button.SaveGame].Height
                                   + Res.Buttons[Button.LoadGame].Height
                                   + Res.Buttons[Button.BigScale].Height
                                   + Res.Buttons[Button.NormalScale].Height
                                   + Res.Buttons[Button.SmallScale].Height
                                   + Res.Buttons[Button.Back].Height + 5) * Scaling));
                break;
              default:
                throw new ArgumentOutOfRangeException("buttonType");
            }
          });
    }
  }
}