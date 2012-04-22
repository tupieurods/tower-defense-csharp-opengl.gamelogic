namespace GameCoClassLibrary.Forms
{
  partial class FormForSave
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.BSave = new System.Windows.Forms.Button();
      this.BCancel = new System.Windows.Forms.Button();
      this.LSaveFileName = new System.Windows.Forms.Label();
      this.TBSaveName = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // BSave
      // 
      this.BSave.Location = new System.Drawing.Point(19, 39);
      this.BSave.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
      this.BSave.Name = "BSave";
      this.BSave.Size = new System.Drawing.Size(138, 42);
      this.BSave.TabIndex = 0;
      this.BSave.Text = "Save";
      this.BSave.UseVisualStyleBackColor = true;
      this.BSave.Click += new System.EventHandler(this.BSave_Click);
      // 
      // BCancel
      // 
      this.BCancel.Location = new System.Drawing.Point(169, 39);
      this.BCancel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
      this.BCancel.Name = "BCancel";
      this.BCancel.Size = new System.Drawing.Size(138, 42);
      this.BCancel.TabIndex = 1;
      this.BCancel.Text = "Cancel";
      this.BCancel.UseVisualStyleBackColor = true;
      this.BCancel.Click += new System.EventHandler(this.BCancel_Click);
      // 
      // LSaveFileName
      // 
      this.LSaveFileName.AutoSize = true;
      this.LSaveFileName.Location = new System.Drawing.Point(15, 9);
      this.LSaveFileName.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      this.LSaveFileName.Name = "LSaveFileName";
      this.LSaveFileName.Size = new System.Drawing.Size(110, 24);
      this.LSaveFileName.TabIndex = 2;
      this.LSaveFileName.Text = "Save name:";
      // 
      // TBSaveName
      // 
      this.TBSaveName.Location = new System.Drawing.Point(120, 6);
      this.TBSaveName.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
      this.TBSaveName.Name = "TBSaveName";
      this.TBSaveName.Size = new System.Drawing.Size(187, 29);
      this.TBSaveName.TabIndex = 3;
      this.TBSaveName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBSaveName_KeyPress);
      // 
      // FormForSave
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(324, 92);
      this.Controls.Add(this.TBSaveName);
      this.Controls.Add(this.LSaveFileName);
      this.Controls.Add(this.BCancel);
      this.Controls.Add(this.BSave);
      this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
      this.Name = "FormForSave";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Save game";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button BSave;
    private System.Windows.Forms.Button BCancel;
    private System.Windows.Forms.Label LSaveFileName;
    private System.Windows.Forms.TextBox TBSaveName;
  }
}