namespace Umbraco.Cms.Core.CodeAnnotations;

/// <summary>
///     Attribute to add a Friendly Name string with an UmbracoObjectType enum value
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public class FriendlyNameAttribute : Attribute
{
    /// <summary>
    ///     friendly name value
    /// </summary>
    private readonly string _friendlyName;

    /// <summary>
    ///     Initializes a new instance of the FriendlyNameAttribute class
    ///     Sets the friendly name value
    /// </summary>
    /// <param name="friendlyName">attribute value</param>
    public FriendlyNameAttribute(string friendlyName) => _friendlyName = friendlyName;

    /// <summary>
    ///     Gets the friendly name
    /// </summary>
    /// <returns>string of friendly name</returns>
    public override string ToString() => _friendlyName;
}
