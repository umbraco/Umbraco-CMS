using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     An API controller used for dealing with member types
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMembersOrMemberTypes)]
public class MemberTypeQueryController : BackOfficeNotificationsController
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly IUmbracoMapper _umbracoMapper;


    public MemberTypeQueryController(
        IMemberTypeService memberTypeService,
        IUmbracoMapper umbracoMapper)
    {
        _memberTypeService = memberTypeService ?? throw new ArgumentNullException(nameof(memberTypeService));
        _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
    }

    /// <summary>
    ///     Returns all member types
    /// </summary>
    public IEnumerable<ContentTypeBasic> GetAllTypes() =>
        _memberTypeService.GetAll()
            .Select(_umbracoMapper.Map<IMemberType, ContentTypeBasic>).WhereNotNull();
}
