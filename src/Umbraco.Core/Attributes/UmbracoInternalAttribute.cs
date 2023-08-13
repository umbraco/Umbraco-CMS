namespace Umbraco.Cms.Core.Attributes;

/// <summary>
///     Marks an API as internal to Umbraco. These APIs are not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use such APIs directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Umbraco release.
/// </summary>
/// <remarks>
/// This approch was inspired by: https://github.com/dotnet/efcore/blob/main/src/EFCore/Infrastructure/EntityFrameworkInternalAttribute.cs
/// </remarks>
[AttributeUsage(
    AttributeTargets.Enum
    | AttributeTargets.Class
    | AttributeTargets.Struct
    | AttributeTargets.Interface
    | AttributeTargets.Event
    | AttributeTargets.Field
    | AttributeTargets.Method
    | AttributeTargets.Delegate
    | AttributeTargets.Property
    | AttributeTargets.Constructor)]
public sealed class UmbracoInternalAttribute : Attribute
{
}
