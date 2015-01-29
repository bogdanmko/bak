using System;

namespace Bak
{
	/// <summary>
	/// Summary description for Utils.
	/// </summary>
	public class Utils
	{
		public Utils()
		{
		}

		//-----------------------------------------------------------------------------------------------
		public static bool SwitchIsPresentInCMD( string sSwitch )
		{
			bool bResult = false;
			sSwitch = sSwitch.ToLower();
			string [] arrCMDArgs;
			arrCMDArgs 	= Environment.GetCommandLineArgs();

			for( int nParamNo=1; nParamNo < arrCMDArgs.Length && !bResult; nParamNo++ )
			{
				string sParam = arrCMDArgs[nParamNo].ToLower();
				bResult = sSwitch == sParam;
			}
    
			return bResult;

		}

		//-----------------------------------------------------------------------------------------------
		public static string GetTimeStamp()
		{
			DateTime timeNow = DateTime.Now;
			return timeNow.ToShortDateString() + " " + timeNow.ToShortTimeString();
		}


		//-----------------------------------------------------------------------------------------------
		private static int GetDeviceNameEndPos( string sPath )
		{
			int nStopPos = sPath.IndexOf(":");
			if( nStopPos < 0 )
				nStopPos = sPath.IndexOf("\\");

			return nStopPos;

		}

		//-----------------------------------------------------------------------------------------------
		//Returns device name (drive letter)
		public static string GetDeviceName( string sPath )
		{
			string sResult = "";
			sPath = sPath.Trim();
			int nStopPos = GetDeviceNameEndPos( sPath );
			if( nStopPos > 0 )
				sResult = sPath.Substring(0, nStopPos );

			return sResult;
		}


		//-----------------------------------------------------------------------------------------------
		//Replaces drive letter in sPath for sNewDeviceName
		public static string ReplaceDeviceName( string sPath, string sNewDeviceName )
		{
			string sResult = sPath.Trim();
			if( sNewDeviceName.Trim() == "" )
				return sResult;

			int nStopPos = GetDeviceNameEndPos( sResult );

			if( nStopPos < 0 )
				return sResult;

			sResult = sResult.Remove(0, nStopPos );

			return sResult.Insert( 0, sNewDeviceName );
		}
	}
}
