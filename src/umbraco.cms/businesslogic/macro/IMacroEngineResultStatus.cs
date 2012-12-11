using System;

namespace umbraco.cms.businesslogic.macro
{
	public interface IMacroEngineResultStatus
	{
		bool Success { get; }
		Exception ResultException { get; }
	}
}