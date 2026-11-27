using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Media.References;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Media.References;

/// <summary>
/// Asserts that the media reference endpoints only answer for media within the user's media start nodes.
/// </summary>
public class MediaReferencesStartNodeAccessTests : StartNodeAccessTestBase<ReferencedByMediaController>
{
    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMedia _accessibleMedia = null!;

    private IMedia _inaccessibleMedia = null!;

    protected override Expression<Func<ReferencedByMediaController, object>> MethodSelector { get; set; } = null!;

    [SetUp]
    public async Task SetUpMediaAndUser()
    {
        IMedia deniedRoot = await CreateFolderAsync(null);
        IMedia allowedRoot = await CreateFolderAsync(null);

        _inaccessibleMedia = await CreateFolderAsync(deniedRoot.Key);
        _accessibleMedia = await CreateFolderAsync(allowedRoot.Key);

        await AuthenticateRestrictedUserAsync([Constants.Applications.Media], startMediaId: allowedRoot.Id);
    }

    [Test]
    public async Task ReferencedBy_Is_Forbidden_For_Media_Outside_User_Start_Nodes()
        => await AssertStatusAsync(ReferencedByUrl(_inaccessibleMedia.Key), HttpStatusCode.Forbidden);

    [Test]
    public async Task ReferencedBy_Is_Allowed_For_Media_Inside_User_Start_Nodes()
        => await AssertStatusAsync(ReferencedByUrl(_accessibleMedia.Key), HttpStatusCode.OK);

    [Test]
    public async Task ReferencedDescendants_Is_Forbidden_For_Media_Outside_User_Start_Nodes()
        => await AssertStatusAsync(ReferencedDescendantsUrl(_inaccessibleMedia.Key), HttpStatusCode.Forbidden);

    [Test]
    public async Task ReferencedDescendants_Is_Allowed_For_Media_Inside_User_Start_Nodes()
        => await AssertStatusAsync(ReferencedDescendantsUrl(_accessibleMedia.Key), HttpStatusCode.OK);

    [Test]
    public async Task AreReferenced_Is_Forbidden_When_Any_Media_Is_Outside_User_Start_Nodes()
        => await AssertStatusAsync(AreReferencedUrl(_accessibleMedia.Key, _inaccessibleMedia.Key), HttpStatusCode.Forbidden);

    [Test]
    public async Task AreReferenced_Is_Allowed_For_Media_Inside_User_Start_Nodes()
        => await AssertStatusAsync(AreReferencedUrl(_accessibleMedia.Key), HttpStatusCode.OK);

    private async Task AssertStatusAsync(string url, HttpStatusCode expected)
    {
        HttpResponseMessage response = await Client.GetAsync(url);

        Assert.AreEqual(expected, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private string ReferencedByUrl(Guid id) =>
        GetManagementApiUrl<ReferencedByMediaController>(x => x.ReferencedBy(CancellationToken.None, id, 0, 20));

    private string ReferencedDescendantsUrl(Guid id) =>
        GetManagementApiUrl<ReferencedDescendantsMediaController>(x => x.ReferencedDescendants(CancellationToken.None, id, 0, 20));

    private string AreReferencedUrl(params Guid[] ids) =>
        WithIds(GetManagementApiUrl<AreReferencedMediaController>(x => x.GetPagedReferencedItems(CancellationToken.None, new HashSet<Guid>(), 0, 20)), ids);

    private async Task<IMedia> CreateFolderAsync(Guid? parentKey)
        => await MediaService.CreateAndSaveAsync(Guid.NewGuid().ToString(), parentKey, Constants.Conventions.MediaTypes.Folder, Constants.Security.SuperUserKey, CancellationToken.None);
}
