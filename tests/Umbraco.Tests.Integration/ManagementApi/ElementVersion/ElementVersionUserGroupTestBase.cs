using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.ElementVersion;

/// <summary>
/// Provides an element and one of its versions for the element version endpoint tests.
/// </summary>
public abstract class ElementVersionUserGroupTestBase<T> : ManagementApiUserGroupTestBase<T>
    where T : ManagementApiControllerBase
{
    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementVersionService ElementVersionService => GetRequiredService<IElementVersionService>();

    protected Guid ElementKey { get; private set; }

    protected Guid VersionId { get; private set; }

    [SetUp]
    public new async Task Setup()
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
            Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
        };

        var createAttempt = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createAttempt.Success, $"Could not create the element: {createAttempt.Status}");

        ElementKey = createAttempt.Result!.Content!.Key;

        Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus> versionAttempt =
            await ElementVersionService.GetPagedContentVersionsAsync(ElementKey, null, 0, 10);
        Assert.IsTrue(versionAttempt.Success, $"Could not read the element versions: {versionAttempt.Status}");

        // Version identifiers are surfaced to the API as the integer version id reinterpreted as a Guid.
        VersionId = versionAttempt.Result!.Items.First().VersionId.ToGuid();
    }
}
