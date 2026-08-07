using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element;

public class ByKeyPublishedElementControllerTests : ManagementApiUserGroupTestBase<ByKeyPublishedElementController>
{
    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _elementKey;

    [SetUp]
    public async Task Setup()
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString())
            .WithName("Test Element")
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = null,
            Variants = [new VariantModel { Name = "Test Element Instance" }],
        };
        var createResponse = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResponse.Success, $"Failed to create element: {createResponse.Status}");
        _elementKey = createResponse.Result!.Content!.Key;

        var publishResponse = await ElementPublishingService.PublishAsync(
            _elementKey,
            [new CulturePublishScheduleModel { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResponse.Success, $"Failed to publish element: {publishResponse.Status}");
    }

    protected override Expression<Func<ByKeyPublishedElementController, object>> MethodSelector =>
        x => x.ByKeyPublished(CancellationToken.None, _elementKey);

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
