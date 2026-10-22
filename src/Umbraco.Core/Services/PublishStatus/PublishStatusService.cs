using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Abstract base class for publish status services, providing shared cache logic for tracking
/// which content items are published and in which cultures.
/// </summary>
public abstract class PublishStatusService
{
    private ConcurrentDictionary<Guid, ISet<string>> _publishedCultures = new();
    private readonly ILogger _logger;
    private readonly string _entityTypeName;
    private string? _defaultCulture;
    private volatile bool _defaultCultureIsStale;

    /// <summary>
    /// Gets or sets the default culture ISO code used when no culture is specified.
    /// </summary>
    protected string? DefaultCulture
    {
        get => _defaultCulture;
        set
        {
            _defaultCultureIsStale = false;
            _defaultCulture = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishStatusService"/> class.
    /// </summary>
    /// <param name="entityType">The object type this service tracks publish status for.</param>
    /// <param name="logger">The logger for diagnostic output.</param>
    protected PublishStatusService(UmbracoObjectTypes entityType, ILogger logger)
    {
        _entityTypeName = entityType.ToString();
        _logger = logger;


    }

    /// <summary>
    /// Checks if an item is published in a specific culture.
    /// </summary>
    /// <param name="key">The item's key.</param>
    /// <param name="culture">The culture to check.</param>
    /// <returns><c>true</c> if the item is published in the specified culture; otherwise, <c>false</c>.</returns>
    protected bool IsPublished(Guid key, string culture)
    {
        if (string.IsNullOrEmpty(culture) && GetDefaultCulture() is { } defaultCulture)
        {
            culture = defaultCulture;
        }

        if (_publishedCultures.TryGetValue(key, out ISet<string>? publishedCultures))
        {
            // If "*" is provided as the culture, we consider this as "published in any culture". This aligns
            // with behaviour in Umbraco 13.
            return culture == Constants.System.InvariantCulture || publishedCultures.Contains(culture, StringComparer.InvariantCultureIgnoreCase);
        }

        _logger.LogDebug("{EntityType} {EntityKey} not found in the publish status cache", _entityTypeName, key);
        return false;
    }

    /// <summary>
    /// Checks if an item is published in any culture.
    /// </summary>
    /// <param name="key">The item's key.</param>
    /// <returns><c>true</c> if the item has any published culture; otherwise, <c>false</c>.</returns>
    protected bool IsPublishedInAnyCulture(Guid key)
    {
        if (_publishedCultures.TryGetValue(key, out ISet<string>? publishedCultures))
        {
            return publishedCultures.Count > 0;
        }

        _logger.LogDebug("{EntityType} {EntityKey} not found in the publish status cache", _entityTypeName, key);
        return false;
    }

    /// <summary>
    /// Populates the cache from a dictionary of publish statuses, replacing any existing entries.
    /// </summary>
    /// <param name="publishStatus">A dictionary mapping item keys to their published culture codes.</param>
    protected void PopulateCache(IDictionary<Guid, ISet<string>> publishStatus)
    {
        // On subscriber/CD servers in a load-balanced setup, cache refresh notifications can trigger
        // re-initialization while other threads are reading _publishedCultures. The previous approach
        // called _publishedCultures.Clear() then gradually re-added entries, creating a window where
        // concurrent readers (e.g. HasPublishedAncestorPath) would see an empty dictionary and
        // incorrectly conclude that content is unpublished.
        //
        // Instead, we build a completely new dictionary from the DB, then swap it in with
        // Interlocked.Exchange. Readers that already hold a reference to the old dictionary continue
        // to use it safely; new readers pick up the fully populated replacement. There is no window
        // where the dictionary is empty.
        //
        // Verified by: PublishStatusServiceTests.Concurrent_Initialize_Never_Transiently_Loses_Published_Status
        var newDictionary = new ConcurrentDictionary<Guid, ISet<string>>(publishStatus);
        Interlocked.Exchange(ref _publishedCultures, newDictionary);
    }

    /// <summary>
    /// Adds or updates the publish status for a single item in the cache.
    /// </summary>
    /// <param name="key">The item's key.</param>
    /// <param name="publishedCultures">The set of culture codes for which the item is published.</param>
    protected void SetStatus(Guid key, ISet<string> publishedCultures)
    {
        if (publishedCultures.Count > 0)
        {
            _publishedCultures[key] = publishedCultures;
        }
        else
        {
            RemoveStatus(key);
        }
    }

    /// <summary>
    /// Removes an item from the publish status cache.
    /// </summary>
    /// <param name="key">The item's key.</param>
    protected void RemoveStatus(Guid key)
        => _publishedCultures.TryRemove(key, out _);

    /// <summary>
    /// Brings the cached publish status for a single culture in line with the given set of published items.
    /// </summary>
    /// <remarks>
    /// The culture is added to every item in <paramref name="publishedKeys"/> and removed from every other item.
    /// </remarks>
    /// <param name="culture">The culture code.</param>
    /// <param name="publishedKeys">The keys of all items that are published in the culture.</param>
    protected void RefreshCulture(string culture, ISet<Guid> publishedKeys)
    {
        // The cached sets are read concurrently, so they are replaced rather than modified in place.
        ConcurrentDictionary<Guid, ISet<string>> publishedCultures = Volatile.Read(ref _publishedCultures);
        foreach (Guid key in publishedKeys)
        {
            publishedCultures.AddOrUpdate(
                key,
                _ => new HashSet<string> { culture },
                (_, existing) => existing.Contains(culture, StringComparer.InvariantCultureIgnoreCase)
                    ? existing
                    : new HashSet<string>(existing) { culture });
        }

        foreach ((Guid key, ISet<string> existing) in publishedCultures)
        {
            if (publishedKeys.Contains(key) || existing.Contains(culture, StringComparer.InvariantCultureIgnoreCase) is false)
            {
                continue;
            }

            var remaining = existing
                .Where(x => string.Equals(x, culture, StringComparison.InvariantCultureIgnoreCase) is false)
                .ToHashSet();

            // A failed swap means the entry was reloaded from the database after the language change, so it is
            // already correct and is left as it is.
            if (remaining.Count > 0)
            {
                publishedCultures.TryUpdate(key, remaining, existing);
            }
            else
            {
                publishedCultures.TryRemove(new KeyValuePair<Guid, ISet<string>>(key, existing));
            }
        }
    }

    /// <summary>
    /// Marks the default culture as stale, so it is re-read with <see cref="ReadDefaultCulture"/> before it is next used.
    /// </summary>
    /// <remarks>
    /// The default culture is re-read on the next query that needs it, rather than straight away, so a language
    /// change does not reload the languages on every server unless a query without a culture is made.
    /// </remarks>
    protected void InvalidateDefaultCulture() => _defaultCultureIsStale = true;

    /// <summary>
    /// Reads the current default culture ISO code, when the default culture is needed after
    /// <see cref="InvalidateDefaultCulture"/> has been called.
    /// </summary>
    /// <returns>The default culture ISO code.</returns>
    protected virtual string? ReadDefaultCulture() => _defaultCulture;

    private string? GetDefaultCulture()
    {
        if (_defaultCultureIsStale)
        {
            DefaultCulture = ReadDefaultCulture();
        }

        return DefaultCulture;
    }
}
