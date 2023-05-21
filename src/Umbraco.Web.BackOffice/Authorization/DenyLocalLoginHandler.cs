// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Web.BackOffice.Security;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Ensures the resource cannot be accessed if <see cref="IBackOfficeExternalLoginProviders.HasDenyLocalLogin" />
///     returns true.
/// </summary>
public class DenyLocalLoginHandler : MustSatisfyRequirementAuthorizationHandler<DenyLocalLoginRequirement>
{
    private readonly IBackOfficeExternalLoginProviders _externalLogins;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DenyLocalLoginHandler" /> class.
    /// </summary>
    /// <param name="externalLogins">Provides access to <see cref="BackOfficeExternalLoginProvider" /> instances.</param>
    public DenyLocalLoginHandler(IBackOfficeExternalLoginProviders externalLogins) => _externalLogins = externalLogins;

    /// <inheritdoc />
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context,
        DenyLocalLoginRequirement requirement) =>
        Task.FromResult(!_externalLogins.HasDenyLocalLogin());
}
