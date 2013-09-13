using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IExternalLogger
    {
        string Assembly { get; }

        string ExternalLoggerType { get; }

        bool LogAuditTrail { get; }
    }
}