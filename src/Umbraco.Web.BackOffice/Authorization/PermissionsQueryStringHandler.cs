// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Abstract base class providing common functionality for authorization checks based on querystrings.
/// </summary>
/// <typeparam name="T">Authorization requirement</typeparam>
public abstract class PermissionsQueryStringHandler<T> : MustSatisfyRequirementAuthorizationHandler<T>
    where T : IAuthorizationRequirement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PermissionsQueryStringHandler{T}" /> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back-office security.</param>
    /// <param name="httpContextAccessor">Accessor for the HTTP context of the current request.</param>
    /// <param name="entityService">Service for entity operations.</param>
    public PermissionsQueryStringHandler(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IHttpContextAccessor httpContextAccessor,
        IEntityService entityService)
    {
        BackOfficeSecurityAccessor = backOfficeSecurityAccessor;
        HttpContextAccessor = httpContextAccessor;
        EntityService = entityService;
    }

    /// <summary>
    ///     Gets or sets the <see cref="IBackOfficeSecurityAccessor" /> instance.
    /// </summary>
    protected IBackOfficeSecurityAccessor BackOfficeSecurityAccessor { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="IHttpContextAccessor" /> instance.
    /// </summary>
    protected IHttpContextAccessor HttpContextAccessor { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="IEntityService" /> instance.
    /// </summary>
    protected IEntityService EntityService { get; set; }

    /// <summary>
    ///     Attempts to parse a node ID from a string representation found in a querystring value.
    /// </summary>
    /// <param name="argument">Querystring value.</param>
    /// <param name="nodeId">Output parsed Id.</param>
    /// <returns>True of node ID could be parased, false it not.</returns>
    protected bool TryParseNodeId(string argument, out int nodeId)
    {
        // If the argument is an int, it will parse and can be assigned to nodeId.
        // It might be a udi, so check that next.
        // Otherwise treat it as a guid - unlikely we ever get here.
        // Failing that, we can't parse it.
        if (int.TryParse(argument, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedId))
        {
            nodeId = parsedId;
            return true;
        }

        if (UdiParser.TryParse(argument, true, out Udi? udi))
        {
            nodeId = EntityService.GetId(udi).Result;
            return true;
        }

        if (Guid.TryParse(argument, out Guid key))
        {
            nodeId = EntityService.GetId(key, UmbracoObjectTypes.Document).Result;
            return true;
        }

        nodeId = 0;
        return false;
    }
}
