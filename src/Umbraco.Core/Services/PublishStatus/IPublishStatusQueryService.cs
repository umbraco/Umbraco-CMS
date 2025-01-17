namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///
/// </summary>
public interface IPublishStatusQueryService
{
    bool IsDocumentPublished(Guid documentKey, string culture);
}
