namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the type of a property group.
/// </summary>
public enum PropertyGroupType : short
{
    /// <summary>
    ///     Display property types in a group.
    /// </summary>
    Group = 0,

    /// <summary>
    ///     Display property types in a tab.
    /// </summary>
    Tab = 1,
}
