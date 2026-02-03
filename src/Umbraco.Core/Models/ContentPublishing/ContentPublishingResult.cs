namespace Umbraco.Cms.Core.Models.ContentPublishing;

public sealed class ContentPublishingResult
{
    public IPublishableContentBase? Content { get; init; }

    public IEnumerable<string> InvalidPropertyAliases { get; set; } = [];
}
