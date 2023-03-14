namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Notifies the TypeFinder that it should ignore the class marked with this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class HideFromTypeFinderAttribute : Attribute
{
}
