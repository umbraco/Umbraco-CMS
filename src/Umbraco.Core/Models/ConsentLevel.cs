using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models
{
    [DataContract]
    public enum ConsentLevel
    {
        Minimal,
        Basic,
        Detailed,
    }
}
