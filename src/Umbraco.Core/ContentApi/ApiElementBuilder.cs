using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiElementBuilder : IApiElementBuilder
{
    private readonly IPropertyMapper _propertyMapper;

    public ApiElementBuilder(IPropertyMapper propertyMapper) => _propertyMapper = propertyMapper;

    public IApiElement Build(IPublishedElement element, bool expand = true) => new ApiElement(
        element.Key,
        element.ContentType.Alias,
        expand ? _propertyMapper.Map(element) : new Dictionary<string, object?>());
}
