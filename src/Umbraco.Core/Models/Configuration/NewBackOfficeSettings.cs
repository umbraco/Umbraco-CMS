using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Models.Configuration;

// FIXME: merge this class with relevant existing settings from Core and clean up
[UmbracoOptions($"{Umbraco.Cms.Core.Constants.Configuration.ConfigPrefix}NewBackOffice")]
public class NewBackOfficeSettings
{
    public Uri? BackOfficeHost { get; set; } = null;

    public string? AuthorizeCallbackPathName { get; set; } = null;
}
