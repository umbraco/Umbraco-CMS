using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

[DataContract]
public enum TelemetryLevel
{
    Minimal,
    Basic,
    Detailed,
}
