using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Member.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Member.Item;

public class ItemMemberItemControllerTests : ManagementApiUserGroupTestBase<ItemMemberItemController>
{
    private IMemberTypeEditingService MemberTypeEditingService => GetRequiredService<IMemberTypeEditingService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private Guid _memberKey;

    [SetUp]
    public async Task SetUp()
    {
        // Member Type
        var memberTypeModel = new MemberTypeCreateModel() { Alias = Guid.NewGuid().ToString(), Name = "Test Member" };
        var memberTypeResponse = await MemberTypeEditingService.CreateAsync(memberTypeModel, Constants.Security.SuperUserKey);

        // Member
        var member = MemberService.CreateMember(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), memberTypeResponse.Result.Alias);
        MemberService.Save(member);
        _memberKey = member.Key;
    }

    protected override Expression<Func<ItemMemberItemController, object>> MethodSelector =>
        x => x.Item(CancellationToken.None, new HashSet<Guid> { _memberKey });

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
