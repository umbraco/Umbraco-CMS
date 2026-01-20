using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Implements <see cref="IDocumentUrlAliasService"/> operations for handling document URL aliases.
/// </summary>
public class DocumentUrlAliasService : IDocumentUrlAliasService
{
    private const string RebuildKey = "UmbracoUrlAliasGeneration";

    /// <summary>
    /// Represents the current rebuild version value used to track changes in alias parsing logic.
    /// </summary>
    /// <remarks>
    /// With the analagous <see cref="DocumentUrlService"/>, implementors can customize how segments are created.
    /// As such we track what URL providers are registered and store that as a value against the rebuild key. If on startup
    /// we find it's changed, we rebuild.
    /// Here, however, the alias parsing logic is internal and not customizable, so we simply use a constant value.
    /// By doing this we can keep the same logic for rebuild on startup after a migration, provide a means of triggering
    /// a rebuild, and we have future-proofing in case the alias parsing logic changes in future versions.
    /// </remarks>
    private const string CurrentRebuildValue = "1";

    private readonly ILogger<DocumentUrlAliasService> _logger;
    private readonly IDocumentUrlAliasRepository _documentUrlAliasRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly ILanguageService _languageService;
    private readonly IKeyValueService _keyValueService;
    private readonly IContentService _contentService;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;

    // Lookup: alias -> list of matching document keys (multiple docs can have same alias).
    private readonly ConcurrentDictionary<AliasCacheKey, List<Guid>> _aliasCache = new();

    // Reverse lookup: document -> aliases (for cache invalidation).
    private readonly ConcurrentDictionary<Guid, HashSet<AliasCacheKey>> _documentToAliasesCache = new();

    // Culture to language ID map (for memory efficiency). Case-insensitive for culture codes like "en-US" vs "en-us".
    private readonly ConcurrentDictionary<string, int> _cultureToLanguageIdMap = new(StringComparer.OrdinalIgnoreCase);

    private bool _isInitialized;

    /// <summary>
    /// Struct-based cache key for memory-efficient alias caching.
    /// </summary>
    internal readonly struct AliasCacheKey : IEquatable<AliasCacheKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AliasCacheKey"/> struct.
        /// </summary>
        /// <param name="normalizedAlias">The alias string that has been normalized for consistent comparison.</param>
        /// <param name="languageId">The identifier of the language associated with the alias.</param>
        public AliasCacheKey(string normalizedAlias, int languageId)
        {
            NormalizedAlias = normalizedAlias;
            LanguageId = languageId;
        }

        /// <summary>
        /// Gets the normalized alias (lowercase, no leading/trailing slashes).
        /// </summary>
        public string NormalizedAlias { get; }

        /// <summary>
        /// Gets the unique identifier for the language associated with this instance.
        /// </summary>
        public int LanguageId { get; }

        /// <inheritdoc/>
        public bool Equals(AliasCacheKey other) =>
            NormalizedAlias == other.NormalizedAlias &&
            LanguageId == other.LanguageId;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is AliasCacheKey other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(NormalizedAlias, LanguageId);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentUrlAliasService"/> class.
    /// </summary>
    public DocumentUrlAliasService(
        ILogger<DocumentUrlAliasService> logger,
        IDocumentUrlAliasRepository documentUrlAliasRepository,
        ICoreScopeProvider coreScopeProvider,
        ILanguageService languageService,
        IKeyValueService keyValueService,
        IContentService contentService,
        IDocumentNavigationQueryService documentNavigationQueryService)
    {
        _logger = logger;
        _documentUrlAliasRepository = documentUrlAliasRepository;
        _coreScopeProvider = coreScopeProvider;
        _languageService = languageService;
        _keyValueService = keyValueService;
        _contentService = contentService;
        _documentNavigationQueryService = documentNavigationQueryService;
    }

    /// <inheritdoc/>
    public async Task InitAsync(bool forceEmpty, CancellationToken cancellationToken)
    {
        if (forceEmpty)
        {
            _isInitialized = true;
            return;
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        if (ShouldRebuildAliases())
        {
            _logger.LogInformation("Rebuilding all document aliases.");
            await RebuildAllAliasesAsync();
        }

        _logger.LogInformation("Caching document aliases.");

        IEnumerable<PublishedDocumentUrlAlias> aliases = _documentUrlAliasRepository.GetAll();
        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();

        PopulateCultureToLanguageIdMap(languages);

        var aliasCount = 0;
        var cancelled = false;
        foreach (PublishedDocumentUrlAlias alias in aliases)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancelled = true;
                break;
            }

            AddToCache(alias);
            aliasCount++;
        }

