using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiMediaBuilder : IApiMediaBuilder
{
    private readonly IApiContentNameProvider _apiContentNameProvider;
    private readonly IApiMediaUrlProvider _apiMediaUrlProvider;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    public ApiMediaBuilder(
        IApiContentNameProvider apiContentNameProvider,
        IApiMediaUrlProvider apiMediaUrlProvider,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
    {
        _apiContentNameProvider = apiContentNameProvider;
        _apiMediaUrlProvider = apiMediaUrlProvider;
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
    }

    public IApiMedia Build(IPublishedContent media) =>
        new ApiMedia(
            media.Key,
            _apiContentNameProvider.GetName(media),
            media.ContentType.Alias,
            _apiMediaUrlProvider.GetUrl(media),
            Properties(media));

    // map all media properties except the umbracoFile one, as we've already included the file URL etc. in the output
    private IDictionary<string, object?> Properties(IPublishedContent media) =>
        _outputExpansionStrategyAccessor.TryGetValue(out IOutputExpansionStrategy? outputExpansionStrategy)
            ? outputExpansionStrategy.MapProperties(media.Properties.Where(p => p.Alias != Constants.Conventions.Media.File))
            : new Dictionary<string, object?>();
}
