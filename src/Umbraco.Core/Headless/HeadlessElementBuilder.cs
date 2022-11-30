using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Headless;

public class HeadlessElementBuilder : IHeadlessElementBuilder
{
    private readonly IHeadlessPropertyMapper _propertyMapper;

    public HeadlessElementBuilder(IHeadlessPropertyMapper propertyMapper) => _propertyMapper = propertyMapper;

    public IHeadlessElement Build(IPublishedElement element) => new HeadlessElement(
        element.Key,
        element.ContentType.Alias,
        _propertyMapper.Map(element));
}
