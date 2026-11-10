using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document.References;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document.References;

/// <summary>
/// Asserts that the document reference endpoints only answer for documents within the user's content start nodes.
/// </summary>
public class DocumentReferencesStartNodeAccessTests : StartNodeAccessTestBase<ReferencedByDocumentController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContent _accessibleDocument = null!;

    private IContent _inaccessibleDocument = null!;

    protected override Expression<Func<ReferencedByDocumentController, object>> MethodSelector { get; set; } = null!;

    [SetUp]
    public async Task SetUpDocumentsAndUser()
    {
        IContentType contentType = ContentTypeBuilder.CreateBasicContentType(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IContent deniedRoot = await CreateDocumentAsync(contentType.Key, Constants.System.RootKey);
        IContent allowedRoot = await CreateDocumentAsync(contentType.Key, Constants.System.RootKey);

        _inaccessibleDocument = await CreateDocumentAsync(contentType.Key, deniedRoot.Key);
        _accessibleDocument = await CreateDocumentAsync(contentType.Key, allowedRoot.Key);

        await AuthenticateRestrictedUserAsync([Constants.Applications.Content], startContentId: allowedRoot.Id);
    }

    [Test]
    public async Task ReferencedBy_Is_Forbidden_For_Document_Outside_User_Start_Nodes()
        => await AssertStatusAsync(ReferencedByUrl(_inaccessibleDocument.Key), HttpStatusCode.Forbidden);

    [Test]
    public async Task ReferencedBy_Is_Allowed_For_Document_Inside_User_Start_Nodes()
        => await AssertStatusAsync(ReferencedByUrl(_accessibleDocument.Key), HttpStatusCode.OK);

    [Test]
    public async Task ReferencedDescendants_Is_Forbidden_For_Document_Outside_User_Start_Nodes()
        => await AssertStatusAsync(ReferencedDescendantsUrl(_inaccessibleDocument.Key), HttpStatusCode.Forbidden);

    [Test]
    public async Task ReferencedDescendants_Is_Allowed_For_Document_Inside_User_Start_Nodes()
        => await AssertStatusAsync(ReferencedDescendantsUrl(_accessibleDocument.Key), HttpStatusCode.OK);

    [Test]
    public async Task AreReferenced_Is_Forbidden_When_Any_Document_Is_Outside_User_Start_Nodes()
        => await AssertStatusAsync(AreReferencedUrl(_accessibleDocument.Key, _inaccessibleDocument.Key), HttpStatusCode.Forbidden);

    [Test]
    public async Task AreReferenced_Is_Allowed_For_Documents_Inside_User_Start_Nodes()
        => await AssertStatusAsync(AreReferencedUrl(_accessibleDocument.Key), HttpStatusCode.OK);

    private async Task AssertStatusAsync(string url, HttpStatusCode expected)
    {
        HttpResponseMessage response = await Client.GetAsync(url);

        Assert.AreEqual(expected, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private string ReferencedByUrl(Guid id) =>
        GetManagementApiUrl<ReferencedByDocumentController>(x => x.ReferencedBy2(CancellationToken.None, id, 0, 20));

    private string ReferencedDescendantsUrl(Guid id) =>
        GetManagementApiUrl<ReferencedDescendantsDocumentController>(x => x.ReferencedDescendants2(CancellationToken.None, id, 0, 20));

    private string AreReferencedUrl(params Guid[] ids) =>
        WithIds(GetManagementApiUrl<AreReferencedDocumentController>(x => x.GetPagedReferencedItems(CancellationToken.None, new HashSet<Guid>(), 0, 20)), ids);

    private async Task<IContent> CreateDocumentAsync(Guid contentTypeKey, Guid? parentKey)
    {
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentTypeKey,
            ParentKey = parentKey,
            Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
        };

        Attempt<ContentCreateResult, ContentEditingOperationStatus> attempt =
            await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(attempt.Success, $"Could not create the document: {attempt.Status}");

        return attempt.Result.Content!;
    }
}
