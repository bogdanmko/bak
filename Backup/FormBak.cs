using System;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices; 
using System.Net;

//////////////////////////////////////////////////////////////////////////
// Backup utility. (c) Micro Logic corp 2011
//////////////////////////////////////////////////////////////////////////
// ProcessJobs() - is entry point for backup process.

namespace Bak
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	/// 

	//========================================================
	public struct BAK_SUB_FILE
	{
		public string sFileName;
		public long Size;

		public BAK_SUB_FILE(int nDummy)
		{
			sFileName = "";
			Size = 0;
		}
	}

  //========================================================
	// Bak item def from XML job file
	struct BAK_ITEM_DEF
	{
		public string sCheckFile;
		public string sCheckDir;
		public string sWait;
		public string sSource;
		public string sAfterCMDEXE;
		public string sAfterCMDPARAMS;
		public string sArcFilePrefix;
		public string sArcSubdir; //Subdirectory to save backups to
		public bool     bAfterCmdShowWindow;
		public bool bAfterCMDWriteStdErrToLog;
		public bool bAfterCmdNowait;
		public bool bDisabled;
		public int nWeightAbstract;
		public int nMaxWaitTime_Sec;

		public void Init()
		{
			sSource = "";
			sAfterCMDEXE = "";
			sAfterCMDPARAMS = "";
			sArcFilePrefix = "";
			sArcSubdir = "";
			bDisabled = false;
			bAfterCMDWriteStdErrToLog = false;
			bAfterCmdShowWindow = false;
			bAfterCmdNowait = false;
			nWeightAbstract = 0;
			nMaxWaitTime_Sec = 7200;
			sCheckFile = "";
			sCheckDir = "";
			sWait = "";
		}

	}


	//========================================================
	// Main application form
	public class CformBak : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox memoJob;
		private System.Windows.Forms.TextBox memoLog;
		private System.Windows.Forms.Label labelStatus;
		private System.Windows.Forms.ProgressBar progressOfJob;
		private System.Windows.Forms.Label labelJob;
		private System.Windows.Forms.Label labelLog;
		private System.Timers.Timer timerStart;

		//*********************** const ***************************
		private const int MAX_CHARS_TO_SHOW_FROM_LOG = 64 * 1024;
		private const string NO_FLASH_DRIVE = "[no flash drive found]";
		

		//**********************************************************
		private string m_sConfigFileName = ""; //Name of file containing jobs
		private string m_sJobName = ""; //Currently processed job name
		private string m_sDestinationDirToSaveBaksTo = ""; //Dir to save baks to
		private string m_sArchiverCmd;
		private string m_sArchiverParamsTempl;
		private string m_sArcFilePrefix;
		private string m_sArcSubdir;
		private string m_sSummary;


		private string m_sOSCommandProcessor;
		private string m_sArcFileExt;

		private bool m_bWriteStdErrToLog;
    private bool m_bDeleteFiles = false;
		private bool m_bSilentMode = false;

		private string m_sLogFileName;
		private string m_sSummaryFileName;
		private string m_sLogFileFullName;
		private IntPtr m_hLogFile = IntPtr.Zero;
		private CLog m_AppLog;

		private int m_nSimulateDate;
		private int m_nBakFilesCountLimit;
		private long m_dwlMaxDiskSpaceToUse;
		private long m_dwlReservedDiskSpace;
		private double m_fltExtraSpaceFactor;
		private double m_fltProgressionBase;
		private int m_nProgressionStart;
		private int m_nAllBacksWeight;
		private DateTime m_dateBak;
		private string m_sBackupFileName;
		private string m_sBackupFileNameWithPath;
		private string m_sArcSubdirFullName;
		private string m_sBakDevice="";

		private XmlDocument m_xmlConfig;
		private XmlElement m_xmlnodeConfigRootElem;
		private XmlElement m_xmlnodeJob;

		private BAK_ITEM_DEF []  m_vecItems;
		private ArrayList m_vecBakFiles;
		private ArrayList m_vecMarkedBakFiles;

		private bool m_bIsAppTerminated;

		private int m_nJobNo; //Current job number
		private int m_nJobCount;
		private ErrorNotificator m_ErrNotifier = null;
		private string m_sFlash = ""; //Flash drive or directory to copy backups to for reliability.
		private string m_sNotifyURL;

		private bool m_bIsTest; //Indicates that the config file is a file for debugging

		private System.Windows.Forms.MainMenu menuOper;
		private System.Windows.Forms.MenuItem mnitemRunJob;
		private System.Windows.Forms.MenuItem mnitemDeleteOldBaks;
		private System.Windows.Forms.MenuItem mnitemOper;

		
		private System.Windows.Forms.MenuItem mnitemProg;
		private System.Windows.Forms.MenuItem meitemTest;
		private System.Windows.Forms.Button buttonRefresh;
		private System.Windows.Forms.Label labelRefreshStatus;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//------------------------------------------------------------------------------------
		public CformBak()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			m_xmlConfig = new XmlDocument();
			m_AppLog = new CLog();
			m_dateBak = DateTime.Now;
			m_vecBakFiles = new ArrayList();
			m_vecMarkedBakFiles = new ArrayList();
			m_ErrNotifier = new ErrorNotificator();

			m_bIsAppTerminated = false;

			Application.ApplicationExit += new EventHandler(this.OnApplicationExit);

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
			this.menuOper = new System.Windows.Forms.MainMenu();
			this.mnitemOper = new System.Windows.Forms.MenuItem();
			this.mnitemRunJob = new System.Windows.Forms.MenuItem();
			this.mnitemDeleteOldBaks = new System.Windows.Forms.MenuItem();
			this.mnitemProg = new System.Windows.Forms.MenuItem();
			this.meitemTest = new System.Windows.Forms.MenuItem();
			this.memoJob = new System.Windows.Forms.TextBox();
			this.labelJob = new System.Windows.Forms.Label();
			this.memoLog = new System.Windows.Forms.TextBox();
			this.labelStatus = new System.Windows.Forms.Label();
			this.labelLog = new System.Windows.Forms.Label();
			this.progressOfJob = new System.Windows.Forms.ProgressBar();
			this.timerStart = new System.Timers.Timer();
			this.buttonRefresh = new System.Windows.Forms.Button();
			this.labelRefreshStatus = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.timerStart)).BeginInit();
			this.SuspendLayout();
			// 
			// menuOper
			// 
			this.menuOper.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																																						 this.mnitemOper,
																																						 this.mnitemProg});
			// 
			// mnitemOper
			// 
			this.mnitemOper.Index = 0;
			this.mnitemOper.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																																							 this.mnitemRunJob,
																																							 this.mnitemDeleteOldBaks});
			this.mnitemOper.Text = "Operations";
			// 
			// mnitemRunJob
			// 
			this.mnitemRunJob.Index = 0;
			this.mnitemRunJob.Text = "Run Jobs";
			this.mnitemRunJob.Click += new System.EventHandler(this.mnitemRunJob_Click);
			// 
			// mnitemDeleteOldBaks
			// 
			this.mnitemDeleteOldBaks.Index = 1;
			this.mnitemDeleteOldBaks.Text = "Delete old backups...";
			this.mnitemDeleteOldBaks.Click += new System.EventHandler(this.mnitemDeleteOldBaks_Click);
			// 
			// mnitemProg
			// 
			this.mnitemProg.Index = 1;
			this.mnitemProg.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																																							 this.meitemTest});
			this.mnitemProg.Text = "Prog";
			// 
			// meitemTest
			// 
			this.meitemTest.Index = 0;
			this.meitemTest.Text = "Test";
			this.meitemTest.Click += new System.EventHandler(this.meitemTest_Click);
			// 
			// memoJob
			// 
			this.memoJob.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.memoJob.Location = new System.Drawing.Point(0, 16);
			this.memoJob.Multiline = true;
			this.memoJob.Name = "memoJob";
			this.memoJob.ReadOnly = true;
			this.memoJob.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.memoJob.Size = new System.Drawing.Size(776, 128);
			this.memoJob.TabIndex = 0;
			this.memoJob.Text = "memoJob";
			// 
			// labelJob
			// 
			this.labelJob.Location = new System.Drawing.Point(0, 2);
			this.labelJob.Name = "labelJob";
			this.labelJob.Size = new System.Drawing.Size(448, 13);
			this.labelJob.TabIndex = 1;
			this.labelJob.Text = "Job:";
			// 
			// memoLog
			// 
			this.memoLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.memoLog.Location = new System.Drawing.Point(1, 168);
			this.memoLog.MaxLength = 0;
			this.memoLog.Multiline = true;
			this.memoLog.Name = "memoLog";
			this.memoLog.ReadOnly = true;
			this.memoLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.memoLog.Size = new System.Drawing.Size(775, 176);
			this.memoLog.TabIndex = 2;
			this.memoLog.Text = "memoLog";
			// 
			// labelStatus
			// 
			this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.labelStatus.Location = new System.Drawing.Point(0, 373);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(768, 16);
			this.labelStatus.TabIndex = 3;
			this.labelStatus.Text = "labelStatus";
			// 
			// labelLog
			// 
			this.labelLog.Location = new System.Drawing.Point(1, 149);
			this.labelLog.Name = "labelLog";
			this.labelLog.Size = new System.Drawing.Size(471, 16);
			this.labelLog.TabIndex = 4;
			this.labelLog.Text = "Log (includes archiver output):";
			// 
			// progressOfJob
			// 
			this.progressOfJob.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.progressOfJob.Location = new System.Drawing.Point(0, 392);
			this.progressOfJob.Name = "progressOfJob";
			this.progressOfJob.Size = new System.Drawing.Size(768, 16);
			this.progressOfJob.TabIndex = 5;
			// 
			// timerStart
			// 
			this.timerStart.Enabled = true;
			this.timerStart.Interval = 500;
			this.timerStart.SynchronizingObject = this;
			this.timerStart.Elapsed += new System.Timers.ElapsedEventHandler(this.timerStart_Elapsed);
			// 
			// buttonRefresh
			// 
			this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonRefresh.Location = new System.Drawing.Point(0, 347);
			this.buttonRefresh.Name = "buttonRefresh";
			this.buttonRefresh.TabIndex = 6;
			this.buttonRefresh.Text = "Refresh";
			this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
			// 
			// labelRefreshStatus
			// 
			this.labelRefreshStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelRefreshStatus.Location = new System.Drawing.Point(78, 352);
			this.labelRefreshStatus.Name = "labelRefreshStatus";
			this.labelRefreshStatus.Size = new System.Drawing.Size(485, 16);
			this.labelRefreshStatus.TabIndex = 7;
			this.labelRefreshStatus.Text = "label1labelRefreshStatus";
			// 
			// CformBak
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(776, 409);
			this.Controls.Add(this.labelRefreshStatus);
			this.Controls.Add(this.buttonRefresh);
			this.Controls.Add(this.progressOfJob);
			this.Controls.Add(this.labelLog);
			this.Controls.Add(this.labelStatus);
			this.Controls.Add(this.memoLog);
			this.Controls.Add(this.labelJob);
			this.Controls.Add(this.memoJob);
			this.Menu = this.menuOper;
			this.Name = "CformBak";
			this.Text = "Backuper C#";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.CformBak_Closing);
			this.Load += new System.EventHandler(this.CformBak_Load);
			((System.ComponentModel.ISupportInitialize)(this.timerStart)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new CformBak());
		}

		//------------------------------------------------------------------------------------
		//Executed on creating main form
		private void CformBak_Load(object sender, System.EventArgs e)
		{
			timerStart.Enabled = true;
			labelJob.Text = "Jobs:";
			this.Text = "Bak.NET (c) Mirco Logic corp.";
			memoJob.Text = "";
			labelStatus.Text = "Starting...";
			memoLog.Text = "";
			labelRefreshStatus.Text = "";
			labelLog.Text = "Log (includes archiver output) last " + MAX_CHARS_TO_SHOW_FROM_LOG.ToString() + " characters:" ;
			mnitemOper.Enabled = false;
		}

		//------------------------------------------------------------------------------------
		//Start timer to begin backup a bit later after the form is created
		private void timerStart_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			mnitemProg.Visible = false; //True To debug
			timerStart.Enabled = false;

			if( m_bSilentMode )
			  SetStatus( "Running" );
			else
				SetStatus( "Please choose action from menu" );

			//Get configuration file from command line
			string [] arrCMDArgs;
			arrCMDArgs 	= Environment.GetCommandLineArgs();

			if( arrCMDArgs.Length > 1 )
			  m_sConfigFileName = arrCMDArgs[1];

			labelJob.Text = "Config file '" + m_sConfigFileName + "':";

			Application.DoEvents();

			//Processing command line parameters
			m_bDeleteFiles = m_bSilentMode = false;
			int nParamNo = 2;
			while( nParamNo < 5 && nParamNo < arrCMDArgs.Length  )
			{
				string sParam = arrCMDArgs[nParamNo].Trim();

				if( sParam == "-u" )
					m_bSilentMode = true;

				if( sParam == "-d" )
					m_bDeleteFiles = true;

				nParamNo++;
			}

			if( m_sConfigFileName == "" )
			{
				SetStatus( "XML config file not specified - please specify it in 1st command line parameter!" );
				Application.DoEvents();
				System.Threading.Thread.Sleep(2000);
				if( m_bSilentMode )
					Application.Exit();
				return;
			}

			//Load job XML file
			try
			{
				m_xmlConfig.Load( m_sConfigFileName );
			}
			catch( Exception E )
			{
			   SetStatus( Defs.ERROR_MARK + "XML config file " + m_sConfigFileName + " load error: " + E.Message );
			   if( m_bSilentMode )
				   Application.Exit();
			   return;
			 }

			m_AppLog.Add( "Processing XML config file " + m_sConfigFileName );

			//Show job source in job memo
			StreamReader strmConfig = new StreamReader( m_sConfigFileName );
			memoJob.Text = strmConfig.ReadToEnd();
			strmConfig.Close();

			mnitemOper.Enabled = true;

			if( m_bSilentMode )
			{ //Start processing and Exit;

				mnitemOper.Enabled = false;
				ProcessJobs();
				Application.Exit();
			}

		}

		//-----------------------------------------------------------------------------------
		private void AddSummary( string sLine )
		{
			m_sSummary += sLine + "\r\n";
		}

		//------------------------------------------------------------------------------------
		//Checks if a job item is disabled or not
		private bool IsItemDisabled( XmlElement xmlnod )
		{
			bool bResult = false;
			string sDisablingSwitch =xmlnod.GetAttribute("DisabledByCmdSwitch");
			string sEnablingSwitch = xmlnod.GetAttribute("EnabledByCmdSwitch");
			if( sDisablingSwitch != "" )
				bResult = Utils.SwitchIsPresentInCMD( sDisablingSwitch );
			if( sEnablingSwitch != "" )
				bResult = ! Utils.SwitchIsPresentInCMD( sEnablingSwitch );

			return bResult;

		}

		//------------------------------------------------------------------------------------
		private void SetStatus( string sStatusText )
	  {
		  labelStatus.Text = sStatusText;
	  }

		//------------------------------------------------------------------------------------
		//Testing, debugging features, etc
		private void meitemTest_Click(object sender, System.EventArgs e)
		{

			WebClient webCli = new WebClient();

			NameValueCollection PostData = new NameValueCollection();
			PostData.Add( "subject", "backup.net" );
			PostData.Add( "text", "need attention" );


			byte[] responseArray = webCli.UploadValues("http://www.pad2pad.com/_prog/notify/index.php","POST", PostData );

			string sResponse = Encoding.ASCII.GetString(responseArray);

			memoLog.Text += "\r\n" + sResponse + "\r\n";

		}

		//------------------------------------------------------------------------------------
		//Reads backup job params from the config file
		//Config file may contain a few backup jobs. Jobs differ by the destination directory (backup medias) to save backup item to
		private void ReadJobParams()
		{
			m_sDestinationDirToSaveBaksTo = m_xmlnodeJob.GetAttribute("DirToSaveBakTo" );

			if( !Directory.Exists( m_sDestinationDirToSaveBaksTo ) )
				m_sDestinationDirToSaveBaksTo = m_xmlnodeJob.GetAttribute("DirToSaveBakToAlt" );

			m_sDestinationDirToSaveBaksTo = m_sDestinationDirToSaveBaksTo.Trim();

			m_sJobName = m_xmlnodeJob.GetAttribute( "Name" );
			if( m_sJobName == "" )
				m_sJobName = "Job #" + m_nJobNo.ToString();
			
			m_sArcFilePrefix = m_xmlnodeJob.GetAttribute( "ArcFilePrefix" );
			m_sArcSubdir = m_xmlnodeJob.GetAttribute( "Directory" );
			m_sOSCommandProcessor =  m_xmlnodeJob.GetAttribute( "WindowsCmdProcessor" ).Trim();
			m_bWriteStdErrToLog = Convert.ToInt16( m_xmlnodeJob.GetAttribute( "WriteStdErrToLog" ) ) != 0;
			m_sBakDevice = m_xmlnodeJob.GetAttribute("BakDevice");

			if( m_sOSCommandProcessor == "" )
				m_sOSCommandProcessor = "c:\\windows\\system32\\cmd.exe";

			if( m_sArcFilePrefix == "" )
				m_sArcFilePrefix = "bak_";


			string sLogFileName = m_xmlnodeJob.GetAttribute( "LogFile" ).Trim();
			if( sLogFileName != "" ) //Override common param?
				m_sLogFileName = sLogFileName;

			string sSummaryFileName = m_xmlnodeJob.GetAttribute("SummaryFile");
			if( sSummaryFileName != "" )
				m_sSummaryFileName = sSummaryFileName;

			//This date is added to current date to simulate a feature date (for debugging)
			string sSimulateDate = m_xmlnodeJob.GetAttribute( "SimulateDate" );
			try { m_nSimulateDate = Convert.ToInt32( sSimulateDate ); } catch {}

			try{	m_nBakFilesCountLimit = Convert.ToInt32(  m_xmlnodeJob.GetAttribute("BakFilesCountLimit") );	}	catch	{	 m_nBakFilesCountLimit = 0;	}

			try{	m_dwlMaxDiskSpaceToUse = Convert.ToInt64( m_xmlnodeJob.GetAttribute("MaxDiskSpaceToUse") );	}	
			catch	
			{	 
				m_dwlMaxDiskSpaceToUse = 0; 
				m_AppLog.Add( Defs.WARNING + "Parameter \"MaxDiskSpaceToUse\" read error. Using default value 0." );
			}

			try	{	  m_dwlReservedDiskSpace = Convert.ToInt64( m_xmlnodeJob.GetAttribute("LeaveFreeDiskSpace") );	}
			catch
			{
				 m_dwlReservedDiskSpace = 12000000000;
				 m_AppLog.Add( Defs.WARNING + "Reserved disk space parameter \"LeaveFreeDiskSpace\" read error. Using default value" );
			}

			//Extra space factor to minimize risk of insufficient space in case of backup size estimation error.
			try{ m_fltExtraSpaceFactor = Convert.ToDouble( m_xmlnodeJob.GetAttribute( "ExtraSpaceFactor" ) ); } catch { m_fltExtraSpaceFactor = 2;	}

			//Bak files delete algorithm params
			try{ m_fltProgressionBase = Convert.ToDouble( m_xmlnodeJob.GetAttribute( "ProgressionBase" ) );	}	catch	{	m_fltProgressionBase = 1.3;	}
			try{ m_nProgressionStart = Convert.ToInt32(  m_xmlnodeJob.GetAttribute( "ProgressionStart" ) );	}	catch {	m_nProgressionStart = 2;	}
			
			//Read backup job items. Usually item defines a set of files to backup (a directory to backup).
			//But there might be special items like to execute a command, check a file to exist, etc.
			XmlNodeList nodelistBakItems = m_xmlnodeJob.ChildNodes;
			int nNodeCount = nodelistBakItems.Count;
			m_vecItems = new BAK_ITEM_DEF[nNodeCount];
			for( int nNodeIdx=0; nNodeIdx< nNodeCount; nNodeIdx++ )
			{
				XmlElement xmlnodCurrItem =  nodelistBakItems.Item( nNodeIdx ) as XmlElement;

				BAK_ITEM_DEF BakItem = new BAK_ITEM_DEF();
				BakItem.Init();


				if( xmlnodCurrItem == null)
				{
					m_vecItems[nNodeIdx] = BakItem;
					continue;
				}
				
				BakItem.sSource = xmlnodCurrItem.GetAttribute("location"); //Location to backup
				BakItem.sCheckFile = xmlnodCurrItem.GetAttribute("checkfile"); //Check file to exist item
				BakItem.sCheckDir = xmlnodCurrItem.GetAttribute("checkdir"); //Check dir to exist item
				BakItem.sWait = xmlnodCurrItem.GetAttribute("wait"); //Suspend to 'wait' seconds
				BakItem.sArcFilePrefix =  xmlnodCurrItem.GetAttribute("ArcFilePrefix"); //Archive backup file porefix (the rest of the name is date)
				BakItem.sArcSubdir = xmlnodCurrItem.GetAttribute("Directory");

				//Item weight - just to show in progress bar
				try{ BakItem.nWeightAbstract = Convert.ToInt32( xmlnodCurrItem.GetAttribute("Weight") ); } catch {BakItem.nWeightAbstract = 0;}

				//Max time the app waits for archiver to finish archiving an item
				try{ BakItem.nMaxWaitTime_Sec =  Convert.ToInt32( xmlnodCurrItem.GetAttribute("MaxWaitTime_Sec") );	} catch {	BakItem.nMaxWaitTime_Sec = 7200; }

				BakItem.bDisabled = IsItemDisabled( xmlnodCurrItem );

				//Command to execute after item backup is done
				XmlElement xmlnodAfterCmd = null;
				try{	xmlnodAfterCmd = xmlnodCurrItem.SelectSingleNode( "aftercmd" ) as XmlElement;	}	catch {	xmlnodAfterCmd = null; }

				if( xmlnodAfterCmd != null )
				{
					BakItem.sAfterCMDEXE = xmlnodAfterCmd.GetAttribute("Exe");
					BakItem.sAfterCMDPARAMS = xmlnodAfterCmd.GetAttribute("Params");
					try {	BakItem.bAfterCmdShowWindow = Convert.ToInt16( xmlnodAfterCmd.GetAttribute("ShowWindow") ) != 0;	}	catch	{	BakItem.bAfterCmdShowWindow = false;	}
					try { BakItem.bAfterCmdNowait = Convert.ToInt16( xmlnodAfterCmd.GetAttribute("nowait")) != 0; } catch { BakItem.bAfterCmdNowait=false;}
					string sAfterCMDDisablingSwitch =xmlnodAfterCmd.GetAttribute("DisabledByCmdSwitch");
					string sAfterCMDEnablingSwitch = xmlnodAfterCmd.GetAttribute("EnabledByCmdSwitch");

					BakItem.bDisabled = IsItemDisabled( xmlnodAfterCmd );

					try{ BakItem.bAfterCMDWriteStdErrToLog = Convert.ToInt16(xmlnodAfterCmd.GetAttribute("WriteStdErrToLog")) != 0; } catch {BakItem.bAfterCMDWriteStdErrToLog=false;}

				}

				m_vecItems[nNodeIdx] = BakItem;
			}

		}

    //------------------------------------------------------------------------------------
	  private string MkErrorMsg( string sMessage )
		{
			return "Job '" + m_sJobName + "': " + sMessage;
		}

		//------------------------------------------------------------------------------------
		private bool CheckJobParams()
	  {
			string sMsg;
			if( m_sDestinationDirToSaveBaksTo == "" )
			{
				sMsg = "DirToSaveBakTo: Destination directory to store backups in is not specified";
				SetStatus( sMsg );
				m_ErrNotifier.Add( MkErrorMsg( sMsg ) );
				return false;
			}

			if( m_sArchiverCmd == "" )
			{
				sMsg = "Archiver: Not specified";
				m_ErrNotifier.Add( MkErrorMsg( sMsg ) );
				SetStatus( sMsg );
				return false;
			}

			if( m_vecItems.Length == 0 )
			{
				sMsg = "Objects to be backed up not specified.";
				m_ErrNotifier.Add( MkErrorMsg( sMsg ) );
				SetStatus( sMsg );
				return false;
			}
	
			return true;
		}

		//---------------------------------------------------------------------------
		private void WriteErrorToBakLog( string sText )
		{
			sText = Utils.GetTimeStamp() + Defs.ERROR_MARK + " " + sText;
			m_ErrNotifier.Add( sText );
			WriteBakLog( sText + "\r\n" );
		}

		//---------------------------------------------------------------------------
		private void WriteBakLog( string sText )
		{
			if( m_hLogFile == IntPtr.Zero )
			{
				m_AppLog.Add( sText );
				return;
			}

			Encoder enc = Encoding.Default.GetEncoder();
				
			char[] charArr = sText.ToCharArray();
			byte[] byteArr = new byte[enc.GetByteCount(charArr, 0, charArr.Length, true)];
				
			enc.GetBytes(charArr, 0, charArr.Length, byteArr, 0, true); //Recoding

				
			long nDummyLong;
			WinAPI.SetFilePointerEx( m_hLogFile, 0, out nDummyLong,(uint) WinAPI.FILE_POINTER.FILE_END );
			int nDummy;
			WinAPI.WriteFile( m_hLogFile,  byteArr , byteArr.Length, out nDummy, IntPtr.Zero  );

			RefreshBakLog();

		}

		//---------------------------------------------------------------------------
		private void SetBakFileName()
	  {
			DateTime dateNow = m_dateBak;
			dateNow = dateNow.AddDays( (double) m_nSimulateDate );

			 //Warning! If you change this also change the regular expression in FreeDiskSpaceByDeletingOldBackups
			m_sBackupFileName = string.Format( "{0}{1}_{2}_{3}.{4}", m_sArcFilePrefix, dateNow.Month, dateNow.Day, dateNow.Year, m_sArcFileExt );
			
			m_sArcSubdirFullName = GetFileNameInDestDir( m_sArcSubdir  );

			if( m_sArcSubdir != "" && ! Directory.Exists( m_sArcSubdirFullName ) ) //Create sub dir
			{
				try
				{
					Directory.CreateDirectory(m_sArcSubdirFullName ) ;
				}
				catch( System.Exception e )
				{
					WriteErrorToBakLog( "Could not create directory '" + m_sArcSubdirFullName + "': " + e.Message + ". Will save backup to '" + this.m_sDestinationDirToSaveBaksTo + "'." );
					m_sArcSubdir = "";
				}
			}

			m_sBackupFileNameWithPath = GetFileNameInDestDir( m_sBackupFileName, m_sArcSubdir );

			WriteBakLog( "\r\n\r\n" + Utils.GetTimeStamp() + " ############# Now backing up to: " + m_sBackupFileNameWithPath + " ###################\r\n\r\n");

			AddSummary("");
			if( m_sArcSubdir != "" )
			  AddSummary( m_sBakDevice +  m_sArcSubdir + " \\ " +  m_sBackupFileName );
			else
				AddSummary( m_sBakDevice + m_sBackupFileName );
	 }

		//---------------------------------------------------------------------------
		//Returns a list of items that will be backed up
		private string GetBackupObjects()
		{
		  string sResult = "";
			foreach( BAK_ITEM_DEF BakItem in m_vecItems )
			{
				if( BakItem.sSource != "" )
					sResult += BakItem.sSource + "\r\n";
			}
			return sResult;
	 }


		//------------------------------------------------------------------------------------
		//Create a backup log file which saved together with backup files
		private void CreateBakLogFile()
		{
			WinAPI.SECURITY_ATTRIBUTES si = new WinAPI.SECURITY_ATTRIBUTES();
			WinAPI.InitSECURITY_ATTRIBUTES( ref si );
			si.bInheritHandle = 1;

			m_hLogFile = WinAPI.CreateFile(
				m_sLogFileFullName, 
				(uint) WinAPI.GENERIC_ACCESS.GENERIC_WRITE | (uint) WinAPI.GENERIC_ACCESS.GENERIC_READ,
				(uint) WinAPI.FILE_SHARE.FILE_SHARE_READ | (uint) WinAPI.FILE_SHARE.FILE_SHARE_WRITE,
				ref si,
				(uint) WinAPI.CREATE_FILE.CREATE_ALWAYS,
				(uint) WinAPI.FILE_ATTR.FILE_ATTRIBUTE_NORMAL,
				IntPtr.Zero
			);

			
			if( (uint) m_hLogFile == WinAPI.INVALID_HANDLE_VALUE )
					m_hLogFile = IntPtr.Zero;

			if( m_hLogFile == IntPtr.Zero )
			{
				string sMsg = Defs.ERROR_MARK + "Log file " + m_sLogFileFullName + " creation error" + WinAPI.GetLastWindowsError();
				m_AppLog.Add( sMsg );
				m_ErrNotifier.Add( MkErrorMsg(sMsg) );
			}


			string sMessage = 
				Defs.APP_VERSION + "\r\n" +
        "========== Backup job '" + m_sConfigFileName + "'.'" + m_sJobName + "' started at " + DateTime.Now.ToString() + " ==========\r\n"+
																	 "Backup objects:\r\n" + GetBackupObjects() + "\r\n";

			 WriteBakLog( sMessage );
	 }

		//---------------------------------------------------------------------------
		//In the specified directory and all subdirectories
		private void GetAllFilesInDirectory( string sDirectory, string sFileMask, ref ArrayList vecFiles )
	  {
			string [] arrFiles = null;

			try
			{
				arrFiles = Directory.GetFiles( sDirectory, sFileMask );
			}
			catch
			{

			}

			if( arrFiles != null )
			  foreach( string sFile in arrFiles )
				  vecFiles.Add( sFile );


			//Subdirectories
			string [] arrDirs =null;
			try
			{
				arrDirs = Directory.GetDirectories( sDirectory );
			}
			catch
			{
				
			}
			
			if( arrDirs != null )
			  foreach( string sDir in arrDirs )
				  GetAllFilesInDirectory( sDir, sFileMask, ref vecFiles ); //Recursion


		}
		

		//---------------------------------------------------------------------------
		// Reads existing backup files to structures for analysis by old backups deleting algorithm
		private void ReadExistingBakFiles()
		{
			Regex RExp = new Regex( ".+?(\\d+)_(\\d+)_(\\d\\d\\d\\d)\\." + m_sArcFileExt + "$", RegexOptions.IgnoreCase );

			ArrayList vecFiles = new ArrayList();
			vecFiles.Clear();

			GetAllFilesInDirectory(m_sDestinationDirToSaveBaksTo,  "*." +m_sArcFileExt, ref vecFiles );
			
			m_vecBakFiles.Clear();

			foreach( string sFile in vecFiles )
			{
				Match match = RExp.Match( sFile );
				if( !match.Success )
					continue;

				//Date of file is encoded in the file name
				int nMon = Convert.ToInt32( match.Groups[1].Value );
				int nDay = Convert.ToInt32( match.Groups[2].Value );
				int nYear = Convert.ToInt32( match.Groups[3].Value );

				//One date backup files info
				BAK_FILE_DEF FileDef = new BAK_FILE_DEF();
				FileDef.dateOfFile = new DateTime( nYear, nMon, nDay );

				//Info of files in one day backup
				BAK_SUB_FILE SubFile = new BAK_SUB_FILE(0);		

				SubFile.sFileName = sFile;

				FileInfo finf = new FileInfo( sFile );
				SubFile.Size = finf.Length;

				//Search if there is already file info for the date
				BAK_FILE_DEF exFileDef = null;
				for( int i=0; i<m_vecBakFiles.Count; i++)
				{
					BAK_FILE_DEF CurrFileDef = (BAK_FILE_DEF) m_vecBakFiles[i];
					if( FileDef.dateOfFile == CurrFileDef.dateOfFile)
					{
						exFileDef = CurrFileDef; //Use existing
						break;
					}
				}

				if( exFileDef == null )
				{ //Not found - use new
					m_vecBakFiles.Add( FileDef );
					exFileDef = FileDef;
				}

				exFileDef.vecSubFiles.Add( SubFile );
			}

			SortBakFilesListByDate( ref m_vecBakFiles );
		}

		//---------------------------------------------------------------------------
		// Just sort the list by date
	private	void SortBakFilesListByDate( ref ArrayList  vec_Files )
	{
		for( int i=0; i< vec_Files.Count; i++ )
			for( int j =i+1; j < vec_Files.Count; j++ )
			{
				BAK_FILE_DEF BakA = (BAK_FILE_DEF) vec_Files[i];
				BAK_FILE_DEF BakB = (BAK_FILE_DEF) vec_Files[j];
				if( BakA.dateOfFile < BakB.dateOfFile )
				{
					object obj = vec_Files[i];
					vec_Files[i] = vec_Files[j];
					vec_Files[j] = obj;
				}
			}
		}


		//---------------------------------------------------------------------------
		private void ScrollMemoToEnd( System.Windows.Forms.TextBox memo )
		{
			try
			{
				memo.Select( memo.Text.Length, 0 );
				memo.ScrollToCaret();
			}
			catch 
			{
				
			}
		}

		//---------------------------------------------------------------------------
		//For estimation future backup size
		private long GetMaxSizeOfLast3Backups()
		{
		  int i=0;
			long dwlResult = 0;
			while( i < m_vecBakFiles.Count && i < 3  )
			{
				BAK_FILE_DEF BakFile = (BAK_FILE_DEF) m_vecBakFiles[i];
				if( BakFile.GetSize() > dwlResult )
					dwlResult = BakFile.GetSize();
			  i++;
			}
			return dwlResult;
	  }


		//-------------------------------------------------------------------------------
		private void FreeDiskSpaceByDeletingOldBackups()
		{
			 if( m_vecBakFiles.Count == 0 )
					 return; //No bak files - nothing to delete

			double dMaxSizeOfLast3Backups =  (double) GetMaxSizeOfLast3Backups();


			WriteBakLog( "Max size of last 3 backups:  " + dMaxSizeOfLast3Backups.ToString() + "\n" );
			WriteBakLog( "Extra space factor:  " + m_fltExtraSpaceFactor.ToString() + "\n" );

			long nPrognozeSizeNeedingToCreateBackup =  (long) Math.Ceiling( dMaxSizeOfLast3Backups  * m_fltExtraSpaceFactor );

			DeleteOldBakToFreeSpace( nPrognozeSizeNeedingToCreateBackup, GetSizeOfAllBackups() );
		}

		//--------------------------------------------------------------------
		private void DeleteOldBakToFreeSpace( long dwlPrognozeBakSizeInBytes, long dwlAllBackupSize )
		{
			long FreeBytesAvailableToCaller;
			long TotalNumberOfBytes;
			long TotalNumberOfFreeBytes;

			WinAPI.GetDiskFreeSpaceEx( m_sDestinationDirToSaveBaksTo, out FreeBytesAvailableToCaller,
					out TotalNumberOfBytes,
					out TotalNumberOfFreeBytes );

			WriteBakLog( Utils.GetTimeStamp() + "\r\nExisting backup files:\r\n"  );

			foreach( object FileObj in m_vecBakFiles )
			{
				BAK_FILE_DEF FileDef = (BAK_FILE_DEF) FileObj;
				WriteBakLog( FileDef.GetFileNames() + " " + FileDef.GetSize().ToString() + "\r\n" );
			}

			WriteBakLog( "\n" );

			WriteBakLog( "Required disk space:  " + dwlPrognozeBakSizeInBytes.ToString() + "\r\n" );
			WriteBakLog( "Available disk space: " + FreeBytesAvailableToCaller.ToString() + "\r\n" );

			long dwlBytesAvailableForBackup = (FreeBytesAvailableToCaller > m_dwlReservedDiskSpace)? FreeBytesAvailableToCaller - m_dwlReservedDiskSpace : 0;
			long dwlBytesDesignatedForBackup = (m_dwlMaxDiskSpaceToUse>dwlAllBackupSize)?   m_dwlMaxDiskSpaceToUse - dwlAllBackupSize : 0;

			if(m_dwlMaxDiskSpaceToUse > 0 )
				WriteBakLog( "Max disk space to use: " +   m_dwlMaxDiskSpaceToUse.ToString() + "\r\n" );

			if( m_dwlMaxDiskSpaceToUse > 0 && dwlBytesAvailableForBackup > dwlBytesDesignatedForBackup )
			{
				dwlBytesAvailableForBackup =  dwlBytesDesignatedForBackup;
				WriteBakLog( "Space designated for backup: " +   dwlBytesDesignatedForBackup.ToString() + "\r\n" );
			}

			WriteBakLog( "Bytes available for backup: " + dwlBytesAvailableForBackup.ToString() + "\r\n" );

			if( dwlPrognozeBakSizeInBytes < dwlBytesAvailableForBackup )
			{
				//Check if we don't exceed number of backup files limit (usually this param is not used)
				if( (m_nBakFilesCountLimit == 0) || (m_vecBakFiles.Count <= m_nBakFilesCountLimit ) )
				{
					 WriteBakLog( "Current backup files count: " + m_vecBakFiles.Count.ToString() + " (limit: " + m_nBakFilesCountLimit.ToString() + ").\r\n" );
					 WriteBakLog( "No need to free space.\r\n" );
					 return; //No need to delete - there is enough space
			  }
			  else
				  WriteBakLog( "Backup files count " + m_vecBakFiles.Count.ToString() + " exceeds the limit " + m_nBakFilesCountLimit.ToString() + ".\r\n" );

			}

			WriteBakLog( "Deleting some old backup file(s) to free disk space.\r\n" );

			DeleteBackupFiles();

		}

		//-------------------------------------------------------------------
		//Uses a special progression algorithm to delete files
		//If the algorithm doesn't find any files to delete - just delete oldest backup
		private void DeleteBackupFiles()
		{
			int nN = 0;
			DateTime timeNow = DateTime.Now;
			timeNow = timeNow.AddDays( m_nSimulateDate );
			ArrayList vecFilesToDelete = new ArrayList();
			ArrayList vecFilesInInterval = new ArrayList();

			if( m_nJobCount > 1 )
			  SetStatus( "Deleting old backups of job '" + m_sJobName + "'." );
			else
				SetStatus( "Deleting old backups" );

			string sInfo = Utils.GetTimeStamp() + "\r\nFiles on disk:\r\n";

			if( m_vecBakFiles.Count == 0 )
			{
				string sMessage = "No files to delete in " + m_sDestinationDirToSaveBaksTo;
				sInfo += sMessage;
				WriteBakLog( sInfo );
				return;
			}

			foreach( object BakFileObj in m_vecBakFiles )
			{
				BAK_FILE_DEF BakFile = (BAK_FILE_DEF) BakFileObj;
				sInfo += "(" +  BakFile.dateOfFile.ToShortDateString()  + ") " + BakFile.GetFileNames() + "\r\n";
			}


			sInfo +=  "\r\nFiles to delete/keep:\r\n";


			vecFilesToDelete.Clear();
			double dPriorIntervalBegin =0;
			double dPriorIntervalEnd = 0;
			while( nN < 5 * 365 )
			{

				nN++;
				//Get interval boundaries
				double dDaysAgoIntervalBegin = Math.Floor( Math.Pow( 1.3, nN ) );
				double dDaysAgoIntervalEnd =  Math.Floor( Math.Pow( 1.3, nN+1 ) );

				if( dPriorIntervalBegin == dDaysAgoIntervalBegin &&  dPriorIntervalEnd == dDaysAgoIntervalEnd  )
					continue;


				dPriorIntervalBegin = dDaysAgoIntervalBegin;
				dPriorIntervalEnd = dDaysAgoIntervalEnd;

				if( dDaysAgoIntervalBegin == dDaysAgoIntervalEnd )
					continue;

				sInfo +=  ((int)dDaysAgoIntervalBegin).ToString() + " - " + ((int)dDaysAgoIntervalEnd).ToString() + " days ago:\r\n";

				DateTime timeIntervalBegin = timeNow.AddDays( -dDaysAgoIntervalBegin );
				DateTime timeIntervalEnd = timeNow.AddDays( - dDaysAgoIntervalEnd );

				vecFilesInInterval.Clear();

				BAK_FILE_DEF LastBak = (BAK_FILE_DEF) m_vecBakFiles[m_vecBakFiles.Count-1];

				if( timeIntervalBegin < LastBak.dateOfFile )
					break;

				//Get all files whose date fall in the current interval (not including end of interval)
				foreach ( object BakFileObj in m_vecBakFiles  )
				{
					BAK_FILE_DEF BakFile = (BAK_FILE_DEF) BakFileObj;
					if( BakFile.dateOfFile <= timeIntervalBegin && BakFile.dateOfFile > timeIntervalEnd )
						vecFilesInInterval.Add( BakFile );
				}

				//Add files to delete skipping oldest file (the last)
				if( vecFilesInInterval.Count > 0 )
					for(int i=0; i<vecFilesInInterval.Count; i++ )
					{
						BAK_FILE_DEF FileInInterval = (BAK_FILE_DEF) vecFilesInInterval[i];
						if( i < vecFilesInInterval.Count-1 )
						{
							vecFilesToDelete.Add( FileInInterval );
							sInfo += "*DEL: ";
						}
						else
							sInfo += "KEEP: ";

						sInfo +=  "(" + FileInInterval.dateOfFile.ToShortDateString() + ") " + FileInInterval.GetFileNames() + "\r\n";

					}

			}

			bool bThereAreFilesToDelete = true;
			if( vecFilesToDelete.Count == 0 )
			{
				sInfo +=  "\r\nNo files to delete by the interval algorithm!\r\n" +
					"Deleting the oldest backup!\r\n";
				vecFilesToDelete.Add( m_vecBakFiles[m_vecBakFiles.Count-1] );

			}
			else
				sInfo = sInfo + "\r\n* These files will be deleted when you click 'Delete files'";

			bool bProceedToDelete = true;
			if( m_bSilentMode )
			{

				WriteBakLog( sInfo + "\r\n" );
				if( vecFilesToDelete.Count == 0 )
					return;

				WriteBakLog( "Silent mode enabled - consider 'Delete files' is clicked!\n" );

			}
			else
			{
				string sCaption = "Delete old backup files";
				if( m_nJobCount > 1 )
					sCaption = "Delete old backup files of job '" + m_sJobName+ "'.";
				bProceedToDelete = DeleteFilesDlg.Do( sInfo, sCaption, bThereAreFilesToDelete );
			}

			if( ! bProceedToDelete )
				return;

			sInfo = "Files deleted:\r\n";

			foreach( object BakFileObj in vecFilesToDelete )
			{
				BAK_FILE_DEF BakFile = BakFileObj as BAK_FILE_DEF;
				DeleteBakFilesOfOneDateBackup(ref BakFile,ref sInfo );
			}

			WriteBakLog( sInfo );
		}

		//-------------------------------------------------------------------
		private void DeleteBakFilesOfOneDateBackup( ref BAK_FILE_DEF  FileDef, ref string sLog)
		{
			foreach( object SubFileObj in FileDef.vecSubFiles )
			{
				BAK_SUB_FILE SubFile = (BAK_SUB_FILE) SubFileObj;
				string sFileName =  SubFile.sFileName;
				FileInfo finf = new FileInfo( sFileName );
				sLog +=  SubFile.sFileName;
				if( ! finf.Exists  )
					sLog +=  " - file does not exist!";
				else
				{
					try
					{
						finf.Delete();
					}
					catch (System.Exception e)
					{
						sLog += " - error deleting this file: " + e.Message;
					}
				}

				sLog += "\r\n";
			}

		}

		//---------------------------------------------------------------------------
		private long GetSizeOfAllBackups()
		{
			long dwlResult = 0;

			foreach( object Obj in m_vecBakFiles  )
			{
				BAK_FILE_DEF BakFile = (BAK_FILE_DEF) Obj;
				dwlResult += BakFile.GetSize();
			}

			return dwlResult;
		}

		private string ConvertDeviceName( string sPath )
		{
			string sDevice = Utils.GetDeviceName( sPath ).ToLower();
			string sDeviceName = m_xmlnodeConfigRootElem.GetAttribute( "Device_" + sDevice );
			return Utils.ReplaceDeviceName( sPath, sDeviceName );
		}


		//------------------------------------------------------------------------------------
		private void PrepareJob()
		{
			labelRefreshStatus.Text = "";

			CalculateAllBacksWeight();

			memoLog.Text = "Backup log.";

			m_sLogFileFullName = GetFileNameInDestDir( m_sLogFileName );

			CreateBakLogFile();

			m_sSummary = "";

			AddSummary( "Summary of backup job:" );
			AddSummary( "" );

			AddSummary( "Start: " + Utils.GetTimeStamp() );
			AddSummary(	"End:   {end_time}" );

			AddSummary( "" );

			AddSummary( "Control file: " + ConvertDeviceName( m_sConfigFileName) );

			AddSummary( "" );

			SetBakFileName();

			ReadExistingBakFiles();

			if( m_bDeleteFiles )
				FreeDiskSpaceByDeletingOldBackups();
			else
				WriteBakLog( "Automatic old backup files deletion is disabled.\r\n" );

			progressOfJob.Maximum = m_nAllBacksWeight;
			progressOfJob.Minimum = 0;
			progressOfJob.Value = 0;

 	  }

		//---------------------------------------------------------------------------
		private void ExecuteShellCommand( string sApp, string sParams, bool bWriteStdErrToLog, ushort wShowWindow, bool bWait, int nMaxWaitTime_Sec )
	  {
		  WinAPI.STARTUPINFO si = new WinAPI.STARTUPINFO();
			WinAPI.InitSTARTUPINFO(ref si );

		  si.dwFlags = (uint)WinAPI.STARTF.STARTF_USESHOWWINDOW |  (uint)WinAPI.STARTF.STARTF_USESTDHANDLES;
			si.wShowWindow = wShowWindow;
			if( m_hLogFile != IntPtr.Zero )
		   si.hStdOutput = m_hLogFile;
			else
				si.hStdOutput = WinAPI.GetStdHandle( (int) WinAPI.STD_HANDLE.STD_OUTPUT_HANDLE );
			si.hStdInput = WinAPI.GetStdHandle( (int) WinAPI.STD_HANDLE.STD_INPUT_HANDLE );

			if( bWriteStdErrToLog && m_hLogFile != IntPtr.Zero )
				si.hStdError = m_hLogFile;
			else
				si.hStdError = WinAPI.GetStdHandle( (int) WinAPI.STD_HANDLE.STD_ERROR_HANDLE );

			WinAPI.PROCESS_INFORMATION pi = new WinAPI.PROCESS_INFORMATION();
			WinAPI.InitPROCESS_INFORMATION( ref pi );

			string sCMD = sApp + " " + sParams;

			WriteBakLog( sCMD + "\r\n" );

			Application.DoEvents();

			RefreshBakLog();

			int nSuccess =  WinAPI.CreateProcess( sApp,
				sParams,
				IntPtr.Zero,IntPtr.Zero, //&SecAttrs, &SecAttrs,
				1,
				(uint) WinAPI.PRIORITY_CLASS.NORMAL_PRIORITY_CLASS	,
				IntPtr.Zero, null,
				ref si, out pi);


			if( nSuccess == 0 )
			{
				WriteErrorToBakLog(  "Failed to execute " + sCMD + ": " + WinAPI.GetLastWindowsError()  );
				return;
			}

			if( bWait )
			{
				uint ulExitCode;
				uint nWaitTimeQuant =  2000;
				uint nRefresTimeQuant = nWaitTimeQuant * 30;

				if( !m_bSilentMode )
				{
					nWaitTimeQuant = 500;
					nRefresTimeQuant = nWaitTimeQuant * 2;
				}


				int nMaxWaitTime_MSec =   nMaxWaitTime_Sec * 1000;
				uint nWaitTime = 0;
				uint nWaitForRefreshTime = 0;
				RefreshBakLog();

	      do
        {
	        ulExitCode = WinAPI.WaitForSingleObject( pi.hProcess, nWaitTimeQuant );
	        nWaitTime += nWaitTimeQuant;
	        nWaitForRefreshTime += nWaitTimeQuant;
	        if( nWaitForRefreshTime > nRefresTimeQuant )
          {
	          nWaitForRefreshTime = 0;
	          RefreshBakLog();
          }

	        Application.DoEvents();

        }while( ulExitCode == (uint) WinAPI.WAIT_OBJECT.WAIT_TIMEOUT && (! m_bIsAppTerminated) && nWaitTime < nMaxWaitTime_MSec );

	      if( (nWaitTime >= nMaxWaitTime_Sec && ulExitCode == (uint) WinAPI.WAIT_OBJECT.WAIT_TIMEOUT) || m_bIsAppTerminated )
        {

	        string sMessage;
	        if( m_bIsAppTerminated )
	          sMessage = "Backup aborted by terminating the backup utility. Backup is likely incomplete.";
		      else
	          sMessage = "Time out executing " + sCMD + "! Time out limit: " + nMaxWaitTime_Sec.ToString() + " seconds";
	        WriteErrorToBakLog( sMessage );
	        WinAPI.TerminateProcess( pi.hProcess, 0 );
        }
	      RefreshBakLog();
    }

	  WinAPI.CloseHandle( pi.hProcess );
   }


   //------------------------------------------------------------------------------------
		private void RefreshBakLog()
		{
			const int nBUF_SIZE = 4096;
			int nReadLastBytes = MAX_CHARS_TO_SHOW_FROM_LOG;
			byte [] buf = new byte[nBUF_SIZE+1];
			int nBytesRead;

			if( m_hLogFile == IntPtr.Zero )
				return;

			if( (uint) m_hLogFile == WinAPI.INVALID_HANDLE_VALUE )
				return;


			long nDummyLong;
			//Read last nReadLastBytes bytes, not the whole huge sized log file
			if( WinAPI.SetFilePointerEx( m_hLogFile, -nReadLastBytes, out nDummyLong,(uint) WinAPI.FILE_POINTER.FILE_END ) == 0 )
			  WinAPI.SetFilePointerEx( m_hLogFile, 0, out nDummyLong,(uint) WinAPI.FILE_POINTER.FILE_BEGIN );

			int nTotalBytesRead = 0;

			memoLog.Clear();

			while( true )
			{
				WinAPI.ReadFile( m_hLogFile, buf, nBUF_SIZE, out nBytesRead, IntPtr.Zero );
				if( nBytesRead <= 0 )
					break;

				nTotalBytesRead += nBytesRead;

				buf[nBytesRead] = 0; //null terminator

				memoLog.Text += Encoding.Default.GetString(buf);
			}

			 ScrollMemoToEnd( memoLog );

			 labelRefreshStatus.Text = "Last refreshed: " + DateTime.Now.ToString();

			Application.DoEvents();

	 }

		//----------------------------------------------------------------------------------------
		private string ProcessStringParams( string sValue, string sLocation )
		{
			
			string sCfgDir = Path.GetDirectoryName( m_sConfigFileName );
			if( sCfgDir != "" )
				 sCfgDir = sCfgDir + '\\';
			sValue = sValue.Replace( "{config_dir}", sCfgDir );
			sValue = sValue.Replace( "{bak}", m_sBackupFileNameWithPath );
			sValue = sValue.Replace( "{bakfull}", m_sBackupFileNameWithPath );
			sValue = sValue.Replace( "{baks}", m_sBackupFileName );
			sValue = sValue.Replace( "{bakshort}", m_sBackupFileName );
			sValue = sValue.Replace( "{dst}", m_sBackupFileNameWithPath );
			sValue = sValue.Replace( "{src}", sLocation );
			sValue = sValue.Replace( "{log}",m_sLogFileFullName );
			sValue = sValue.Replace( "{flash}", m_sFlash );
			sValue = sValue.Replace( "{ext}", m_sArcFileExt );

			return sValue;
		}

		//----------------------------------------------------------------------------------------
		private string ProcessStringParams( string sValue )
		{
			return ProcessStringParams( sValue, "" );
		}


		//----------------------------------------------------------------------------------------
		private void DetectWhereFlashDriveIs()
		{
			string [] arrDrives = m_sFlash.Split( new Char [] {','});
			m_sFlash = NO_FLASH_DRIVE;
			foreach( string sDrive in arrDrives )
				if( Directory.Exists( sDrive + @"\" ) )
				{
					m_sFlash = sDrive;
					break;
				}
		}

		//----------------------------------------------------------------------------------------
		//Params that are common for all backup jobs (some can be redefined in jobs definition)
		private void ReadCommonParams()
		{
			m_sNotifyURL = m_xmlnodeConfigRootElem.GetAttribute( "NotifyURL" ).Trim();

			m_sFlash = m_xmlnodeConfigRootElem.GetAttribute( "flash" ).Trim();
			DetectWhereFlashDriveIs();

			string sSimulateDate = m_xmlnodeConfigRootElem.GetAttribute( "SimulateDate" );
			try { m_nSimulateDate = Convert.ToInt32( sSimulateDate ); } catch { m_nSimulateDate = 0; }

			m_sArchiverCmd = m_xmlnodeConfigRootElem.GetAttribute( "Archiver" ).Trim();
			m_sArchiverParamsTempl = m_xmlnodeConfigRootElem.GetAttribute( "ArchiverParams" );
			m_sArcFileExt = m_xmlnodeConfigRootElem.GetAttribute( "ArcFileExtension" );

			m_sLogFileName = m_xmlnodeConfigRootElem.GetAttribute( "LogFile" );
			m_sSummaryFileName = m_xmlnodeConfigRootElem.GetAttribute( "SummaryFile" );
			if( m_sSummaryFileName == "" )
				m_sSummaryFileName = "bak_summary.txt";

			if( m_sArcFileExt == "" )
				m_sArcFileExt = "zip";

			string sTest = m_xmlnodeConfigRootElem.GetAttribute( "Test" ).ToUpper().Trim();

			m_bIsTest = sTest == "YES";

			m_ErrNotifier.Init( m_sNotifyURL, m_bIsTest );

		}


		//----------------------------------------------------------------------------------------
		private void ProcessJobs()
		{
			mnitemOper.Enabled = false;

			m_xmlnodeConfigRootElem = m_xmlConfig.DocumentElement;
			ReadCommonParams();

		
			XmlNodeList nodelistJobs = m_xmlnodeConfigRootElem.ChildNodes;
			m_nJobCount = nodelistJobs.Count;
			m_nJobNo = 1;
			for( int nNodeIdx=0; nNodeIdx< m_nJobCount; nNodeIdx++ )
			{
			   m_xmlnodeJob = nodelistJobs.Item( nNodeIdx ) as XmlElement;

				 ReadJobParams();
				 if( CheckJobParams() )
				 {
					 PrepareJob();
					 RunJob();
					 if( m_bIsAppTerminated )
						 break;
			   }

				 m_nJobNo++;

			}


			if( m_bSilentMode )
			{
				if( !	m_ErrNotifier.EmailEvents() )
					m_AppLog.Add( "Email errors failed!" ); 
			}
			else if( ! m_bIsAppTerminated )
			{
				m_ErrNotifier.ShowEvents( ref memoLog );
				ScrollMemoToEnd( memoLog );
			}

			SetStatus( "Done all." );
		
		}

		//------------------------------------------------------------------------------------
		private void ProcessAfterCmd( BAK_ITEM_DEF BakItem)
		{
			string sAfterCMDPARAMS =  ProcessStringParams( BakItem.sAfterCMDPARAMS );

			WriteBakLog( Utils.GetTimeStamp() + " Executing command after backup: " + BakItem.sAfterCMDEXE + " " +  sAfterCMDPARAMS + "\r\n" );

			ushort wShowWindow = (ushort) WinAPI.SHOWWINDOW.SW_HIDE;
			if(  BakItem.bAfterCmdShowWindow )
				wShowWindow = (ushort) WinAPI.SHOWWINDOW.SW_SHOW;


			ExecuteShellCommand( BakItem.sAfterCMDEXE,  sAfterCMDPARAMS, BakItem.bAfterCMDWriteStdErrToLog, wShowWindow, ! BakItem.bAfterCmdNowait, 7200 );

		}

		//------------------------------------------------------------------------------------
		private void SaveSummary()
		{
			string sSummaryFile = GetFileNameInDestDir( m_sSummaryFileName );
			try
			{
				StreamWriter strmSummary = new StreamWriter( sSummaryFile );
				strmSummary.Write( m_sSummary );
				strmSummary.Close();
			}
			catch( System.Exception e )
			{
				WriteErrorToBakLog( "Could not create summary file '" + sSummaryFile + "': " + e.Message );
			}
}

		//------------------------------------------------------------------------------------
		private void ProcessWait( BAK_ITEM_DEF BakItem)
		{
			int nWaitSecs = 0;
			try
			{
				nWaitSecs = Convert.ToInt32( BakItem.sWait );
				}
			catch
			{
				WriteErrorToBakLog( "item 'wait' parameter parse error: Integral number expected." );
				return;
			}

			WriteBakLog( Utils.GetTimeStamp() + " Waiting " + nWaitSecs.ToString() + " seconds..." );

			const int nWaitQuant_msec = 500;
			int nWait_msec = 0;
			while( nWait_msec < nWaitSecs * 1000 && !m_bIsAppTerminated )
			{
				System.Threading.Thread.Sleep( nWaitQuant_msec );
				nWait_msec += nWaitQuant_msec;
				Application.DoEvents();
			}

			WriteBakLog( "OK\r\n" );

		}


		//------------------------------------------------------------------------------------
		private void ProcessCheckFile( BAK_ITEM_DEF BakItem)
		{
			string sCheckFileName = ProcessStringParams( BakItem.sCheckFile );

			WriteBakLog( Utils.GetTimeStamp() + " Checking for file '" + sCheckFileName + "' to exist..." );

			FileInfo finf;
			try
			{
				finf = new FileInfo( sCheckFileName );
			}
			catch
			{
				WriteErrorToBakLog( "Can't check file: '" + sCheckFileName + "'."  );
				return;
			}

			if(  finf.Exists )
				WriteBakLog( "OK\r\n" );
			else
				WriteErrorToBakLog( "File does not exist: '" + sCheckFileName );


		}

		//------------------------------------------------------------------------------------
		private void ProcessCheckDir( BAK_ITEM_DEF BakItem)
		{
			string sCheckDirName = ProcessStringParams( BakItem.sCheckDir );

			if( sCheckDirName == NO_FLASH_DRIVE )
			{
				WriteErrorToBakLog( "No flash drive found." );
				return;
			}

			WriteBakLog( Utils.GetTimeStamp() + " Checking for directory '" + sCheckDirName + "' to exist..." );

			bool bExists = false;
			try
			{
				 bExists =  Directory.Exists( sCheckDirName );
					
			}
			catch
			{
				WriteErrorToBakLog( "Can't check directory: '" + sCheckDirName + "'."  );
				return;
			}

			if(  bExists )
				WriteBakLog( "OK\r\n" );
			else
				WriteErrorToBakLog(  "Directory does not exist: '" + sCheckDirName  );
		}

    //------------------------------------------------------------------------------------
		private void RunJob()
		{
			mnitemOper.Enabled = false;

			int nMaxItems = 0;
			foreach( BAK_ITEM_DEF BakItem in m_vecItems  )
			{
				if(   BakItem.sSource != "" )
					nMaxItems++;
			}

			int nItemNo = 1;
			foreach( BAK_ITEM_DEF CurrBakItem in m_vecItems  )
			{
				string sSource = CurrBakItem.sSource;
				string sParamLine =  m_sArchiverParamsTempl;
				string sArcFilePrefix = CurrBakItem.sArcFilePrefix;
				string sArcSubdir = CurrBakItem.sArcSubdir;

				if( 
					   (sArcFilePrefix != "" && sArcFilePrefix != m_sArcFilePrefix) 
					   || 
					  (sArcSubdir != "" && sArcSubdir != m_sArcSubdir)
					)
				{
					m_sArcFilePrefix = sArcFilePrefix;
					m_sArcSubdir = sArcSubdir.Trim();
					SetBakFileName();
				}

				if( sSource != "" )
				{//If backup source is specified - this is a normal backup item 

					string sStatus = "";
					if( m_nJobCount > 1 )
						sStatus = "Processing job " + m_nJobNo.ToString() + " of " + m_nJobCount.ToString() + ". ";

					sStatus += "Processing item " + nItemNo.ToString() + " of "  + nMaxItems.ToString() + ": " + sSource;

					SetStatus( sStatus );
					nItemNo++;

					
					sParamLine = ProcessStringParams( sParamLine, sSource );

					WriteBakLog( "\r\n\r\n\r\n" + Utils.GetTimeStamp() + "******************** Backing up: " + sSource + " ***********************\r\n\r\n" );

					AddSummary( "  " + ConvertDeviceName( sSource ) );

					//Run archiver to create backup file
					ExecuteShellCommand(  
						m_sArchiverCmd, 
						sParamLine, 
						m_bWriteStdErrToLog, 
						(ushort) WinAPI.SHOWWINDOW.SW_HIDE, 
						true, 
						CurrBakItem.nMaxWaitTime_Sec 
					);
				}

				if( m_bIsAppTerminated )
					break;

				if( !CurrBakItem.bDisabled )
				{
					if(  CurrBakItem.sAfterCMDEXE != "" )
						ProcessAfterCmd(CurrBakItem );

					if(  CurrBakItem.sWait != "" )
						ProcessWait(CurrBakItem );

					if( CurrBakItem.sCheckFile != "" )
						ProcessCheckFile( CurrBakItem );

					if( CurrBakItem.sCheckDir != "" )
						ProcessCheckDir( CurrBakItem );
				}


				progressOfJob.Increment( CurrBakItem.nWeightAbstract );
			}


			AddSummary( "" );

			if( m_ErrNotifier.AreThereErrors() )
			{
				AddSummary( "======== ERRORS:" );
				AddSummary( m_ErrNotifier.GetReport() );
			}
			else
				AddSummary( "No errors." );

			m_sSummary = m_sSummary.Replace( "{end_time}", Utils.GetTimeStamp() );

			SaveSummary();

			RefreshBakLog();

			WinAPI.CloseHandle( m_hLogFile );
			m_hLogFile = IntPtr.Zero;


			if( m_bIsAppTerminated )
				SetStatus( "Terminating...." );
			else	if( m_nJobCount > 1 )
			  SetStatus( "Done job '" + m_sJobName + "'." );
			else
				SetStatus( "Done job." );

		}

		//------------------------------------------------------------------------------------
		//Used as max value of progress bar
		private void CalculateAllBacksWeight()
	  {
			m_nAllBacksWeight = 0;
			foreach( BAK_ITEM_DEF BakItem in m_vecItems  )
				 m_nAllBacksWeight += BakItem.nWeightAbstract;
		}

		//------------------------------------------------------------------------------------
		private string GetFileNameInDestDir( string sFileName )
		{
			return m_sDestinationDirToSaveBaksTo + "\\" + sFileName;
		}


		//------------------------------------------------------------------------------------
		private string GetFileNameInDestDir( string sFileName, string sSubDir )
	  {
			sSubDir = sSubDir.Trim();
			if( sSubDir != "" )
				return m_sDestinationDirToSaveBaksTo + "\\" + sSubDir + "\\" + sFileName;
			else
	      return GetFileNameInDestDir( sFileName );
	  }

		//------------------------------------------------------------------------------------
		private void mnitemRunJob_Click(object sender, System.EventArgs e)
		{
			if( m_bSilentMode ) return;

			ProcessJobs();
		}

		//------------------------------------------------------------------------------------
		private void OnApplicationExit(object sender, EventArgs e) 
		{
			m_bIsAppTerminated = true;
		}

		//------------------------------------------------------------------------------------
		private void CformBak_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			OnApplicationExit( sender, e );
		}

		//------------------------------------------------------------------------------------
		private void buttonRefresh_Click(object sender, System.EventArgs e)
		{
			buttonRefresh.Enabled = false;
			Application.DoEvents();
			RefreshBakLog();
			buttonRefresh.Enabled = true;
			Application.DoEvents();
		}

		//------------------------------------------------------------------------------------
		// This is mainly for debugging. Usually backup deleted automatically by specifying -d cmd switch
		private void mnitemDeleteOldBaks_Click(object sender, System.EventArgs e)
		{
			if( m_bSilentMode ) return;

			m_xmlnodeConfigRootElem = m_xmlConfig.DocumentElement;
			XmlNodeList nodelistJobs = m_xmlnodeConfigRootElem.ChildNodes;
			m_nJobCount = nodelistJobs.Count;
			m_nJobNo = 1;
			for( int nNodeIdx=0; nNodeIdx< m_nJobCount; nNodeIdx++ )
			{
				m_xmlnodeJob = nodelistJobs.Item( nNodeIdx ) as XmlElement;

				ReadJobParams();
				m_bDeleteFiles = false;
				if( CheckJobParams() )
				{
					PrepareJob();
					m_bDeleteFiles = true;
					DeleteBackupFiles();
				}
				m_nJobNo++;
			}

			SetStatus( "Done deleting." );
		
		}

	}

  //====================================================================================
	// It's rather s struct but use class
	// Information about existing one date backup
	public class  BAK_FILE_DEF
	{
		public ArrayList vecSubFiles = null; //Set of file of one date backup
		public DateTime dateOfFile;

		//----------------------------------------------------------------------
		public BAK_FILE_DEF()
		{
			vecSubFiles = new ArrayList();
		}

		//----------------------------------------------------------------------
		public string GetFileNames()
		{
			string sResult = "";
			if( vecSubFiles.Count > 0 )
			{
				BAK_SUB_FILE SubFile = (BAK_SUB_FILE) vecSubFiles[0];
				sResult =SubFile.sFileName;
				for( int i=1; i<vecSubFiles.Count; i++ )
				{
					SubFile = (BAK_SUB_FILE) vecSubFiles[i];
					sResult = sResult + "," + SubFile.sFileName;
				}
			}
			return sResult;
		}

		//----------------------------------------------------------------------
		public long GetSize()
		{
			long nResult = 0;
			for( int i=0; i < vecSubFiles.Count; i++ )
			{
				BAK_SUB_FILE SubFile = (BAK_SUB_FILE) vecSubFiles[i];
				nResult += SubFile.Size;
			}
			return nResult;
		}
	}

}
////////////////////////////////////////////////////////////////////////
