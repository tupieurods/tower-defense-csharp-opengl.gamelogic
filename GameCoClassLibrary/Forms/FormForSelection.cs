using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GameCoClassLibrary.Enums;

namespace GameCoClassLibrary.Forms
{
  public partial class FormForSelection : Form
  {
    private readonly FormType _type;

    private FormForSelection()
    {
      InitializeComponent();
    }

    public FormForSelection(FormType type)
    {
      InitializeComponent();
      _type = type;
      switch (type)
      {
        case FormType.GameConfiguration:
          this.Text = "Game configuration selection";
          BSelect.Text = "Select";
          break;
        case FormType.Load:
          this.Text = "Load the game";
          BSelect.Text = "Load";
          break;
        default:
          throw new ArgumentOutOfRangeException("type");
      }
    }

    private void BCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
    }

    private void BSelect_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

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

    public string ReturnFileName()
    {
      return LBFileList.SelectedIndex >= 0 ? Convert.ToString(LBFileList.SelectedItem) : null;
    }
  }
}
