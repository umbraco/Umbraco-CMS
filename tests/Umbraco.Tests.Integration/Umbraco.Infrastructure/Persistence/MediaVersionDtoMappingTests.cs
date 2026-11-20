using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence;

/// <summary>
///     Verifies that the EF Core media version mapping reads the media version table as the existing persistence layer writes it.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaVersionDtoMappingTests : UmbracoIntegrationTest
{
    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    [Test]
    public async Task Media_Version_Table_Reads_Back_Through_EF_Core()
    {
        IMediaType imageType = MediaTypeBuilder.CreateImageMediaType("testImage");
        await MediaTypeService.CreateAsync(imageType, Constants.Security.SuperUserKey);
        IMediaType folderType = MediaTypeBuilder.CreateSimpleMediaType("testFolder", "Test Folder");
        await MediaTypeService.CreateAsync(folderType, Constants.Security.SuperUserKey);

        IMedia image = MediaBuilder.CreateMediaImage(imageType, Constants.System.Root);
        Assert.That(MediaService.Save(image).Success, Is.True);
        IMedia folder = MediaBuilder.CreateMediaFolder(folderType, Constants.System.Root);
        Assert.That(MediaService.Save(folder).Success, Is.True);

        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        using var scope = NewScopeProvider.CreateScope();
        List<MediaVersionDto> rows = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db => db.MediaVersions.ToListAsync());
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows.Single(row => row.Id == image.VersionId).Path, Is.EqualTo("/media/test-image.png"));
            Assert.That(rows.Single(row => row.Id == folder.VersionId).Path, Is.Null);
        });
    }
}
