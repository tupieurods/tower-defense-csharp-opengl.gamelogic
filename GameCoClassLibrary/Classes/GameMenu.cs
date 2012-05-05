using System;
using System.Drawing;
using System.Windows.Forms;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Forms;
using Button = GameCoClassLibrary.Enums.Button;

namespace GameCoClassLibrary.Classes
{
  public sealed class GameMenu
  {

    /// <summary>
    /// GraphicEngine object
    /// </summary>
    private readonly GraphicEngine _graphicEngine;

    /// <summary>
    /// Timer object
    /// </summary>
    private readonly Timer _timer;

    /// <summary>
    /// Scaling factor
    /// </summary>
    private float _scale;

    /// <summary>
    /// Game object
    /// </summary>
    private Game _game;

    /// <summary>
    /// Picture Box, null if not WinForms graphics
    /// </summary>
    private readonly PictureBox _drawingSpace;

    /// <summary>
    /// Gets the scaling.
    /// </summary>
    internal float Scaling
    {
      get
      {
        return _scale;
      }
      private set
      {
        //Если программист не догадывается что изображение не может содержать не целый пиксель
        //мы защитимся от такого тормоза
        //if (Convert.ToInt32((value*Settings.ElemSize) - Math.Floor(value*Settings.ElemSize)) != 0) return;
        if (Math.Abs(_scale - value) < 0.0001)
          return;
        _scale = value;
        //Создание буфера кадров
        if (_drawingSpace != null)
        {
          _drawingSpace.Width = Convert.ToInt32(Settings.WindowWidth * Scaling);
          _drawingSpace.Height = Convert.ToInt32(Settings.WindowHeight * Scaling);
          _graphicEngine.SetNewGraphBuffer(BufferedGraphicsManager.Current.Allocate(_drawingSpace.CreateGraphics(),
                                                                                    new Rectangle(new Point(0, 0),
                                                                                                  _drawingSpace.Size)));
        }
        if (_game != null)
          _game.Scaling = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether game created or loaded.
    /// </summary>
    /// <value>
    ///   <c>true</c> if game started or loaded; otherwise, <c>false</c>.
    /// </value>
    internal bool GameStarted
    {
      get { return _game != null; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameMenu"/> class.
    /// </summary>
    /// <param name="graphicEngineType">Type of the graphic engine.</param>
    /// <param name="pbForDraw">The pb for draw.</param>
    public GameMenu(GraphicEngineType graphicEngineType, PictureBox pbForDraw)
    {
      _drawingSpace = pbForDraw;
      _timer = new Timer();
      _timer.Tick += TimerTick;
      _timer.Interval = 30;
      _timer.Start();
      switch (graphicEngineType)
      {
        case GraphicEngineType.WinForms:
          _graphicEngine = new GraphicEngine(new WinFormsGraphic(null));
          break;
        case GraphicEngineType.OpenGL:
          break;
        case GraphicEngineType.SharpDX:
          break;
        default:
          throw new ArgumentOutOfRangeException("graphicEngineType");
      }
      Scaling = 1F;
    }

    /// <summary>
    /// Timer tick event
    /// </summary>
    /// <param name="obj">The obj.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void TimerTick(object obj, EventArgs e)
    {
      if (_game != null)
      {
        if (!_game.Paused)
        {
          _game.Tick(_drawingSpace.PointToClient(Control.MousePosition));
        }
        if (_game.Lose)
        {
          _game = null;
          MessageBox.Show("You lost.");
          return;
        }
        _game.Render();
      }
      _graphicEngine.ShowMenu(this, _game != null && _game.Paused);
      _graphicEngine.Render();
    }

    /// <summary>
    /// Mouse move event.
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    public void MouseMove(MouseEventArgs e)
    {
      if (GameStarted)
        _game.MouseMove(e);
    }

    /// <summary>
    /// Mouse up event.
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    public void MouseUp(MouseEventArgs e)
    {
      if (GameStarted)
      {
        if (Helpers.BuildButtonRect(_game.Paused ? Button.Unpause : Button.Pause, Scaling).Contains(e.X, e.Y))
        {
          _game.Paused = !_game.Paused;
        }
        else if (Helpers.BuildButtonRect(Button.SaveGame, Scaling).Contains(e.X, e.Y))
        {
          if (_game == null) return;
          _game.Paused = true;
          FormForSave saveNameForm = new FormForSave();
          if (saveNameForm.ShowDialog() == DialogResult.OK)
          {
            _game.SaveGame(saveNameForm.ReturnSaveFileName());
            MessageBox.Show("Saved");
          }
          _game.Paused = false;
        }
        else
          if (!ChechkScaleChanging(e))
            _game.MouseUp(e);
      }
      else
      {
        if (Helpers.BuildButtonRect(Button.Exit, Scaling).Contains(e.X, e.Y))
          Environment.Exit(0);
        else if (ChechkScaleChanging(e))
          return;
      }
      if (Helpers.BuildButtonRect(Button.NewGame, Scaling, GameStarted).Contains(e.X, e.Y))
      {
        CreateNewGame(FormType.GameConfiguration);
      }
      else if (Helpers.BuildButtonRect(Button.LoadGame, Scaling, GameStarted).Contains(e.X, e.Y))
      {
        CreateNewGame(FormType.Load);
      }
    }

    /// <summary>
    /// Chechks the scale changing.
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
    /// <returns></returns>
    private bool ChechkScaleChanging(MouseEventArgs e)
    {
      bool result = false;
      if (Helpers.BuildButtonRect(Button.BigScale, Scaling, GameStarted).Contains(e.X, e.Y))
      {
        Scaling = 2.0F;
        result = true;
      }
      else if (Helpers.BuildButtonRect(Button.NormalScale, Scaling, GameStarted).Contains(e.X, e.Y))
      {
        Scaling = 1.0F;
        result = true;
      }
      else if (Helpers.BuildButtonRect(Button.SmallScale, Scaling, GameStarted).Contains(e.X, e.Y))
      {
        Scaling = 0.6875F;
        result = true;
      }
      return result;
    }

    /// <summary>
    /// Creates or loads the new game.
    /// </summary>
    /// <param name="formType">Type of the form.</param>
    private void CreateNewGame(FormType formType)
    {
      FormForSelection selectorForm = new FormForSelection(formType);
      if (selectorForm.ShowDialog() == DialogResult.OK)
      {
        if (_game != null)
        {
          _timer.Stop();
          _game = null;
        }
        try
        {
          _game = Game.Factory(selectorForm.ReturnFileName(),
                               formType == FormType.GameConfiguration ? FactoryAct.Create : FactoryAct.Load,
                               _graphicEngine);
        }
        catch
        {
          _game = null;
        }
        if (_game != null)
        {
          _game.Scaling = Scaling;
          MessageBox.Show("Game conf loaded successeful");
        }
        _timer.Start();
      }
    }
  }
}
