using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using umbraco.BusinessLogic;

namespace UmbracoExamine.DataServices
{
    public class UmbracoLogService : UmbracoExamine.DataServices.ILogService
    {
        public string ProviderName { get; set; }

		[SecuritySafeCritical]
        public void AddInfoLog(int nodeId, string msg)
        {
            Log.Add(LogTypes.Custom, nodeId, "[UmbracoExamine] (" + ProviderName + ")" + msg);
        }

		[SecuritySafeCritical]
        public void AddErrorLog(int nodeId, string msg)
        {
            Log.Add(LogTypes.Error, nodeId, "[UmbracoExamine] (" + ProviderName + ")" + msg);
        }

		[SecuritySafeCritical]
        public void AddVerboseLog(int nodeId, string msg)
        {
            if (LogLevel == LoggingLevel.Verbose)
                Log.Add(LogTypes.Custom, nodeId, "[UmbracoExamine] (" + ProviderName + ")" + msg);
        }

        public LoggingLevel LogLevel { get; set; }

    }
}
