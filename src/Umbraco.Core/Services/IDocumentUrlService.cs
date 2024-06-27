using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
namespace Umbraco.Cms.Core.Services;

public interface IDocumentUrlService
{
    /// <summary>
    /// Rebuilds all urls for all documents in all combinations.
    /// </summary>
    Task RebuildAllUrlsAsync();

    /// <summary>
    /// Gets a bool indicating whether the current urls are valid with the current configuration or not.
    /// </summary>
    Task<bool> ShouldRebuildUrlsAsync();

    /// <summary>
    /// Gets the Url from a document key, culture and segment. Preview urls are returned if isPreview is true.
    /// </summary>
    /// <param name="documentKey">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="isDraft">Whether to get the url of the draft or published document.</param>
    /// <returns>The url of the document.</returns>
    Task<string> GetUrlAsync(Guid documentKey, string culture, string segment, bool isDraft);

    /// <summary>
    /// Creates or updates a url for a document key, culture and segment and draft information.
    /// </summary>
    /// <param name="documentKey">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="isDraft">Whether to set the url of the draft or published document.</param>
    /// <param name="url">The new url.</param>
    Task CreateOrUpdateUrlAsync(Guid documentKey, string culture, string segment, bool isDraft, string url);

    /// <summary>
    /// Delete a specific url for a document key, culture and segment and draft information.
    /// </summary>
    /// <param name="documentKey">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="isDraft">Whether to delete the url of the draft or published document.</param>
    Task DeleteUrlAsync(Guid documentKey, string culture, string segment, bool isDraft);

    /// <summary>
    /// Delete all url for a document key, culture and segment and draft information.
    /// </summary>
    /// <param name="documentKey">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="isDraft">Whether to delete the url of the draft or published document.</param>
    Task DeleteUrlAsync(Guid documentKey);
}

public class DocumentUrlService : IDocumentUrlService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public DocumentUrlService(
        IDocumentRepository documentRepository,
        ICoreScopeProvider coreScopeProvider)
    {
        _documentRepository = documentRepository;
        _coreScopeProvider = coreScopeProvider;
    }

    public Task RebuildAllUrlsAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);

        IEnumerable<IContent> contents = _documentRepository.GetMany(Array.Empty<Guid>());

        foreach (IContent content in contents)
        {

        }

        return Task.CompletedTask;
    }

    public Task<bool> ShouldRebuildUrlsAsync() => throw new NotImplementedException();

    public Task<string> GetUrlAsync(Guid documentKey, string culture, string segment, bool isDraft) => throw new NotImplementedException();

    public Task CreateOrUpdateUrlAsync(Guid documentKey, string culture, string segment, bool isDraft, string url) => throw new NotImplementedException();

    public Task DeleteUrlAsync(Guid documentKey, string culture, string segment, bool isDraft) => throw new NotImplementedException();

    public Task DeleteUrlAsync(Guid documentKey) => throw new NotImplementedException();
}