        if (cancelled)
        {
            _logger.LogWarning("Initialization was cancelled after caching {AliasCount} document aliases.", aliasCount);
        }
        else
        {
            _logger.LogInformation("Cached {AliasCount} document aliases.", aliasCount);
            _isInitialized = true;
        }

        scope.Complete();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Guid>> GetDocumentKeysByAliasAsync(string alias, string? culture)
    {
        ThrowIfNotInitialized();

        var normalizedAlias = this.NormalizeAlias(alias);
        if (string.IsNullOrEmpty(normalizedAlias))
        {
            return [];
        }

        // Default to the default language when culture is not specified (like DocumentUrlService)
        culture ??= await _languageService.GetDefaultIsoCodeAsync();

        if (TryGetLanguageId(culture, out var languageId) is false)
        {
            return [];
        }

        var cacheKey = new AliasCacheKey(normalizedAlias, languageId);

        if (_aliasCache.TryGetValue(cacheKey, out List<Guid>? documentKeys) is false || documentKeys.Count == 0)
        {
            return [];
        }

        // Return all matching document keys.
        return documentKeys;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetAliasesAsync(Guid documentKey, string? culture)
    {
        ThrowIfNotInitialized();

        // Default to the default language when culture is not specified (like DocumentUrlService)
        culture ??= await _languageService.GetDefaultIsoCodeAsync();

        if (TryGetLanguageId(culture, out var languageId) is false)
        {
            return [];
        }

        if (_documentToAliasesCache.TryGetValue(documentKey, out HashSet<AliasCacheKey>? aliasKeys) is false || aliasKeys.Count == 0)
        {
            return [];
        }

        // Filter by language and return the alias strings.
        return aliasKeys
            .Where(key => key.LanguageId == languageId)
            .Select(key => key.NormalizedAlias)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateAliasesAsync(Guid documentKey)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.DocumentUrlAliases);

        await CreateOrUpdateAliasesInternalAsync(documentKey);

        scope.Complete();
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateAliasesWithDescendantsAsync(Guid documentKey)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.DocumentUrlAliases);

        // Get document and all descendants
        var documentKeys = new List<Guid> { documentKey };
        if (_documentNavigationQueryService.TryGetDescendantsKeys(documentKey, out IEnumerable<Guid> descendantKeys))
        {
            documentKeys.AddRange(descendantKeys);
        }

        foreach (Guid key in documentKeys)
        {
            await CreateOrUpdateAliasesInternalAsync(key);
        }

