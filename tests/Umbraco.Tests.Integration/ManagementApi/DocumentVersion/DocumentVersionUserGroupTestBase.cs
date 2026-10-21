using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentVersion;

/// <summary>
/// Provides a document and one of its versions for the document version endpoint tests.
/// </summary>
public abstract class DocumentVersionUserGroupTestBase<T> : ManagementApiUserGroupTestBase<T>
    where T : ManagementApiControllerBase
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentVersionService ContentVersionService => GetRequiredService<IContentVersionService>();

    protected Guid DocumentKey { get; private set; }

    protected Guid VersionId { get; private set; }

    [SetUp]
    public new async Task Setup()
    {
        IContentType contentType = ContentTypeBuilder.CreateBasicContentType(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
        };

        Attempt<ContentCreateResult, ContentEditingOperationStatus> createAttempt =
            await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createAttempt.Success, $"Could not create the document: {createAttempt.Status}");

        DocumentKey = createAttempt.Result.Content.Key;

        Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus> versionAttempt =
            await ContentVersionService.GetPagedContentVersionsAsync(DocumentKey, null, 0, 10);
        Assert.IsTrue(versionAttempt.Success, $"Could not read the document versions: {versionAttempt.Status}");

        // Version identifiers are surfaced to the API as the integer version id reinterpreted as a Guid.
        VersionId = versionAttempt.Result!.Items.First().VersionId.ToGuid();
    }
}
