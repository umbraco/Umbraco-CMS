using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Umbraco.Core.Models.Header
{
    /// <summary>
    /// Represent the header app badge types
    /// </summary>
    [DataContract(Name = "headerAppBadgeType")]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HeaderAppBadgeType
    {
        [EnumMember(Value = "default")]
        Default = 0,

        [EnumMember(Value = "warning")]
        Warning = 1,

        [EnumMember(Value = "alert")]
        Alert = 2
    }
}
