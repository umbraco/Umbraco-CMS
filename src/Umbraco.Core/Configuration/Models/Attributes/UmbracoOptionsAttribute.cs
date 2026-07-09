namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Attribute used to mark a class as an Umbraco configuration options class.
/// </summary>
/// <remarks>
///     This attribute specifies the configuration key used to bind settings from configuration sources.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class UmbracoOptionsAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoOptionsAttribute" /> class.
    /// </summary>
    /// <param name="configurationKey">The configuration key used to bind the settings.</param>
    public UmbracoOptionsAttribute(string configurationKey) => ConfigurationKey = configurationKey;

    /// <summary>
    ///     Gets the configuration key used to bind the settings.
    /// </summary>
    public string ConfigurationKey { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether to bind non-public properties.
    /// </summary>
    public bool BindNonPublicProperties { get; set; }
}
