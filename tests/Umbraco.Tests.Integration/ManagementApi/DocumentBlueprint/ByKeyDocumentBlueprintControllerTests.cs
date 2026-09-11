using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentBlueprint;

public class ByKeyDocumentBlueprintControllerTests : ManagementApiUserGroupTestBase<ByKeyDocumentBlueprintController>
{
    private IContentBlueprintEditingService ContentBlueprintEditingService =>
        GetRequiredService<IContentBlueprintEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _documentBlueprintKey;

    [SetUp]
    public new async Task Setup()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Document Type {Guid.NewGuid()}")
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var result = await ContentBlueprintEditingService.CreateAsync(
            new ContentBlueprintCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = Constants.System.RootKey,
                Variants = [new VariantModel { Name = $"Blueprint {Guid.NewGuid()}" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create the blueprint: {result.Status}");
        _documentBlueprintKey = result.Result.Content!.Key;
    }

    protected override Expression<Func<ByKeyDocumentBlueprintController, object>> MethodSelector =>
        x => x.ByKey(CancellationToken.None, _documentBlueprintKey);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };
}
