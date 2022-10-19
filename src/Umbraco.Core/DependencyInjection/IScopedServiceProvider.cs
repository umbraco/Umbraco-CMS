namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
///     Provides access to a request scoped service provider when available for cases where
///     IHttpContextAccessor is not available. e.g. No reference to AspNetCore.Http in core.
/// </summary>
public interface IScopedServiceProvider
{
    /// <summary>
    ///     Gets a request scoped service provider when available.
    /// </summary>
    /// <remarks>
    ///     Can be null.
    /// </remarks>
    IServiceProvider? ServiceProvider { get; }
}
