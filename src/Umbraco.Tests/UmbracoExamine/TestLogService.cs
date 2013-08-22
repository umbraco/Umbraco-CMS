using System.Diagnostics;
using UmbracoExamine;
using UmbracoExamine.DataServices;

namespace Umbraco.Tests.UmbracoExamine
{
	public class TestLogService : ILogService
	{
		#region ILogService Members

		public string ProviderName { get; set; }

		public void AddErrorLog(int nodeId, string msg)
		{
			Trace.WriteLine("ERROR: (" + nodeId.ToString() + ") " + msg);
		}

		public void AddInfoLog(int nodeId, string msg)
		{
			Trace.WriteLine("INFO: (" + nodeId.ToString() + ") " + msg);
		}

		public void AddVerboseLog(int nodeId, string msg)
		{
			if (LogLevel == LoggingLevel.Verbose)
				Trace.WriteLine("VERBOSE: (" + nodeId.ToString() + ") " + msg);
		}

		public LoggingLevel LogLevel
		{
			get
			{
				return LoggingLevel.Verbose;
			}
			set
			{
				//do nothing
			}
		}

		#endregion
	}
}