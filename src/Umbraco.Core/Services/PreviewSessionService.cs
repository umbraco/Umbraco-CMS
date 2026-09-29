// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class PreviewSessionService : IPreviewSessionService
{
    private const string SessionCacheKey = "PreviewSessionService:IsInPreview";

    private readonly IRequestCache _requestCache;

    public PreviewSessionService(IRequestCache requestCache)
        => _requestCache = requestCache;

    /// <inheritdoc />
    public void Start() => _requestCache.Set(SessionCacheKey, true);

    /// <inheritdoc />
    public bool IsActive() => _requestCache.Get(SessionCacheKey) is true;
}
