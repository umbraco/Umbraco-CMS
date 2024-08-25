namespace Umbraco.Cms.Core.Models.ContentPublishing;

public sealed class ContentPublishingResult
{
    public IContent? Content { get; init; }

    public IEnumerable<string> InvalidPropertyAliases { get; set; } = [];
}
