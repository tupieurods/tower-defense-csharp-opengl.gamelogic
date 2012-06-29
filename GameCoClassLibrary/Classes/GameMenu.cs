using System;
using System.Drawing;
using System.Windows.Forms;
using GameCoClassLibrary.Enums;
using GameCoClassLibrary.Forms;
using Button = GameCoClassLibrary.Enums.Button;

namespace GameCoClassLibrary.Classes
{
  [Obsolete]
  public sealed class GameMenu
  {
    #region Private vars
    /// <summary>
    /// GraphicEngine object
    /// </summary>
    private readonly GraphicEngine _graphicEngine;

    /// <summary>
    /// Timer object
    /// Null, if _graphicEngine!=WinForms
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
    #endregion

    #region internal vars
    /// <summary>
    /// Gets the scaling.
    /// sets in private
    /// </summary>
    internal float Scaling
    {
      get
      {
        return _scale;
      }
      private set
      {
        if (Math.Abs(_scale - value) < 0.0001)
          return;
        _scale = value;
        //frame buffer
        /*if (_drawingSpace != null)
        {
          _drawingSpace.Width = Convert.ToInt32(Settings.WindowWidth * Scaling);
          _drawingSpace.Height = Convert.ToInt32(Settings.WindowHeight * Scaling);
          _graphicEngine.SetNewGraphBuffer(BufferedGraphicsManager.Current.Allocate(_drawingSpace.CreateGraphics(),
                                                                                    new Rectangle(new Point(0, 0),
                                                                                                  _drawingSpace.Size)));
        }*/
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
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="GameMenu"/> class.
    /// </summary>
    /// <param name="graphicEngineType">Type of the graphic engine.</param>
    /// <param name="pbForDraw">The pb for draw.</param>
    public GameMenu(GraphicEngineType graphicEngineType, PictureBox pbForDraw)
    {
      _timer = null;
      switch (graphicEngineType)
      {
        case GraphicEngineType.WinForms:
          _graphicEngine = new GraphicEngine(new WinFormsGraphic(null));
          _timer = new Timer();
          _timer.Tick += TimerTick;
          _timer.Interval = 30;
          _timer.Start();
          break;
        case GraphicEngineType.OpenGL:
          break;
        case GraphicEngineType.SharpDX:
          break;
        default:
          throw new ArgumentOutOfRangeException("graphicEngineType");
      }
      _drawingSpace = pbForDraw;
      Scaling = 1F;
    }

    /// <summary>
    /// Timer tick event
    /// </summary>
    /// <param name="obj">The obj.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    public void TimerTick(object obj, EventArgs e)
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
      bool flag = false;
      Helpers.ButtonCycle((Button i) =>
                            {
                              if ((!GameStarted) && (i == Button.Pause || i == Button.Unpause || i == Button.SaveGame || i == Button.StartLevelEnabled || i == Button.UpgradeTower || i == Button.DestroyTower))
                                return false;
                              if ((GameStarted) && ((_game.Paused && i == Button.Pause) || (!_game.Paused && i == Button.Unpause)))
                                return false;
                              if (!Helpers.BuildButtonRect(i, Scaling, GameStarted).Contains(e.X, e.Y))
                                return false;
                              MenuButtonAct(i);
                              flag = true;
                              return true;
                            });
      if (GameStarted && !flag)
        _game.MouseUp(e);
    }

    /// <summary>
    /// Button pressed in menu
    /// </summary>
    /// <param name="button">The button.</param>
    private void MenuButtonAct(Button button)
    {
      switch (button)
      {
        case Button.StartLevelEnabled:
          _game.NewLevelButtonClick();
          break;
        case Button.StartLevelDisabled:
          break;
        case Button.DestroyTower:
          _game.DestroyButtonClick();
          break;
        case Button.UpgradeTower:
          _game.UpgdareButtonClick();
          break;
        case Button.BigScale:
          Scaling = 2.0F;
          break;
        case Button.NormalScale:
          Scaling = 1.0F;
          break;
        case Button.SmallScale:
          Scaling = 0.6875F;
          break;
        case Button.Exit:
          Environment.Exit(0);
          break;
        case Button.LoadGame:
          CreateNewGame(FormType.Load);
          break;
        case Button.SaveGame:
          _game.Paused = true;
          FormForSave saveNameForm = new FormForSave();
          if (saveNameForm.ShowDialog() == DialogResult.OK)
          {
            _game.SaveGame(saveNameForm.ReturnSaveFileName());
            MessageBox.Show("Saved");
          }
          _game.Paused = false;
          break;
        case Button.Pause:
        case Button.Unpause:
          _game.Paused = !_game.Paused;
          break;
        case Button.NewGame:
          CreateNewGame(FormType.GameConfiguration);
          break;
        default:
          throw new ArgumentOutOfRangeException("button");
      }
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
          if (_timer != null)
            _timer.Stop();
          _game = null;
        }
        try
        {
          /*_game = Game.Factory(selectorForm.ReturnFileName(),
                               formType == FormType.GameConfiguration ? FactoryAct.Create : FactoryAct.Load,
                               _graphicEngine);*/
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
        if (_timer != null)
          _timer.Start();
      }
    }
  }
}
