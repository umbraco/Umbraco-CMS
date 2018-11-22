using System;

namespace umbraco
{
	public class ScriptingMacroResult
	{
		public ScriptingMacroResult()
		{
		}

		public ScriptingMacroResult(string result, Exception resultException)
		{
			Result = result;
			ResultException = resultException;
		}

		public string Result { get; set; }
		public Exception ResultException { get; set; }
	}
}