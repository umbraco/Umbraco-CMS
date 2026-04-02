using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory interface for creating redirect URL presentation models.
/// </summary>
public interface IRedirectUrlPresentationFactory
{
    /// <summary>Creates a <see cref="RedirectUrlResponseModel"/> from the given <see cref="IRedirectUrl"/> source.</summary>
    /// <param name="source">The source <see cref="IRedirectUrl"/> to create the response model from.</param>
    /// <returns>A <see cref="RedirectUrlResponseModel"/> representing the redirect URL.</returns>
    RedirectUrlResponseModel Create(IRedirectUrl source);

    /// <summary>
    /// Creates a collection of <see cref="RedirectUrlResponseModel"/> from the given <see cref="IRedirectUrl"/> sources.
    /// </summary>
    /// <param name="sources">The collection of <see cref="IRedirectUrl"/> instances to convert.</param>
    /// <returns>An enumerable of <see cref="RedirectUrlResponseModel"/> representing the converted redirect URLs.</returns>
    IEnumerable<RedirectUrlResponseModel> CreateMany(IEnumerable<IRedirectUrl> sources);
}
