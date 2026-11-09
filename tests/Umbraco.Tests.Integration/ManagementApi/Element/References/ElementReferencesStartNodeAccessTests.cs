using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.References;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.References;

/// <summary>
/// Asserts that the element reference endpoints only answer for elements and folders within the user's element start nodes.
/// </summary>
public class ElementReferencesStartNodeAccessTests : StartNodeAccessTestBase<ReferencedByElementController>
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _accessibleFolderKey;

    private Guid _inaccessibleFolderKey;

    private Guid _accessibleElementKey;

    private Guid _inaccessibleElementKey;

    protected override Expression<Func<ReferencedByElementController, object>> MethodSelector { get; set; } = null!;

    [SetUp]
    public async Task SetUpElementsAndUser()
    {
        IContentType elementType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString())
            .WithName(Guid.NewGuid().ToString())
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        EntityContainer deniedFolder = await CreateFolderAsync();
        EntityContainer allowedFolder = await CreateFolderAsync();
        _inaccessibleFolderKey = deniedFolder.Key;
        _accessibleFolderKey = allowedFolder.Key;

        _inaccessibleElementKey = await CreateElementAsync(elementType.Key, deniedFolder.Key);
        _accessibleElementKey = await CreateElementAsync(elementType.Key, allowedFolder.Key);

        await AuthenticateRestrictedUserAsync([Constants.Applications.Library], startElementId: allowedFolder.Id);
    }

    [Test]
    public async Task ReferencedBy_Is_Forbidden_For_Element_Outside_User_Start_Nodes()
        => await AssertStatusAsync(ReferencedByUrl(_inaccessibleElementKey), HttpStatusCode.Forbidden);

    [Test]
    public async Task ReferencedBy_Is_Allowed_For_Element_Inside_User_Start_Nodes()
        => await AssertStatusAsync(ReferencedByUrl(_accessibleElementKey), HttpStatusCode.OK);

    [Test]
    public async Task ReferencedDescendants_Is_Forbidden_For_Folder_Outside_User_Start_Nodes()
        => await AssertStatusAsync(ReferencedDescendantsUrl(_inaccessibleFolderKey), HttpStatusCode.Forbidden);

    [Test]
    public async Task ReferencedDescendants_Is_Allowed_For_Folder_Inside_User_Start_Nodes()
        => await AssertStatusAsync(ReferencedDescendantsUrl(_accessibleFolderKey), HttpStatusCode.OK);

    [Test]
    public async Task AreReferenced_Is_Forbidden_When_Any_Element_Is_Outside_User_Start_Nodes()
        => await AssertStatusAsync(AreReferencedUrl(_accessibleElementKey, _inaccessibleElementKey), HttpStatusCode.Forbidden);

    [Test]
    public async Task AreReferenced_Is_Allowed_For_Elements_Inside_User_Start_Nodes()
        => await AssertStatusAsync(AreReferencedUrl(_accessibleElementKey), HttpStatusCode.OK);

    private async Task AssertStatusAsync(string url, HttpStatusCode expected)
    {
        HttpResponseMessage response = await Client.GetAsync(url);

        Assert.AreEqual(expected, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private string ReferencedByUrl(Guid id) =>
        GetManagementApiUrl<ReferencedByElementController>(x => x.ReferencedBy(CancellationToken.None, id, 0, 20));

    private string ReferencedDescendantsUrl(Guid id) =>
        GetManagementApiUrl<ReferencedDescendantsElementFolderController>(x => x.ReferencedDescendants(CancellationToken.None, id, 0, 20));

    private string AreReferencedUrl(params Guid[] ids) =>
        WithIds(GetManagementApiUrl<AreReferencedElementController>(x => x.GetPagedReferencedItems(CancellationToken.None, new HashSet<Guid>(), 0, 20)), ids);

    private async Task<EntityContainer> CreateFolderAsync()
    {
        var attempt = await ElementContainerService.CreateAsync(null, Guid.NewGuid().ToString(), null, Constants.Security.SuperUserKey);
        Assert.IsTrue(attempt.Success, $"Could not create the folder: {attempt.Status}");

        return attempt.Result!;
    }

    private async Task<Guid> CreateElementAsync(Guid elementTypeKey, Guid parentKey)
    {
        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementTypeKey,
            ParentKey = parentKey,
            Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
        };

        var attempt = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(attempt.Success, $"Could not create the element: {attempt.Status}");

        return attempt.Result.Content!.Key;
    }
}
