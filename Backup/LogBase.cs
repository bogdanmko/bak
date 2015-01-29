using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Bak
{
	/// <summary>
	/// Summary description for LogBase.
	/// </summary>
	
	

	public class CLogBase
	{

		private bool m_bLogDate = true;
		protected bool m_bLogUser = true;
		private const string REXP_USER = "^(\\w)\\:(.*?)$";
		private string m_sDir = "";
		private long m_nMaxLogSize = 0;
		private string m_sFileName = "";
		private string m_sText = "";
		private string m_sBtwUserAndDate = "";
		private FileStream m_File;
		

		//------------------------------------------------------------------------------------------
		public CLogBase()
		{
			m_bLogDate = true;
			m_bLogUser = true;
		}

		//------------------------------------------------------------------------------------------
		public CLogBase(string sFileName, int nMaxLogSize)
		{
			Init( sFileName, nMaxLogSize );
		}

		//------------------------------------------------------------------------------------------
		public string GetLine( ref string sText, ref int nPos )
		{
			string sResult = "";
			bool bWasNewLine = false;
			while( nPos < sText.Length )
			{
				if( bWasNewLine )
				{
					if( sText[nPos] == '\r' )
						nPos++;
					break;
				}

				if( sText[nPos] == '\n' )
					bWasNewLine = true;
				else
					sResult += sText[nPos];
				nPos++;
			}
			return sResult;

		}

		//------------------------------------------------------------------------------------------
		public void InternalAdd()
		{
      string sUser = "bak";

			if( m_bLogUser || m_bLogDate )
			{
				m_sText.Replace( "\r", "<CR>" );
				m_sText.Replace( "\n", "<NL>" );
     	}
			else
			{
				m_sText.Replace( "\r", "" );
			}

			string sLogLine = "";

			if( m_bLogUser )
				sLogLine += sUser + " ";

			if( m_sBtwUserAndDate != "" )
				sLogLine += m_sBtwUserAndDate + " ";

			if( m_bLogDate )
			{
				DateTime now = DateTime.Now;
				sLogLine += now.ToString( "MM/dd/yy" ) + " ";
			}

			sLogLine += m_sText;



			  int nAttemptsCount = 0;
				bool bSuccess = false;

				while( ! bSuccess ) // Access denied resolving loop
				{
					bSuccess = true;
					try
					{
						StreamWriter stream = new StreamWriter( m_sFileName, true ); //, System.Text.Encoding.GetEncoding("utf-8")  );
						stream.WriteLine( sLogLine );
						stream.Close();
					}
					catch( Exception  E )
					{
						if( E.Message.IndexOf( "Code: 5" ) != 0 && nAttemptsCount < 5 )
							bSuccess = false;
						else
							bSuccess = true;
					}
				}
					

			}

		//---------------------------------------------------------------------------
		public void Add()
		{
			Add( "", "" );
		}

		//---------------------------------------------------------------------------
		public void Add( string sText)
		{
			Add( sText, "" );
		}
		

			//---------------------------------------------------------------------------
			public void Add(string sText, string sBtwUserAndDate )
		  {
			   m_sText = sText;
				 m_sBtwUserAndDate = sBtwUserAndDate;
				 InternalAdd();
      }

		//------------------------------------------------------------------------------------------
		public void  NoLogUser() { m_bLogUser = false; }

		//------------------------------------------------------------------------------------------
		public void NoLogDate() { m_bLogDate = false; }

		//------------------------------------------------------------------------------------------
		public void InitUsingPath( string sFileName, string sBaseDir, int nMaxLogSize )
		{
			InitUsingPath( sFileName, sBaseDir, nMaxLogSize, "" );
		}


		//------------------------------------------------------------------------------------------
		public void InitUsingPath( string sFileName, string sBaseDir, int nMaxLogSize, string sUserID )
		{
			m_sDir = sBaseDir + "PEMSO_LOGS\\";

			if( ! Directory.Exists( m_sDir ) ) 
				Directory.CreateDirectory( m_sDir );

			if( sUserID != "" )
			{
				m_sDir += sUserID + "\\";
				if( Directory.Exists(m_sDir ) )
					Directory.CreateDirectory( m_sDir );
			}

			Init( m_sDir + sFileName, nMaxLogSize );

		}

		//------------------------------------------------------------------------------------------
		public void Init( string sFileName, int nMaxLogSize )
		{
			m_bLogDate = true;
			m_bLogUser = true;
			m_nMaxLogSize = nMaxLogSize;
			m_sFileName = sFileName;

			FileInfo finf =  new FileInfo( sFileName );
			


			long nFileSize =  finf.Exists ? finf.Length : 0;

		

			if( nFileSize > m_nMaxLogSize )
			{

				byte [] Buffer = new byte[m_nMaxLogSize+1];
				m_File = new FileStream( sFileName, FileMode.Open, FileAccess.Read  );
				m_File.Seek( nFileSize - m_nMaxLogSize, SeekOrigin.Begin );
				int nRead = m_File.Read( Buffer, 0, (int)m_nMaxLogSize );
					m_File.Close();
				m_File = new FileStream( sFileName, FileMode.Open, FileAccess.Write );
				m_File.Write( Buffer, 0, nRead );
				m_File.Close();
			}

			



		}




	}
}
