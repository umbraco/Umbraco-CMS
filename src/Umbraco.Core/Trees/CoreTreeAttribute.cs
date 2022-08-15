namespace Umbraco.Cms.Core.Trees;

/// <summary>
///     Indicates that a tree is a core tree and should not be treated as a plugin tree.
/// </summary>
/// <remarks>
///     This ensures that umbraco will look in the umbraco folders for views for this tree.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class CoreTreeAttribute : Attribute
{
}
