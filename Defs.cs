using System;

namespace Bak
{
	/// <summary>
	/// Summary description for Defs.
	/// </summary>
	public class Defs
	{
		public Defs()
		{
            
		}

        public static void Init()
        {
            APP_VERSION = APP_TITLE + " version from " +
                Utils.GetCompileTime.Month.ToString() + "/" +
                Utils.GetCompileTime.Day.ToString() + "/" +
                Utils.GetCompileTime.Year.ToString();

        }

		public const string APP_TITLE = "Bak.NET";
		public const string ERROR_MARK = "***ERROR:";
		public const string WARNING = "***WARNING:";
        public static string APP_VERSION;

	}
}
