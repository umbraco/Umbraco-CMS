using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Website.Security;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> for the Umbraco back office
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds support for external login providers in Umbraco
    /// </summary>
    public static IUmbracoBuilder AddMemberExternalLogins(
        this IUmbracoBuilder umbracoBuilder,
        Action<MemberExternalLoginsBuilder> builder)
    {
        builder(new MemberExternalLoginsBuilder(umbracoBuilder.Services));
        return umbracoBuilder;
    }
}
