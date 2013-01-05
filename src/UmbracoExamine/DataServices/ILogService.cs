using System;
namespace UmbracoExamine.DataServices
{
    public interface ILogService
    {
        string ProviderName { get; set; }
        void AddErrorLog(int nodeId, string msg);
        void AddInfoLog(int nodeId, string msg);
        void AddVerboseLog(int nodeId, string msg);

		[Obsolete("This value is no longer used since we support the log levels that are available with LogHelper")]
		LoggingLevel LogLevel { get; set; }
    }
}
