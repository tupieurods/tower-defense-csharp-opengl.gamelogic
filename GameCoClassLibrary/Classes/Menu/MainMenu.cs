using System;
using System.Collections.Generic;
using System.Drawing;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Loaders;
using GameCoClassLibrary.Structures;
using GraphicLib.Interfaces;

namespace GameCoClassLibrary.Classes
{
  public sealed class MainMenu : Menu
  {

    public MainMenu(IGraphic graphObject)
      : base(graphObject)
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
      RealShow(() =>
               GraphObject.DrawImage(Res.MenuBackground(Scaling), 0, 0, Convert.ToInt32(Settings.WindowWidth * Scaling), Convert.ToInt32(Settings.WindowHeight * Scaling)));
    }

    protected override Rectangle BuildButtonRect(Button buttonType)
    {
      /*Point location;
      Size size = new Size(Convert.ToInt32(Res.Buttons[buttonType].Width * Scaling), Convert.ToInt32(Res.Buttons[buttonType].Height * Scaling));*/
      return RealBuildButtonRect(
        buttonType,
        delegate(out Point location, ref Size size)
        {
          switch (buttonType)
          {
            case Button.NewGame:
              location =
                new Point(
                  Convert.ToInt32((Settings.WindowWidth -
                                   Res.Buttons[Button.NewGame].Width) * Scaling),
                  Convert.ToInt32(100 * Scaling));
              break;
            case Button.BigScale:
              location = new Point(Convert.ToInt32((Settings.WindowWidth -
                                                    Res.Buttons[Button.NewGame].Width -
                                                    Res.Buttons[Button.BigScale].Width) *
                                                   Scaling), Convert.ToInt32(100 * Scaling));
              break;
            case Button.NormalScale:
              location = new Point(Convert.ToInt32((Settings.WindowWidth -
                                                    Res.Buttons[Button.NewGame].Width -
                                                    Res.Buttons[Button.BigScale].Width -
                                                    Res.Buttons[Button.NormalScale].Width) *
                                                   Scaling), Convert.ToInt32(100 * Scaling));
              break;
            case Button.SmallScale:
              location = new Point(Convert.ToInt32((Settings.WindowWidth -
                                                    Res.Buttons[Button.NewGame].Width -
                                                    Res.Buttons[Button.BigScale].Width -
                                                    Res.Buttons[Button.NormalScale].Width -
                                                    Res.Buttons[Button.SmallScale].Width) *
                                                   Scaling), Convert.ToInt32(100 * Scaling));
              break;
            case Button.Exit:
              location =
                new Point(
                  Convert.ToInt32((Settings.WindowWidth - Res.Buttons[Button.Exit].Width) *
                                  Scaling),
                  Convert.ToInt32((100 + Res.Buttons[Button.NewGame].Height +
                                   Res.Buttons[Button.LoadGame].Height) * Scaling) /* + 5*/);
              break;
            case Button.LoadGame:
              location =
                new Point(
                  Convert.ToInt32((Settings.WindowWidth -
                                   Res.Buttons[Button.LoadGame].Width) * Scaling),
                  Convert.ToInt32((100 + Res.Buttons[Button.NewGame].Height) * Scaling)
                /* + 5*/);
              break;
            default:
              throw new ArgumentOutOfRangeException("buttonType");
          }
        });
      //return new Rectangle(location, size);
    }
  }
}