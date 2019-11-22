namespace Umbraco.Core.Models.ContentEditing
{
    using System.Runtime.Serialization;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Represent the content app badge types
    /// </summary>
    [DataContract(Name = "contentAppBadgeType")]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContentAppBadgeType
    {
        [EnumMember(Value = "default")]
        Default = 0,

        [EnumMember(Value = "warning")]
        Warning = 1,

        [EnumMember(Value = "alert")]
        Alert = 2
    }
}
