using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiElementBuilder : IApiElementBuilder
{
    private readonly IPropertyMapper _propertyMapper;

    public ApiElementBuilder(IPropertyMapper propertyMapper) => _propertyMapper = propertyMapper;

    public IApiElement Build(IPublishedElement element) => new ApiElement(
        element.Key,
        element.ContentType.Alias,
        _propertyMapper.Map(element));
}
