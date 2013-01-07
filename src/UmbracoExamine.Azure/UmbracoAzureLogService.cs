using Examine;
using Examine.Azure;
using UmbracoExamine.DataServices;

namespace UmbracoExamine.Azure
{
    public class UmbracoAzureLogService : ILogService
    {
        public string ProviderName { get; set; }

        public void AddErrorLog(int nodeId, string msg)
        {
            AzureExtensions.LogExceptionFile(ProviderName, new IndexingErrorEventArgs(msg, nodeId, null));
        }

        public void AddInfoLog(int nodeId, string msg)
        {
            if (LogLevel == LoggingLevel.Verbose)
                AzureExtensions.LogMessageFile("[UmbracoExamine] (" + ProviderName + ")" + msg + ". " + nodeId);
        }

        public void AddVerboseLog(int nodeId, string msg)
        {
            if (LogLevel == LoggingLevel.Verbose)
                AzureExtensions.LogMessageFile("[UmbracoExamine] (" + ProviderName + ")" + msg + ". " + nodeId);
        }

        public LoggingLevel LogLevel { get; set; }
    }
}