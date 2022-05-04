// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Scoping;

/// <summary>
///     Cleans up orphaned <see cref="IScope" /> references at the end of a request
/// </summary>
public interface IHttpScopeReference : IDisposable
{
    /// <summary>
    ///     Register for cleanup in the request
    /// </summary>
    void Register();
}
