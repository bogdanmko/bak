using System;
using System.Runtime.InteropServices; 

//////////////////////////////////////////////////////
//WinAPI interface routines and definitions
//////////////////////////////////////////////////////
namespace Bak
{
	/// <summary>
	/// Summary description for WinAPI.
	/// </summary>
	public class WinAPI
	{
		public WinAPI()
		{
			//The class is used as static
		}

		//****************************************************************************************
		public const uint INVALID_HANDLE_VALUE = 0xFFFFFFFF;

		//****************************************************************************************
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			public struct STARTUPINFO
		{
			public uint cb;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string lpReserved;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string lpDesktop;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string lpTitle;
			public uint dwX;
			public uint dwY;
			public uint dwXSize;
			public uint dwYSize;
			public uint dwXCountChars;
			public uint dwYCountChars;
			public uint dwFillAttribute;
			public uint dwFlags;
			public ushort wShowWindow;
			public ushort cbReserved2;
			public IntPtr lpReserved2;
			public IntPtr hStdInput;
			public IntPtr hStdOutput;
			public IntPtr hStdError;
		}

		//****************************************************************************************
		[StructLayout(LayoutKind.Sequential)]
			public struct PROCESS_INFORMATION
		{
			public IntPtr hProcess;
			public IntPtr hThread;
			public uint dwProcessId;
			public uint dwThreadId;
		}

		//****************************************************************************************
		[StructLayout(LayoutKind.Sequential)]
			public struct SECURITY_ATTRIBUTES
		{
			public int nLength;
			public IntPtr lpSecurityDescriptor;
			public int bInheritHandle;
		}

		//****************************************************************************************
		[Flags]
		public enum PRIORITY_CLASS : uint
		{
			NORMAL_PRIORITY_CLASS = 		0x00000020
		}
 
		//****************************************************************************************
		[Flags]
			public enum STARTF : uint
		{
			STARTF_USESHOWWINDOW = 0x00000001,
			STARTF_USESIZE = 0x00000002,
			STARTF_USEPOSITION = 0x00000004,
			STARTF_USECOUNTCHARS = 0x00000008,
			STARTF_USEFILLATTRIBUTE = 0x00000010,
			STARTF_RUNFULLSCREEN = 0x00000020,  // ignored for non-x86 platforms
			STARTF_FORCEONFEEDBACK = 0x00000040,
			STARTF_FORCEOFFFEEDBACK = 0x00000080,
			STARTF_USESTDHANDLES = 0x00000100
		}

		//****************************************************************************************
		public enum SHOWWINDOW : uint
		{
			SW_HIDE = 0,
			SW_SHOWNORMAL = 1,
			SW_NORMAL = 1,
			SW_SHOWMINIMIZED = 2,
			SW_SHOWMAXIMIZED = 3,
			SW_MAXIMIZE = 3,
			SW_SHOWNOACTIVATE = 4,
			SW_SHOW = 5,
			SW_MINIMIZE = 6,
			SW_SHOWMINNOACTIVE = 7,
			SW_SHOWNA = 8,
			SW_RESTORE = 9,
			SW_SHOWDEFAULT = 10,
			SW_FORCEMINIMIZE = 11,
			SW_MAX = 11
		}

		//****************************************************************************************
		[Flags]
		public enum FILE_ATTR : uint
		{
		  FILE_ATTRIBUTE_ARCHIVE = 0x20,
		  FILE_ATTRIBUTE_HIDDEN = 0x2,
		  FILE_ATTRIBUTE_NORMAL = 0x80
	  }

	
		//****************************************************************************************
	[Flags]
		public enum CREATE_FILE : uint
		{
		  CREATE_NEW = 1,
		  CREATE_ALWAYS = 2,
		  OPEN_ALWAYS = 4,
		  OPEN_EXISTING = 3,
		  TRUNCATE_EXISTING =5

	  }

		//****************************************************************************************
		[Flags]
			public enum FILE_POINTER : uint
		{
			FILE_BEGIN = 0,
			FILE_CURRENT = 1,
			FILE_END = 2
		}


		//****************************************************************************************
	[Flags]
		public enum FILE_SHARE : uint
		{
      FILE_SHARE_READ  =  0x00000001,
      FILE_SHARE_WRITE =  0x00000002,
      FILE_SHARE_DELETE = 0x00000004  
	  }


		//****************************************************************************************
	[Flags]
		public enum GENERIC_ACCESS : uint
		{
      GENERIC_READ =   0x80000000,
      GENERIC_WRITE =  0x40000000,
      GENERIC_EXECUTE = 0x20000000,
      GENERIC_ALL = 0x10000000
	  }

		//****************************************************************************************
		public enum STD_HANDLE : int
		{
			STD_INPUT_HANDLE = -10,
		  STD_OUTPUT_HANDLE = -11,
		  STD_ERROR_HANDLE = -12
		}

		//****************************************************************************************
		public enum WAIT_OBJECT : uint
		{
			WAIT_TIMEOUT = 0x00000102,
			WAIT_ABANDONED = 0x00000080,
			WAIT_OBJECT_0 = 0x00000000,
			WAIT_FAILED = 0xFFFFFFFF
		}

