using System;

namespace Bak
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class CLog : Bak.CLogBase
	{
		public CLog()
			: base( "backuper_log.txt", 22000 )
		{
			m_bLogUser = false;
			Add();
			Add();
		}
	}
}
