using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.Web;

/// <summary>
/// Provides access to the current <see cref="IUmbracoContext"/>.
/// </summary>
/// <remarks>
/// <para>Provides a <see cref="TryGetUmbracoContext"/> method that returns <c>true</c> if the current <see cref="IUmbracoContext"/> is not null.</para>
/// <para>Provides a <see cref="Clear"/> method that will clear the current <see cref="IUmbracoContext"/> object.</para>
/// <para>Provides a <see cref="Set"/> method that will set the current <see cref="IUmbracoContext"/> object.</para>
/// </remarks>
public interface IUmbracoContextAccessor
{
    /// <summary>
    /// Tries to get the current <see cref="IUmbracoContext"/>.
    /// </summary>
    /// <param name="umbracoContext">When this method returns, contains the current <see cref="IUmbracoContext"/> if available; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the current <see cref="IUmbracoContext"/> is available; otherwise, <c>false</c>.</returns>
    bool TryGetUmbracoContext([MaybeNullWhen(false)] out IUmbracoContext umbracoContext);

    /// <summary>
    /// Clears the current <see cref="IUmbracoContext"/>.
    /// </summary>
    void Clear();

    /// <summary>
    /// Sets the current <see cref="IUmbracoContext"/>.
    /// </summary>
    /// <param name="umbracoContext">The <see cref="IUmbracoContext"/> to set as the current context.</param>
    void Set(IUmbracoContext umbracoContext);
}
