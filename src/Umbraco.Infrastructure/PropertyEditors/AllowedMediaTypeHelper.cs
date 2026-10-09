// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Shared helper for checking whether a media type alias is among a set of allowed media type keys.
/// Used by both RTE and MediaPicker3 allowed-type validators.
/// </summary>
internal sealed class AllowedMediaTypeHelper
{
    private const string MediaTypeCacheKeyFormat = nameof(AllowedMediaTypeHelper) + "_MediaTypeKey_{0}";

    private readonly IMediaTypeService _mediaTypeService;
    private readonly AppCaches _appCaches;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedMediaTypeHelper"/> class.
    /// </summary>
    /// <param name="mediaTypeService">Service for media type lookups.</param>
    /// <param name="appCaches">Application caches for request-level caching.</param>
    public AllowedMediaTypeHelper(IMediaTypeService mediaTypeService, AppCaches appCaches)
    {
        _mediaTypeService = mediaTypeService;
        _appCaches = appCaches;
    }

    /// <summary>
    /// Checks whether a media type alias resolves to one of the allowed media type keys.
    /// </summary>
    /// <param name="typeAlias">The media type alias to check.</param>
    /// <param name="allowedTypeKeys">The set of allowed media type keys (case-insensitive).</param>
    /// <returns><c>true</c> if the alias resolves to an allowed key; otherwise <c>false</c>.</returns>
    public bool IsAllowed(string typeAlias, HashSet<string> allowedTypeKeys)
    {
        var typeKey = GetMediaTypeKey(typeAlias);
        return typeKey is not null && allowedTypeKeys.Contains(typeKey);
    }

    /// <summary>
    /// Parses a comma-separated allowed types configuration string into a case-insensitive set of keys.
    /// </summary>
    /// <returns>
    /// A case-insensitive set of allowed media type keys, or an empty set if the configuration is empty or
    /// not set (meaning all types are allowed).
    /// </returns>
    public static HashSet<string> ParseAllowedTypeKeys(string? configValue)
    {
        var allowedTypes = configValue?.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);

        if (allowedTypes is null || allowedTypes.Length == 0)
        {
            return [];
        }

        return new HashSet<string>(allowedTypes, StringComparer.OrdinalIgnoreCase);
    }

    private string? GetMediaTypeKey(string typeAlias)
    {
        string? GetMediaTypeKeyFromService(string alias) => _mediaTypeService.Get(alias)?.Key.ToString();

        if (_appCaches.RequestCache.IsAvailable is false)
        {
            return GetMediaTypeKeyFromService(typeAlias);
        }

        var cacheKey = string.Format(MediaTypeCacheKeyFormat, typeAlias);
        var typeKey = _appCaches.RequestCache.GetCacheItem<string?>(cacheKey);
        if (typeKey is null)
        {
            typeKey = GetMediaTypeKeyFromService(typeAlias);
            if (typeKey is not null)
            {
                _appCaches.RequestCache.Set(cacheKey, typeKey);
            }
        }

        return typeKey;
    }
}
