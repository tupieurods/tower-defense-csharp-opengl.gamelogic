using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GameCoClassLibrary.Interfaces;
using GameCoClassLibrary.Loaders;
using GameCoClassLibrary.Structures;
using Button = GameCoClassLibrary.Enums.Button;

namespace GameCoClassLibrary.Classes
{
  /// <summary>
  /// Menu abstract class
  /// </summary>
  public abstract class Menu
  {
    /// <summary>
    /// delegate with button location building process(changing size if necessary).
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="size">The size.</param>
    protected delegate void ButtonBuilder(out Point location, ref Size size);

    /// <summary>
    /// graphic scale
    /// </summary>
    private float _scale = 1.0f;

    /// <summary>
    /// Graphic object to render menu
    /// </summary>
    protected readonly IGraphic GraphObject;

    /// <summary>
    /// Buttons data
    /// </summary>
    protected Dictionary<Button, ButtonParams> Buttons;

    /// <summary>
    /// Gets or sets the scaling.
    /// </summary>
    /// <value>
    /// The scaling.
    /// </value>
    public float Scaling
    {
      get { return _scale; }
      set
      {
        if (Math.Abs(_scale - value) < 0.0001)
          return;
        _scale = value;
        Resize();
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Menu"/> class.
    /// </summary>
    /// <param name="graphObject">The graph object.</param>
    protected Menu(IGraphic graphObject)
    {
      GraphObject = graphObject;
      Scaling = 1.0f;
    }

    /// <summary>
    /// Shows the menu by real;
    /// </summary>
    protected void RealShow(Action act)
    {
      if (act != null)
        act();
      foreach (var button in Buttons.Where(x => x.Value.Render))
      {
        GraphObject.DrawImage(button.Value.Image, button.Value.Area);
      }
    }

    /// <summary>
    /// Builds the button rect(interface).
    /// </summary>
    /// <param name="buttonType">Type of the button.</param>
    /// <returns></returns>
    protected abstract Rectangle BuildButtonRect(Button buttonType);

    /// <summary>
    /// Reals the build button rect.
    /// </summary>
    /// <param name="buttonType">Type of the button.</param>
    /// <param name="act">delegate with button location building process(changing size if necessary).</param>
    /// <returns></returns>
    protected Rectangle RealBuildButtonRect(Button buttonType, ButtonBuilder act)
    {
      Point location;
      Size size = new Size(Convert.ToInt32(Res.Buttons[buttonType].Width * Scaling), Convert.ToInt32(Res.Buttons[buttonType].Height * Scaling));
      act(out location, ref size);
      return new Rectangle(location, size);
    }

    /// <summary>
    /// Shows the menu(interface).
    /// </summary>
    public abstract void Show();

    /// <summary>
    /// Mouse up event.
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    /// <returns>Pressed button</returns>
    public Button MouseUp(MouseEventArgs e)
    {
      return e.Button != MouseButtons.Left ?
        Button.Empty
        : (from button in Buttons
           where button.Value.Render && button.Value.Area.Contains(e.X, e.Y)
           select button.Key).FirstOrDefault();
    }

    /// <summary>
    /// Optimization method. Checks click for one button.
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    /// <param name="buttonType">Type of the button.</param>
    /// <returns></returns>
    internal bool MouseUpCheckOne(MouseEventArgs e, Button buttonType)
    {
      if (!Buttons.ContainsKey(buttonType))
        throw new ArgumentException("buttonType");
      return Buttons[buttonType].Area.Contains(e.X, e.Y);
    }

    /// <summary>
    /// Sets the state of the render.
    /// </summary>
    /// <param name="buttonType">Type of the button.</param>
    /// <param name="state">if set to <c>true</c> [state].</param>
    internal void SetRenderState(Button buttonType, bool state)
    {
      if (!Buttons.ContainsKey(buttonType))
        throw new ArgumentException("buttonType");
      var tmp = Buttons[buttonType];
      tmp.Render = state;
      Buttons[buttonType] = tmp;
    }

    /// <summary>
    /// Gets the button position.
    /// </summary>
    /// <param name="buttonType">Type of the button.</param>
    /// <returns></returns>
    internal Point GetButtonPosition(Button buttonType)
    {
      if (!Buttons.ContainsKey(buttonType))
        throw new ArgumentException("buttonType");
      return Buttons[buttonType].Area.Location;
    }

    /// <summary>
    /// Resize menu elements
    /// </summary>
    private void Resize()
    {
      for (Button i = (Button)1; i < (Button)Enum.GetNames(typeof(Button)).Length; i++)
      {
        if (!Buttons.ContainsKey(i))
          continue;
        ButtonParams tmp = Buttons[i];
        tmp.Area = BuildButtonRect(i);
        Buttons[i] = tmp;
      }
    }
  }
}
