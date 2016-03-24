using System;

namespace Umbraco.Core.Macros
{
	internal class PartialViewMacroResult
	{
		public PartialViewMacroResult()
		{
		}

		public PartialViewMacroResult(string result, Exception resultException)
		{
			Result = result;
			ResultException = resultException;
		}

		public string Result { get; set; }
		public Exception ResultException { get; set; }
	}
}