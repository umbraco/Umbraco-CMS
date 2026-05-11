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

    /// <summary>
    /// Gets or sets the default culture ISO code used when no culture is specified.
    /// </summary>
    protected string? DefaultCulture { get; set; }

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
        if (string.IsNullOrEmpty(culture) && DefaultCulture is not null)
        {
            culture = DefaultCulture;
        }

        if (_publishedCultures.TryGetValue(key, out ISet<string>? publishedCultures))
        {
            // If "*" is provided as the culture, we consider this as "published in any culture". This aligns
            // with behaviour in Umbraco 13.
            return culture == Constants.System.InvariantCulture || publishedCultures.Contains(culture, StringComparer.InvariantCultureIgnoreCase);
        }

        _logger.LogDebug("{EntityType} {Key} not found in the publish status cache", _entityTypeName, key);
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

        _logger.LogDebug("{EntityType} {Key} not found in the publish status cache", _entityTypeName, key);
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
        => _publishedCultures[key] = publishedCultures;

    /// <summary>
    /// Removes an item from the publish status cache.
    /// </summary>
    /// <param name="key">The item's key.</param>
    protected void RemoveStatus(Guid key)
        => _publishedCultures.TryRemove(key, out _);
}
