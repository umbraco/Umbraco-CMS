using System;

using System.Runtime.InteropServices;


// GPL Licensed work
// (c) Christian Palm + umbraco crew 2005
// Kudos to Christian Palm for finally making a 
// Tidy solution that just works
// palmdk@gmail.com

namespace CPalmTidy
{
	/// <summary>
	/// Summary description for tidy.
	/// </summary>
	public class Tidy
	{
		public string error;
		public string outputFile;
		public string inputFile;
		public string configFile;
		
		public Tidy()
		{
			this.error = "";
			this.outputFile = "";
			this.inputFile = "";
			this.configFile = "";
		}
		
		public bool Run()
		{
			int i = CallTidyMain(this.inputFile, this.outputFile, this.configFile);
			this.error = i.ToString();
			//this.error = "this.inputFile: " + this.inputFile + "<br>this.outputFile: " + this.outputFile + "<br>this.configFile: " + this.configFile;
			return true;
		}
		
		[DllImport(@"TidyDll.dll")]
		static extern int CallTidyMain(
			[MarshalAs(UnmanagedType.LPStr)] string inFileName,
			[MarshalAs(UnmanagedType.LPStr)] string outFileName,
			[MarshalAs(UnmanagedType.LPStr)] string optionsFileName
			);

		
		public string ErrorValue
		{
			get
			{
				return this.error;
			}
		}

		public string ConfigFile
		{
			get
			{
				return this.configFile;
			}
			set
			{
				this.configFile = value;
			}
		}

		public string OutputFile
		{
			get
			{
				return this.outputFile;
			}
			set
			{
				this.outputFile = value;
			}
		}

		public string InputFile
		{
			get
			{
				return this.inputFile;
			}
			set
			{
				this.inputFile = value;
			}
		}

		
	}
}
