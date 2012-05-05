using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GameCoClassLibrary.Enums;

namespace GameCoClassLibrary.Forms
{
  public partial class FormForSelection : Form
  {
    /// <summary>
    /// Form type(loading or new game)
    /// </summary>
    private readonly FormType _type;

    /// <summary>
    /// Prevents a default instance of the <see cref="FormForSelection"/> class from being created.
    /// </summary>
    private FormForSelection()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FormForSelection"/> class.
    /// </summary>
    /// <param name="type">The type.</param>
    public FormForSelection(FormType type)
    {
      InitializeComponent();
      _type = type;
      switch (type)
      {
        case FormType.GameConfiguration:
          Text = "Game configuration selection";
          BSelect.Text = "Select";
          break;
        case FormType.Load:
          Text = "Load the game";
          BSelect.Text = "Load";
          break;
        default:
          throw new ArgumentOutOfRangeException("type");
      }
    }

    /// <summary>
    /// Cancel selection
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void BCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
    }

    /// <summary>
    /// Handles the Click event of the BSelect control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void BSelect_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    /// <summary>
    /// File list loading
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void GameConfSelector_Load(object sender, EventArgs e)
    {
      string path = Environment.CurrentDirectory;
      string extension;
      switch (_type)
      {
        case FormType.GameConfiguration:
          path += "\\Data\\GameConfigs\\";
          extension = ".tdgc";
          break;
        case FormType.Load:
          path += "\\Data\\SavedGames\\";
          extension = ".tdsg";
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      DirectoryInfo diForListLoad = new DirectoryInfo(path);
      FileInfo[] fileList = diForListLoad.GetFiles();
      foreach (FileInfo i in fileList.Where(i => i.Extension == extension))
      {
        LBFileList.Items.Add(i.Name.Substring(0, i.Name.Length - 5));
      }
      if (LBFileList.Items.Count == 0)
      {
        MessageBox.Show("Files not founded!");
        DialogResult = DialogResult.Abort;
      }
      else
        LBFileList.SelectedIndex = 0;
    }

    /// <summary>
    /// Returns the name of the file.
    /// </summary>
    /// <returns></returns>
    public string ReturnFileName()
    {
      return LBFileList.SelectedIndex >= 0 ? Convert.ToString(LBFileList.SelectedItem) : null;
    }
  }
}