        scope.Complete();
    }

    /// <summary>
    /// Internal implementation that processes a single document without creating its own scope.
    /// Caller must ensure a scope with write lock is active.
    /// </summary>
    private async Task CreateOrUpdateAliasesInternalAsync(Guid documentKey)
    {
        IContent? document = _contentService.GetById(documentKey);
        if (document is null || document.Trashed || document.Blueprint)
        {
            // Remove from cache if document doesn't exist, is trashed, or is a blueprint.
            RemoveFromCacheDeferred(_coreScopeProvider.Context!, documentKey);
            return;
        }

        List<PublishedDocumentUrlAlias> aliases = await ExtractAliasesFromDocumentAsync(document);

        // Remove old aliases from cache (deferred until scope completes)
        RemoveFromCacheDeferred(_coreScopeProvider.Context!, documentKey);

        // Save to database (handles insert/update/delete via diff) and add to cache
        if (aliases.Count > 0)
        {
            _documentUrlAliasRepository.Save(aliases);

            foreach (PublishedDocumentUrlAlias alias in aliases)
            {
                AddToCacheDeferred(_coreScopeProvider.Context!, alias);
            }
        }
        else
        {
            // No aliases - delete any existing aliases for this document from the database
            _documentUrlAliasRepository.DeleteByDocumentKey(new[] { documentKey });
        }
    }

    /// <inheritdoc/>
    public Task DeleteAliasesFromCacheAsync(IEnumerable<Guid> documentKeys)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        foreach (Guid documentKey in documentKeys)
        {
            RemoveFromCacheDeferred(_coreScopeProvider.Context!, documentKey);
        }

        scope.Complete();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public bool HasAny()
    {
        ThrowIfNotInitialized();
        return _aliasCache.Count > 0;
    }

    /// <inheritdoc/>
    public async Task RebuildAllAliasesAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);

        // Get all languages for culture-to-languageId mapping
        var languages = (await _languageService.GetAllAsync()).ToList();
        PopulateCultureToLanguageIdMap(languages);

        // Use optimized SQL query to fetch only documents with aliases
        IEnumerable<DocumentUrlAliasRaw> rawAliases = _documentUrlAliasRepository.GetAllDocumentUrlAliases();

        var documentKeys = rawAliases.Select(x => x.DocumentKey).Distinct().ToList();
        var toSave = new List<PublishedDocumentUrlAlias>();

        foreach (DocumentUrlAliasRaw raw in rawAliases)
        {
            // Invariant content (LanguageId = null) is stored for ALL languages (like DocumentUrlService)
            // This avoids cache invalidation when languages change or content varies by culture changes
            if (raw.LanguageId is null)
            {
                foreach (ILanguage language in languages)
                {
                    foreach (var alias in NormalizeAliases(raw.AliasValue))
                    {
                        toSave.Add(new PublishedDocumentUrlAlias
                        {
                            DocumentKey = raw.DocumentKey,
                            LanguageId = language.Id,
                            Alias = alias,
                        });
                    }
                }
            }
            else
            {
                foreach (var alias in NormalizeAliases(raw.AliasValue))
                {
                    toSave.Add(new PublishedDocumentUrlAlias
                    {
                        DocumentKey = raw.DocumentKey,
                        LanguageId = raw.LanguageId.Value,
                        Alias = alias,
                    });
                }
            }
        }

        // Clear existing database records and save new
        scope.WriteLock(Constants.Locks.DocumentUrlAliases);
        _documentUrlAliasRepository.DeleteByDocumentKey(documentKeys);
        if (toSave.Count > 0)
        {
            _documentUrlAliasRepository.Save(toSave);
        }

        _keyValueService.SetValue(RebuildKey, CurrentRebuildValue);

        // Clear and repopulate the in-memory cache
        _aliasCache.Clear();
        _documentToAliasesCache.Clear();

        foreach (PublishedDocumentUrlAlias alias in toSave)
        {
            AddToCache(alias);
        }

        _logger.LogInformation("Rebuilt {AliasCount} document aliases.", toSave.Count);

        scope.Complete();
    }

    private async Task<List<PublishedDocumentUrlAlias>> ExtractAliasesFromDocumentAsync(IContent document)
    {
        var aliases = new List<PublishedDocumentUrlAlias>();
        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();

        // Handle invariant content - store alias for ALL languages (like DocumentUrlService).
        // This avoids cache invalidation when languages change or content varies by culture changes.
        if (document.ContentType.VariesByCulture() is false)
        {
            var aliasValue = document.GetValue<string>(Constants.Conventions.Content.UrlAlias);

            if (!string.IsNullOrWhiteSpace(aliasValue))
            {
                foreach (ILanguage language in languages)
                {
                    foreach (var alias in NormalizeAliases(aliasValue))
                    {
                        aliases.Add(new PublishedDocumentUrlAlias
                        {
                            DocumentKey = document.Key,
                            LanguageId = language.Id,
                            Alias = alias,
                        });
                    }
                }
            }

            return aliases;
        }

        // Handle culture-variant content
        foreach (ILanguage language in languages)
        {
            var aliasValue = document.GetValue<string>(Constants.Conventions.Content.UrlAlias, language.IsoCode);

            if (string.IsNullOrWhiteSpace(aliasValue))
            {
                continue;
            }

            foreach (var alias in NormalizeAliases(aliasValue))
            {
                aliases.Add(new PublishedDocumentUrlAlias
                {
                    DocumentKey = document.Key,
                    LanguageId = language.Id,
                    Alias = alias,
                });
            }
        }

        return aliases;
    }

    private bool ShouldRebuildAliases()
    {
        var persistedValue = _keyValueService.GetValue(RebuildKey);
        return string.Equals(persistedValue, CurrentRebuildValue, StringComparison.Ordinal) is false;
    }

    private void PopulateCultureToLanguageIdMap(IEnumerable<ILanguage> languages)
    {
        _cultureToLanguageIdMap.Clear();
        foreach (ILanguage language in languages)
        {
            if (language.IsoCode is not null)
            {
                _cultureToLanguageIdMap[language.IsoCode] = language.Id;
            }
        }
    }

    private bool TryGetLanguageId(string? culture, out int languageId)
    {
        if (string.IsNullOrEmpty(culture))
        {
            languageId = 0;
            return false;
        }

        return _cultureToLanguageIdMap.TryGetValue(culture, out languageId);
    }

    /// <summary>
    /// Adds an alias to the cache immediately. Used during initialization and rebuild.
    /// </summary>
    private void AddToCache(PublishedDocumentUrlAlias alias)
    {
        var cacheKey = new AliasCacheKey(alias.Alias, alias.LanguageId);

        _aliasCache.AddOrUpdate(
            cacheKey,
            _ => new List<Guid> { alias.DocumentKey },
            (_, existingList) =>
            {
                // Avoid duplicates - use immutable pattern for thread safety.
                if (existingList.Contains(alias.DocumentKey))
                {
                    return existingList;
                }

                // Create new list to avoid concurrent modification.
                var newList = new List<Guid>(existingList.Count + 1);
                newList.AddRange(existingList);
                newList.Add(alias.DocumentKey);
                return newList;
            });

        _documentToAliasesCache.AddOrUpdate(
            alias.DocumentKey,
            _ => new HashSet<AliasCacheKey> { cacheKey },
            (_, existingSet) =>
            {
                if (existingSet.Contains(cacheKey))
                {
                    return existingSet;
                }

                // Create new set to avoid concurrent modification.
                return new HashSet<AliasCacheKey>(existingSet) { cacheKey };
            });
    }

    /// <summary>
    /// Adds an alias to the cache when the scope completes successfully.
    /// This ensures cache updates are rolled back if the database transaction fails.
    /// </summary>
    private void AddToCacheDeferred(IScopeContext scopeContext, PublishedDocumentUrlAlias alias) =>
        scopeContext.Enlist($"AddAliasToCache_{alias.DocumentKey}_{alias.Alias}_{alias.LanguageId}", () =>
        {
            AddToCache(alias);
            return true;
        });

    /// <summary>
    /// Removes all aliases for a document from the cache immediately. Used during initialization and rebuild.
    /// </summary>
    private void RemoveFromCache(Guid documentKey)
    {
        if (_documentToAliasesCache.TryRemove(documentKey, out HashSet<AliasCacheKey>? keys) is false)
        {
            return;
        }

        foreach (AliasCacheKey key in keys)
        {
            // Use AddOrUpdate with immutable pattern for thread safety
            _aliasCache.AddOrUpdate(
                key,
                _ => new List<Guid>(), // Key doesn't exist, return empty list
                (_, existingList) =>
                {
                    // Create new list excluding the document being removed
                    return existingList.Where(k => k != documentKey).ToList();
                });

            // Clean up empty entries using atomic compare-and-remove.
            // This ensures we only remove if the value reference hasn't changed since we checked.
            // If another thread added a document (creating a new list via immutable pattern),
            // this removal will safely fail because the list reference won't match.
            if (_aliasCache.TryGetValue(key, out List<Guid>? documentKeys) && documentKeys.Count == 0)
            {
                ((ICollection<KeyValuePair<AliasCacheKey, List<Guid>>>)_aliasCache)
                    .Remove(new KeyValuePair<AliasCacheKey, List<Guid>>(key, documentKeys));
            }
        }
    }

    /// <summary>
    /// Removes all aliases for a document from the cache when the scope completes successfully.
    /// This ensures cache updates are rolled back if the database transaction fails.
    /// </summary>
    private void RemoveFromCacheDeferred(IScopeContext scopeContext, Guid documentKey) =>
        scopeContext.Enlist($"RemoveAliasFromCache_{documentKey}", () =>
        {
            RemoveFromCache(documentKey);
            return true;
        });

    private IEnumerable<string> NormalizeAliases(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            yield break;
        }

        foreach (var alias in rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var normalized = this.NormalizeAlias(alias);
            if (!string.IsNullOrEmpty(normalized))
            {
                yield return normalized;
            }
        }
    }

    private void ThrowIfNotInitialized()
    {
        if (_isInitialized is false)
        {
            throw new InvalidOperationException("The DocumentUrlAliasService needs to be initialized before it can be used.");
        }
    }
}
