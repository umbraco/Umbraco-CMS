using System;
using System.Web.UI;

namespace umbraco
{
	[Obsolete("This has been replaced with ScriptingMacroResult instead")]
	public class DLRMacroResult
	{
		public DLRMacroResult()
		{
		}

		public DLRMacroResult(Control control, Exception resultException)
		{
			Control = control;
			ResultException = resultException;
		}

		public Control Control { get; set; }
		public Exception ResultException { get; set; }
	}
}