using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

/////////////////////////////////////////////////////////////////////////////
// Dialog to show list of files to delete and get user confirmation.
// Rather for debugging
////////////////////////////////////////////////////////////////////////////
namespace Bak
{
	/// <summary>
	/// Summary description for DeleteFilesDlg.
	/// </summary>
	public class DeleteFilesDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox memoInfo;
		private System.Windows.Forms.Button buttonDelete;
		private System.Windows.Forms.Button buttonCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		bool m_bDoDelete = false;

		public DeleteFilesDlg()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.memoInfo = new System.Windows.Forms.TextBox();
			this.buttonDelete = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// memoInfo
			// 
			this.memoInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.memoInfo.Location = new System.Drawing.Point(8, 8);
			this.memoInfo.Multiline = true;
			this.memoInfo.Name = "memoInfo";
			this.memoInfo.ReadOnly = true;
			this.memoInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.memoInfo.Size = new System.Drawing.Size(664, 368);
			this.memoInfo.TabIndex = 0;
			this.memoInfo.Text = "memoInfo";
			// 
			// buttonDelete
			// 
			this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonDelete.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonDelete.Location = new System.Drawing.Point(512, 392);
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.TabIndex = 1;
			this.buttonDelete.Text = "Delete files";
			this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(600, 392);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.TabIndex = 2;
			this.buttonCancel.Text = "Cancel";
			// 
			// DeleteFilesDlg
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(680, 421);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonDelete);
			this.Controls.Add(this.memoInfo);
			this.MinimizeBox = false;
			this.Name = "DeleteFilesDlg";
			this.ShowInTaskbar = false;
			this.Text = "DeleteFilesDlg";
			this.ResumeLayout(false);

		}
		#endregion

		private void buttonDelete_Click(object sender, System.EventArgs e)
		{
//			DialogResult result = MessageBox.Show( "Are you sure you want to delete the files?", Defs.APP_TITLE, MessageBoxButtons.YesNo );
			m_bDoDelete = true; // DialogResult.Yes == result;
		}

		public static bool Do( string sInfo, string sCaption, bool bThereAreFilesToDelete )
		{
			DeleteFilesDlg dlg = new DeleteFilesDlg();
			dlg.memoInfo.Text = sInfo;
			dlg.Text = sCaption;
			dlg.buttonDelete.Enabled = bThereAreFilesToDelete;
			dlg.ShowDialog();
			return dlg.m_bDoDelete;
		}
	}
}
