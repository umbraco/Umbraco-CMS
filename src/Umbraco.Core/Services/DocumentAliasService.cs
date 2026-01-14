using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Implements <see cref="IDocumentAliasService"/> operations for handling document URL aliases.
/// </summary>
public class DocumentAliasService : IDocumentAliasService
{
    private const string RebuildKey = "Umbraco.Core.DocumentAlias.Generation";

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

    private readonly ILogger<DocumentAliasService> _logger;
    private readonly IDocumentAliasRepository _documentAliasRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly ILanguageService _languageService;
    private readonly IKeyValueService _keyValueService;
    private readonly IContentService _contentService;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;

    // Lookup: alias -> list of matching documents (multiple docs can have same alias).
    private readonly ConcurrentDictionary<AliasCacheKey, List<DocumentAliasEntry>> _aliasCache = new();

    // Reverse lookup: document -> aliases (for cache invalidation).
    private readonly ConcurrentDictionary<Guid, HashSet<AliasCacheKey>> _documentToAliasesCache = new();

    // Culture to language ID map (for memory efficiency).
    private readonly ConcurrentDictionary<string, int> _cultureToLanguageIdMap = new();

    private bool _isInitialized;

    /// <summary>
    /// Struct-based cache key for memory-efficient alias caching.
    /// </summary>
    internal readonly struct AliasCacheKey : IEquatable<AliasCacheKey>
    {
        /// <summary>
        /// Initializes a new instance of the AliasCacheKey class with the specified normalized alias and language
        /// identifier.
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
    /// Entry stores document key and root for domain scoping.
    /// </summary>
    internal sealed class DocumentAliasEntry
    {
        /// <summary>
        /// Gets the unique identifier for the document.
        /// </summary>
        public Guid DocumentKey { get; init; }

        /// <summary>
        /// Gets the unique identifier of the root ancestor entity in the hierarchy.
        /// </summary>
        public Guid RootAncestorKey { get; init; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentAliasService"/> class.
    /// </summary>
    public DocumentAliasService(
        ILogger<DocumentAliasService> logger,
        IDocumentAliasRepository documentAliasRepository,
        ICoreScopeProvider coreScopeProvider,
        ILanguageService languageService,
        IKeyValueService keyValueService,
        IContentService contentService,
        IDocumentNavigationQueryService documentNavigationQueryService)
    {
        _logger = logger;
        _documentAliasRepository = documentAliasRepository;
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

        IEnumerable<PublishedDocumentAlias> aliases = _documentAliasRepository.GetAll();
        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();

        PopulateCultureToLanguageIdMap(languages);

        var aliasCount = 0;
        foreach (PublishedDocumentAlias alias in aliases)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            AddToCache(alias);
            aliasCount++;
        }

        _logger.LogInformation("Cached {AliasCount} document aliases.", aliasCount);

        _isInitialized = true;
        scope.Complete();
    }

    /// <inheritdoc/>
    public Guid? GetDocumentKeyByAlias(string alias, string? culture, Guid? domainRootKey)
    {
        ThrowIfNotInitialized();

        var normalizedAlias = NormalizeAlias(alias);
        if (string.IsNullOrEmpty(normalizedAlias))
        {
            return null;
        }

        // Default to the default language when culture is not specified (like DocumentUrlService)
        culture ??= _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult();

        if (!TryGetLanguageId(culture, out var languageId))
        {
            return null;
        }

        var cacheKey = new AliasCacheKey(normalizedAlias, languageId);

        if (!_aliasCache.TryGetValue(cacheKey, out List<DocumentAliasEntry>? entries) || entries.Count == 0)
        {
            return null;
        }

        // If no domain scoping, return first match
        if (domainRootKey is null)
        {
            return entries[0].DocumentKey;
        }

        // Filter by domain scope - find entry where RootAncestorKey matches or is descendant of domain root
        foreach (DocumentAliasEntry entry in entries)
        {
            if (entry.RootAncestorKey == domainRootKey.Value ||
                IsDescendantOf(entry.DocumentKey, domainRootKey.Value))
            {
                return entry.DocumentKey;
            }
        }

        // Fallback to first match if no domain-scoped match found
        return entries[0].DocumentKey;
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateAliasesAsync(Guid documentKey)
    {
        ThrowIfNotInitialized();

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        IContent? document = _contentService.GetById(documentKey);
        if (document is null || document.Trashed || document.Blueprint)
        {
            // Remove from cache if document doesn't exist, is trashed, or is a blueprint
            RemoveFromCache(documentKey);
            scope.Complete();
            return;
        }

        List<PublishedDocumentAlias> aliases = await ExtractAliasesFromDocumentAsync(document);

        // Remove old aliases from cache
        RemoveFromCache(documentKey);

        // Save to database (handles insert/update/delete via diff) and add to cache
        if (aliases.Count > 0)
        {
            scope.WriteLock(Constants.Locks.DocumentAliases);
            _documentAliasRepository.Save(aliases);

            foreach (PublishedDocumentAlias alias in aliases)
            {
                AddToCache(alias);
            }
        }

        scope.Complete();
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateAliasesWithDescendantsAsync(Guid documentKey)
    {
        ThrowIfNotInitialized();

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        // Get document and all descendants
        var documentKeys = new List<Guid> { documentKey };
        if (_documentNavigationQueryService.TryGetDescendantsKeys(documentKey, out IEnumerable<Guid> descendantKeys))
        {
            documentKeys.AddRange(descendantKeys);
        }

        foreach (Guid key in documentKeys)
        {
            await CreateOrUpdateAliasesAsync(key);
        }

        scope.Complete();
    }

    /// <inheritdoc/>
    public Task DeleteAliasesFromCacheAsync(IEnumerable<Guid> documentKeys)
    {
        ThrowIfNotInitialized();

        foreach (Guid documentKey in documentKeys)
        {
            RemoveFromCache(documentKey);
        }

        return Task.CompletedTask;
    }

    private async Task RebuildAllAliasesAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);

        // Get all languages for culture-to-languageId mapping
        var languages = (await _languageService.GetAllAsync()).ToList();
        PopulateCultureToLanguageIdMap(languages);

        // Use optimized SQL query to fetch only documents with aliases
        IEnumerable<DocumentAliasRaw> rawAliases = _documentAliasRepository.GetAllDocumentAliases();

        // Get root ancestor keys for domain scoping
        var documentKeys = rawAliases.Select(x => x.DocumentKey).Distinct().ToList();
        Dictionary<Guid, Guid> rootAncestorMap = GetRootAncestorKeys(documentKeys);

        var toSave = new List<PublishedDocumentAlias>();

        foreach (DocumentAliasRaw raw in rawAliases)
        {
            Guid rootAncestorKey = rootAncestorMap.GetValueOrDefault(raw.DocumentKey, raw.DocumentKey);

            // Invariant content (LanguageId = null) is stored for ALL languages (like DocumentUrlService)
            // This avoids cache invalidation when languages change or content varies by culture changes
            if (raw.LanguageId is null)
            {
                foreach (ILanguage language in languages)
                {
                    foreach (var alias in NormalizeAliases(raw.AliasValue))
                    {
                        toSave.Add(new PublishedDocumentAlias
                        {
                            DocumentKey = raw.DocumentKey,
                            LanguageId = language.Id,
                            Alias = alias,
                            RootAncestorKey = rootAncestorKey,
                        });
                    }
                }
            }
            else
            {
                foreach (var alias in NormalizeAliases(raw.AliasValue))
                {
                    toSave.Add(new PublishedDocumentAlias
                    {
                        DocumentKey = raw.DocumentKey,
                        LanguageId = raw.LanguageId.Value,
                        Alias = alias,
                        RootAncestorKey = rootAncestorKey,
                    });
                }
            }
        }

        // Clear existing and save new
        scope.WriteLock(Constants.Locks.DocumentAliases);
        _documentAliasRepository.DeleteByDocumentKey(documentKeys);
        if (toSave.Count > 0)
        {
            _documentAliasRepository.Save(toSave);
        }

        _keyValueService.SetValue(RebuildKey, CurrentRebuildValue);

        scope.Complete();
    }

    private async Task<List<PublishedDocumentAlias>> ExtractAliasesFromDocumentAsync(IContent document)
    {
        var aliases = new List<PublishedDocumentAlias>();
        Guid rootAncestorKey = GetRootAncestorKey(document.Key);
        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();

        // Handle invariant content - store alias for ALL languages (like DocumentUrlService)
        // This avoids cache invalidation when languages change or content varies by culture changes
        if (document.ContentType.VariesByCulture() is false)
        {
            var aliasValue = document.GetValue<string>(Constants.Conventions.Content.UrlAlias);

            if (!string.IsNullOrWhiteSpace(aliasValue))
            {
                foreach (ILanguage language in languages)
                {
                    foreach (var alias in NormalizeAliases(aliasValue))
                    {
                        aliases.Add(new PublishedDocumentAlias
                        {
                            DocumentKey = document.Key,
                            LanguageId = language.Id,
                            Alias = alias,
                            RootAncestorKey = rootAncestorKey,
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
                aliases.Add(new PublishedDocumentAlias
                {
                    DocumentKey = document.Key,
                    LanguageId = language.Id,
                    Alias = alias,
                    RootAncestorKey = rootAncestorKey,
                });
            }
        }

        return aliases;
    }

    private bool ShouldRebuildAliases()
    {
        var persistedValue = _keyValueService.GetValue(RebuildKey);
        return !string.Equals(persistedValue, CurrentRebuildValue, StringComparison.Ordinal);
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

    private void AddToCache(PublishedDocumentAlias alias)
    {
        var cacheKey = new AliasCacheKey(alias.Alias, alias.LanguageId);
        var entry = new DocumentAliasEntry
        {
            DocumentKey = alias.DocumentKey,
            RootAncestorKey = alias.RootAncestorKey
        };

        _aliasCache.AddOrUpdate(
            cacheKey,
            _ => new List<DocumentAliasEntry> { entry },
            (_, existingList) =>
            {
                // Avoid duplicates - use immutable pattern for thread safety
                if (existingList.Any(e => e.DocumentKey == entry.DocumentKey))
                {
                    return existingList;
                }

                // Create new list to avoid concurrent modification
                var newList = new List<DocumentAliasEntry>(existingList.Count + 1);
                newList.AddRange(existingList);
                newList.Add(entry);
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

                // Create new set to avoid concurrent modification
                return new HashSet<AliasCacheKey>(existingSet) { cacheKey };
            });
    }

    private void RemoveFromCache(Guid documentKey)
    {
        if (!_documentToAliasesCache.TryRemove(documentKey, out HashSet<AliasCacheKey>? keys))
        {
            return;
        }

        foreach (AliasCacheKey key in keys)
        {
            // Use AddOrUpdate with immutable pattern for thread safety
            _aliasCache.AddOrUpdate(
                key,
                _ => new List<DocumentAliasEntry>(), // Key doesn't exist, return empty list
                (_, existingList) =>
                {
                    // Create new list excluding the document being removed
                    return existingList.Where(e => e.DocumentKey != documentKey).ToList();
                });

            // Clean up empty entries
            if (_aliasCache.TryGetValue(key, out List<DocumentAliasEntry>? entries) && entries.Count == 0)
            {
                _aliasCache.TryRemove(key, out _);
            }
        }
    }

    private Dictionary<Guid, Guid> GetRootAncestorKeys(IEnumerable<Guid> documentKeys)
    {
        var result = new Dictionary<Guid, Guid>();
        foreach (Guid documentKey in documentKeys)
        {
            result[documentKey] = GetRootAncestorKey(documentKey);
        }
        return result;
    }

    private Guid GetRootAncestorKey(Guid documentKey)
    {
        if (_documentNavigationQueryService.TryGetAncestorsKeys(documentKey, out IEnumerable<Guid> ancestorKeys))
        {
            var ancestors = ancestorKeys.ToList();
            if (ancestors.Count > 0)
            {
                return ancestors[^1]; // Last ancestor is the root
            }
        }
        return documentKey; // Document is itself a root
    }

    private bool IsDescendantOf(Guid documentKey, Guid potentialAncestorKey)
    {
        if (_documentNavigationQueryService.TryGetAncestorsKeys(documentKey, out IEnumerable<Guid> ancestorKeys))
        {
            return ancestorKeys.Contains(potentialAncestorKey);
        }
        return false;
    }

    private static string NormalizeAlias(string alias)
    {
        return alias
            .Trim()
            .TrimStart('/')
            .TrimEnd('/')
            .ToLowerInvariant();
    }

    private static IEnumerable<string> NormalizeAliases(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            yield break;
        }

        foreach (var alias in rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var normalized = NormalizeAlias(alias);
            if (!string.IsNullOrEmpty(normalized))
            {
                yield return normalized;
            }
        }
    }

    private void ThrowIfNotInitialized()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("The DocumentAliasService needs to be initialized before it can be used.");
        }
    }
}
