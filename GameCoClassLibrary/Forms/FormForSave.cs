using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace GameCoClassLibrary.Forms
{
  public partial class FormForSave : Form
  {
    public FormForSave()
    {
      InitializeComponent();
    }

    private void TBSaveName_KeyPress(object sender, KeyPressEventArgs e)
    {
      const string badSymbols = "\\|/:*?\"<>|";
      if (badSymbols.IndexOf(e.KeyChar.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) != -1)
        e.Handled = true;
    }

    private void BCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
    }

    private void BSave_Click(object sender, EventArgs e)
    {
      if (File.Exists(Environment.CurrentDirectory + "\\Data\\SavedGames\\") && (MessageBox.Show("File already exists. Do you want rewrite it?", "Tower defence", MessageBoxButtons.OKCancel) == DialogResult.Cancel))
        return;
      DialogResult = DialogResult.OK;
    }

    public string ReturnSaveFileName()
    {
      return TBSaveName.Text;
    }
  }
}
