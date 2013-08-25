using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using GameCoClassLibrary.Properties;

namespace GameCoClassLibrary.Forms
{
  public partial class FormForSave: Form
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="FormForSave"/> class.
    /// </summary>
    public FormForSave()
    {
      InitializeComponent();
    }

    /// <summary>
    /// No bad for Windows symbols
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Forms.KeyPressEventArgs"/> instance containing the event data.</param>
    private void TBSaveName_KeyPress(object sender, KeyPressEventArgs e)
    {
      const string badSymbols = "\\|/:*?\"<>|";
      if(badSymbols.IndexOf(e.KeyChar.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) != -1)
      {
        e.Handled = true;
      }
    }

    /// <summary>
    /// Cancel saving
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void BCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
    }

    /// <summary>
    ///  Saving
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void BSave_Click(object sender, EventArgs e)
    {
      if(File.Exists(Environment.CurrentDirectory + "\\Data\\SavedGames\\" + TBSaveName.Text + ".tdsg")
         &&
         (MessageBox.Show(Resources.File_already_exist, Resources.AppName, MessageBoxButtons.OKCancel)
          == DialogResult.Cancel))
      {
        return;
      }
      DialogResult = DialogResult.OK;
    }

    /// <summary>
    /// Returns the name of the save file.
    /// </summary>
    /// <returns></returns>
    public string ReturnSaveFileName()
    {
      return TBSaveName.Text;
    }
  }
}