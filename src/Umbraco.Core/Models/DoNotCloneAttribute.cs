namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Used to attribute properties that have a setter and are a reference type
///     that should be ignored for cloning when using the DeepCloneHelper
/// </summary>
/// <remarks>
///     This attribute must be used:
///     * when the property is backed by a field but the result of the property is the un-natural data stored in the field
///     This attribute should not be used:
///     * when the property is virtual
///     * when the setter performs additional required logic other than just setting the underlying field
/// </remarks>
public class DoNotCloneAttribute : Attribute
{
}
