// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Manages the current preview session (per request).
/// </summary>
public interface IPreviewSessionService
{
    /// <summary>
    /// Flags the current session as a preview session.
    /// </summary>
    void Start();

    /// <summary>
    /// Determines whether a preview session is currently active.
    /// </summary>
    /// <returns><c>true</c> if a preview session is active; otherwise, <c>false</c>.</returns>
    bool IsActive();
}