		//****************************************************************************************
		public enum FORMAT_MSG : uint
		{
			FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100,
			FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000,
			FORMAT_MESSAGE_FROM_HMODULE = 0x00000800,
			FORMAT_MESSAGE_FROM_STRING = 0x00000400,
			FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000,
			FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200,
			FORMAT_MESSAGE_MAX_WIDTH_MASK=	0x000000FF
		}



		//---------------------------------------------------------------------------
		public delegate int WndEnumProc(IntPtr hwnd, int lParam);


		//---------------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
		public static  extern bool CloseHandle(IntPtr hHandle);

		//---------------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetStdHandle(int nStdHandle);

		//---------------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint GetLastError();

		//---------------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int GetDiskFreeSpaceEx( 
			string lpDirectoryName,
			out long lpFreeBytesAvailable,
			out long lpTotalNumberOfBytes,
			out long lpTotalNumberOfFreeBytes );


		//---------------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
			public static extern uint SetFilePointerEx( IntPtr hFile, long liDistanceToMove, out long lpNewFilePointer, uint dwMoveMethod ); 



		//---------------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint WaitForSingleObject( IntPtr hHandle, uint dwMilliseconds );

		//---------------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int TerminateProcess( IntPtr hHandle, uint uExitCode );

		//---------------------------------------------------------------------------
    [DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint FormatMessage(
						uint dwFlags,
			      string lpSource,
			      uint  dwMessageId,
		        uint dwLanguageId,
		        out string lpBuffer,
		        uint nSize,
			      IntPtr Arguments
					 );
		//---------------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr CreateFile(
			string lpFileName,
			uint dwDesiredAccess,
			uint dwShareMode,
			ref  SECURITY_ATTRIBUTES lpSecurityAttributes,
		  uint dwCreationDisposition,
		  uint dwFlagsAndAttributes,
		  IntPtr hTemplateFile
	 );

		//---------------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int WriteFile(
			IntPtr hFile,
			byte[]  lpBuffer,
			int nNumberOfBytesToWrite,
		  out int lpNumberOfBytesWritten,
		  IntPtr lpOverlapped
		);	

		//---------------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int ReadFile(
			IntPtr hFile,
			byte[]  lpBuffer,
			int nNumberOfBytesToRead,
			out int lpNumberOfBytesRead,
			IntPtr lpOverlapped
			);	
 
 
		//---------------------------------------------------------------------------
		[DllImport("kernel32.dll", EntryPoint = "CreateProcessW", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int CreateProcess(
			string lpApplicationName,
			string lpCommandLine,
			IntPtr lpProcessAttributes,
			IntPtr lpThreadAttributes,
			int bInheritHandles,
			uint dwCreationFlags,
			IntPtr lpEnvironment,
			string lpCurrentDirectory,
			ref STARTUPINFO lpStartupInfo,
			out PROCESS_INFORMATION lpProcessInformation);

		//---------------------------------------------------------------------------
		public static void InitPROCESS_INFORMATION(ref PROCESS_INFORMATION pi )
		{
			pi.dwProcessId = 0;
			pi.dwThreadId = 0;
			pi.hProcess = IntPtr.Zero;
			pi.hThread = IntPtr.Zero;
		}

		//---------------------------------------------------------------------------
		public static void InitSECURITY_ATTRIBUTES(ref SECURITY_ATTRIBUTES si )
		{
			si.nLength = (int) Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES));
			si.lpSecurityDescriptor = IntPtr.Zero;
			si.bInheritHandle = 0;


		}

		//---------------------------------------------------------------------------
		public static void InitSTARTUPINFO(ref STARTUPINFO si )
		{
			si.lpTitle = null;
			si.cb = (uint) Marshal.SizeOf(typeof(STARTUPINFO));
			si.lpReserved = null;
			si.lpDesktop = null;
			si.lpTitle = null;
			si.dwX = 0;
			si.dwY = 0;
			si.dwXSize = 0;
			si.dwYSize = 0;
			si.dwXCountChars = 0;
			si.dwYCountChars = 0;
			si.dwFillAttribute = 0;
			si.dwFlags = 0;
			si.wShowWindow = 0;
			si.cbReserved2 = 0;
			si.lpReserved2 = IntPtr.Zero;
			si.hStdInput = IntPtr.Zero;
			si.hStdOutput = IntPtr.Zero;
			si.hStdError = IntPtr.Zero;
		}


		//---------------------------------------------------------------------------
		public static string GetLastWindowsError()
		{
			string sMessage;
			uint nErrorCode = GetLastError();

			FormatMessage(
				(uint) FORMAT_MSG.FORMAT_MESSAGE_ALLOCATE_BUFFER | (uint) FORMAT_MSG.FORMAT_MESSAGE_FROM_SYSTEM,
				null,
				nErrorCode,
				0, // Default language
				out sMessage,
				0,
				IntPtr.Zero
				);

			return  nErrorCode.ToString() + " " + sMessage;
		}

	}

}
////////////////////////////////////////////////////////////
