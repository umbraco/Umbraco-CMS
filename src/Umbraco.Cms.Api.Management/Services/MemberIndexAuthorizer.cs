using System.Security.Claims;
using Examine;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Services;

/// <inheritdoc />
public class MemberIndexAuthorizer : IMemberIndexAuthorizer
{
    private readonly IExamineManager _examineManager;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberIndexAuthorizer"/> class.
    /// </summary>
    /// <param name="examineManager">The <see cref="IExamineManager"/> used to resolve indexes by name.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> used to evaluate section access.</param>
    public MemberIndexAuthorizer(IExamineManager examineManager, IAuthorizationService authorizationService)
    {
        _examineManager = examineManager;
        _authorizationService = authorizationService;
    }

    /// <inheritdoc />
    public bool IsMemberIndex(IIndex index) => index is IUmbracoMemberIndex;

    /// <inheritdoc />
    public bool IsMemberIndex(string indexOrSearcherName)
        => _examineManager.Indexes.Any(index => IsMemberIndex(index)
            && (index.Name.InvariantEquals(indexOrSearcherName)
                || index.Searcher.Name.InvariantEquals(indexOrSearcherName)));

    /// <inheritdoc />
    public async Task<bool> HasAccessAsync(ClaimsPrincipal principal)
        => (await _authorizationService.AuthorizeAsync(principal, AuthorizationPolicies.SectionAccessMembers))
            .Succeeded;
}
