using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiContentNameProvider : IApiContentNameProvider
{
    public string GetName(IPublishedContent content) => content.Name;
}
