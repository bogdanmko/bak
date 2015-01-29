using System;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;


//////////////////////////////////////////////////////////////////
// Collects errors and then allows them either to show or send
// by email 
//////////////////////////////////////////////////////////////////

namespace Bak
{
	/// <summary>
	/// Summary description for ErrorNotificator.
	/// </summary>
	public class ErrorNotificator
	{

		private string m_sNotifyURL; //URL of script that sends notification
		private ArrayList m_Errors;
		private bool m_bIsTest;

		//-------------------------------------------------------------
		public ErrorNotificator()
		{
			m_Errors = new ArrayList();
		}


		//-------------------------------------------------------------
		public void Init(  string sNotifyURL, bool bIsTest  )
		{
			m_sNotifyURL = sNotifyURL;
			m_bIsTest = bIsTest;
		}

   //-------------------------------------------------------------
		public void Add( string sEvent )
		{
			m_Errors.Add( sEvent );
		}

		//-------------------------------------------------------------
		public bool EmailEvents()
		{
			if( m_Errors.Count == 0 )
				return true;

			WebClient HTTP = new WebClient();

			NameValueCollection PostData = new NameValueCollection();
			PostData.Add( "subject", "Backup error" );
			PostData.Add( "text", GetReport() );
			PostData.Add( "test", m_bIsTest ? "1" : "0" );
			PostData.Add( "sender", "bak" );

			byte[] responseArray = HTTP.UploadValues( m_sNotifyURL,"POST", PostData );

			string sResponse = Encoding.ASCII.GetString(responseArray);

			return sResponse.IndexOf( "==OK==" ) >= 0;
		}

		//-------------------------------------------------------------
		public bool AreThereErrors()
		{
			return m_Errors.Count > 0;
		}

		//-------------------------------------------------------------
		public string GetReport()
		{
			string sResult = "";
			foreach(object ErrObj in m_Errors )
			{
				string sMsg = ErrObj as string;
				sResult += sMsg + "\r\n";
			}
			return sResult;

		}

		//-------------------------------------------------------------
		public void ShowEvents(ref  System.Windows.Forms.TextBox memo )
		{
			if( m_Errors.Count == 0 )
				return;

			memo.Text += "\r\n====================== ERRORS ==========================\r\n";
			foreach(object ErrObj in m_Errors )
			{
				string sMsg = ErrObj as string;
				memo.Text += sMsg + "\r\n";
			}
		}

	}
}
