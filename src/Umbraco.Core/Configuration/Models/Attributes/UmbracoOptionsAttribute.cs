namespace Umbraco.Cms.Core.Configuration.Models;

[AttributeUsage(AttributeTargets.Class)]
public class UmbracoOptionsAttribute : Attribute
{
    public UmbracoOptionsAttribute(string configurationKey) => ConfigurationKey = configurationKey;

    public string ConfigurationKey { get; }

    public bool BindNonPublicProperties { get; set; }
}
