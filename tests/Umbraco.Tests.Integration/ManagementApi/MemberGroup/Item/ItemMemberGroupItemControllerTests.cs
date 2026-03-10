using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.MemberGroup.Item;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.MemberGroup.Item;

public class ItemMemberGroupItemControllerTests : ManagementApiUserGroupTestBase<ItemMemberGroupItemController>
{
    private IMemberGroupService MemberGroupService => GetRequiredService<IMemberGroupService>();

    private Guid _memberGroupKey;

    [SetUp]
    public async Task SetUp()
    {
        var memberGroup = new Core.Models.MemberGroup { Name = "Test Member Group" };
        MemberGroupService.Save(memberGroup);
        _memberGroupKey = memberGroup.Key;
    }

    protected override Expression<Func<ItemMemberGroupItemController, object>> MethodSelector =>
        x => x.Item(CancellationToken.None, new HashSet<Guid> { _memberGroupKey });

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };
}
