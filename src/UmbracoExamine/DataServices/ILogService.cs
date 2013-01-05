using System;
namespace UmbracoExamine.DataServices
{
    public interface ILogService
    {
        string ProviderName { get; set; }
        void AddErrorLog(int nodeId, string msg);
        void AddInfoLog(int nodeId, string msg);
        void AddVerboseLog(int nodeId, string msg);
        LoggingLevel LogLevel { get; set; }
    }
}
