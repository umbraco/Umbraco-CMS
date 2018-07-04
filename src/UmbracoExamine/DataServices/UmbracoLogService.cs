using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;

namespace UmbracoExamine.DataServices
{
    public class UmbracoLogService : ILogService
    {
        public string ProviderName { get; set; }

		
        public void AddInfoLog(int nodeId, string msg)
		{
			LogHelper.Info<UmbracoLogService>("{0}, Provider={1}, NodeId={2}", () => msg, () => ProviderName, () => nodeId);
        }

		
        public void AddErrorLog(int nodeId, string msg)
		{
			//NOTE: not really the prettiest but since AddErrorLog is legacy code, we cannot change it now to accept a real Exception obj for
			// use with the new LogHelper
			LogHelper.Error<UmbracoLogService>(
				string.Format("Provider={0}, NodeId={1}", ProviderName, nodeId),
				new Exception(msg));
        }

		
        public void AddVerboseLog(int nodeId, string msg)
        {
			LogHelper.Debug<UmbracoLogService>("{0}, Provider={1}, NodeId={2}", () => msg, () => ProviderName, () => nodeId);
        }

		[Obsolete("This value is no longer used since we support the log levels that are available with LogHelper")]
        public LoggingLevel LogLevel { get; set; }

    }
}
