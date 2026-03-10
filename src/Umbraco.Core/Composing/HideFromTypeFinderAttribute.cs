namespace Umbraco.Cms.Core.Composing;

/// <summary>
/// Notifies the <see cref="ITypeFinder" /> that it should ignore the class marked with this attribute.
/// </summary>
/// <remarks>
/// Apply this attribute to classes that should not be discovered during type scanning operations.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class HideFromTypeFinderAttribute : Attribute
{
}
