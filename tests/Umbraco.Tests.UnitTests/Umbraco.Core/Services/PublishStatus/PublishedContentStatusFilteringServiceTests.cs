using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.PublishStatus;

[TestFixture]
public partial class PublishedContentStatusFilteringServiceTests
{
    private IPublishStatusQueryService SetupPublishStatusQueryService(Dictionary<Guid, IPublishedContent> items)
        => SetupPublishStatusQueryService(items, id => id % 2 == 0);

    private IPublishStatusQueryService SetupPublishStatusQueryService(Dictionary<Guid, IPublishedContent> items, Func<int, bool> idIsPublished)
    {
        var publishStatusQueryService = new Mock<IPublishStatusQueryService>();
        publishStatusQueryService
            .Setup(p => p.IsDocumentPublished(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns((Guid key, string culture) => items
                                                       .TryGetValue(key, out var item)
                                                   && idIsPublished(item.Id)
                                                   && (item.ContentType.VariesByCulture() is false || item.Cultures.ContainsKey(culture)));
        return publishStatusQueryService.Object;
    }

    private IPreviewService SetupPreviewService(bool forPreview)
    {
        var previewService = new Mock<IPreviewService>();
        previewService.Setup(p => p.IsInPreview()).Returns(forPreview);
        return previewService.Object;
    }

    private IVariationContextAccessor SetupVariantContextAccessor(string? requestCulture)
    {
        var variationContextAccessor = new Mock<IVariationContextAccessor>();
        variationContextAccessor.SetupGet(v => v.VariationContext).Returns(new VariationContext(requestCulture));
        return variationContextAccessor.Object;
    }

    private IPublishedContentCache SetupPublishedContentCache(bool forPreview, Dictionary<Guid, IPublishedContent> items)
    {
        var publishedContentCache = new Mock<IPublishedContentCache>();
        publishedContentCache
            .Setup(c => c.GetById(forPreview, It.IsAny<Guid>()))
            .Returns((bool preview, Guid key) => items.TryGetValue(key, out var item) ? item : null);
        return publishedContentCache.Object;
    }
}
