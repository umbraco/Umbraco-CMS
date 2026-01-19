using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.RecycleBin;

public class SiblingsElementRecycleBinControllerTests : ElementRecycleBinControllerTestBase<SiblingsElementRecycleBinController>
{
    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _elementKey;

    [SetUp]
    public async Task Setup()
    {
        // Element Type
        var elementType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString())
            .WithName("Test Element")
            .WithIsElement(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        // Create two elements at root so we have siblings in the recycle bin
        var createModel1 = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = null,
            Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
        };
        var response1 = await ElementEditingService.CreateAsync(createModel1, Constants.Security.SuperUserKey);
        _elementKey = response1.Result!.Content!.Key;

        var createModel2 = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = null,
            Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
        };
        var response2 = await ElementEditingService.CreateAsync(createModel2, Constants.Security.SuperUserKey);

        // Move both to recycle bin
        await ElementEditingService.MoveToRecycleBinAsync(_elementKey, Constants.Security.SuperUserKey);
        await ElementEditingService.MoveToRecycleBinAsync(response2.Result!.Content!.Key, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<SiblingsElementRecycleBinController, object>> MethodSelector =>
        x => x.Siblings(CancellationToken.None, _elementKey, 0, 10, null);

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
