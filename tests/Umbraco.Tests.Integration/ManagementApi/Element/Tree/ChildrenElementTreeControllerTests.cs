using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.Tree;

public class ChildrenElementTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenElementTreeController>
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _parentKey;

    [SetUp]
    public async Task Setup()
    {
        // Create element type
        var elementType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString())
            .WithName("Test Element")
            .WithIsElement(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        // Create parent container
        var parentResult = await ElementContainerService.CreateAsync(null, "ParentContainer", null, Constants.Security.SuperUserKey);
        _parentKey = parentResult.Result!.Key;

        // Create child container
        await ElementContainerService.CreateAsync(null, "ChildContainer", _parentKey, Constants.Security.SuperUserKey);

        // Create child element
        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = _parentKey,
            Variants = [new VariantModel { Name = "ChildElement" }],
        };
        await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ChildrenElementTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, _parentKey, 0, 100, false);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };
}
