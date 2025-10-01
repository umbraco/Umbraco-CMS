using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.MemberType.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.MemberType.Item;

public class ItemMemberTypeItemControllerTests : ManagementApiUserGroupTestBase<ItemMemberTypeItemController>
{
    private IMemberTypeEditingService MemberTypeEditingService => GetRequiredService<IMemberTypeEditingService>();

    private Guid _memberTypeKey;

    [SetUp]
    public async Task SetUp()
    {
        var memberTypeModel = new MemberTypeCreateModel() { Alias = Guid.NewGuid().ToString(), Name = "Test Member Type", };
        var memberTypeResponse = await MemberTypeEditingService.CreateAsync(memberTypeModel, Constants.Security.SuperUserKey);
        _memberTypeKey = memberTypeResponse.Result.Key;
    }

    protected override Expression<Func<ItemMemberTypeItemController, object>> MethodSelector =>
        x => x.Item(CancellationToken.None, new HashSet<Guid> { _memberTypeKey });

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
