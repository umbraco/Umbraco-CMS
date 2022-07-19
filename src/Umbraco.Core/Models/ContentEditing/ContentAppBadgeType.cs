using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

// TODO: This was marked with `[StringEnumConverter]` to inform the serializer
// to serialize the values to string instead of INT (which is the default)
// so we need to either invent our own attribute and make the implementation aware of it
// or ... something else?

/// <summary>
///     Represent the content app badge types
/// </summary>
[DataContract(Name = "contentAppBadgeType")]
public enum ContentAppBadgeType
{
    [EnumMember(Value = "default")]
    Default = 0,

    [EnumMember(Value = "warning")]
    Warning = 1,

    [EnumMember(Value = "alert")]
    Alert = 2,
}
