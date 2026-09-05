using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.Security;

/// <summary>
///     Default <see cref="IBackOfficeSecurityAccessor" /> implementation that resolves the
///     backoffice identity from the current HTTP context's request services, with support for
///     <see cref="AsyncLocal{T}" />-based overrides for background processing scenarios
///     where no HTTP context is available.
/// </summary>
public class BackOfficeSecurityAccessor : IBackOfficeSecurityAccessor
{
    private static readonly AsyncLocal<IBackOfficeSecurity?> _ambientOverride = new();
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeSecurityAccessor" /> class.
    /// </summary>
    public BackOfficeSecurityAccessor(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    /// <summary>
    ///     Gets the current <see cref="IBackOfficeSecurity" /> object.
    /// </summary>
    /// <remarks>
    ///     Returns the ambient override if one has been set for the current async flow via
    ///     <see cref="Override" />, otherwise resolves from the HTTP context's request services.
    ///     RequestServices can be null when testing, even though compiler says it can't.
    /// </remarks>
    public IBackOfficeSecurity? BackOfficeSecurity
        => _ambientOverride.Value
           ?? _httpContextAccessor.HttpContext?.RequestServices?.GetService<IBackOfficeSecurity>();

    /// <inheritdoc />
    public IDisposable Override(IBackOfficeSecurity backOfficeSecurity)
    {
        ArgumentNullException.ThrowIfNull(backOfficeSecurity);

        IBackOfficeSecurity? previous = _ambientOverride.Value;
        _ambientOverride.Value = backOfficeSecurity;
        return new OverrideScope(previous);
    }

    /// <summary>
    ///     Disposable scope that restores the previous <see cref="AsyncLocal{T}" />-based ambient
    ///     override for the current async flow when disposed, supporting nested overrides.
    /// </summary>
    private sealed class OverrideScope : IDisposable
    {
        private readonly IBackOfficeSecurity? _previous;

        public OverrideScope(IBackOfficeSecurity? previous) => _previous = previous;

        public void Dispose() => _ambientOverride.Value = _previous;
    }
}
