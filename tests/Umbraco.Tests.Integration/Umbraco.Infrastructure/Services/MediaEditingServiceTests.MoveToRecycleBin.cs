using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaEditingServiceMoveToRecycleBinTests : UmbracoIntegrationTest
{
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    private IRelationService RelationService => GetRequiredService<IRelationService>();

    public static void ConfigureDisableDeleteWhenReferencedTrue(IUmbracoBuilder builder)
        => builder.Services.Configure<ContentSettings>(config =>
            config.DisableDeleteWhenReferenced = true);

    private async Task<IMedia> CreateFolderMediaAsync(string name)
    {
        var folderMediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.Folder);
        var createModel = new MediaCreateModel
        {
            ContentTypeKey = folderMediaType!.Key,
            ParentKey = Constants.System.RootKey,
            Key = Guid.NewGuid(),
            Variants = [new VariantModel { Name = name }],
        };

        var result = await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    private void Relate(IMedia parent, IMedia child)
    {
        var relationType = RelationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelatedMediaAlias);
        var relation = RelationService.Relate(parent.Id, child.Id, relationType);
        RelationService.Save(relation);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisableDeleteWhenReferencedTrue))]
    public async Task Cannot_Move_To_Recycle_Bin_When_Media_Is_Referenced_And_DisableDeleteWhenReferenced_Is_True()
    {
        // Setup a relation where the media being trashed is the child (i.e. another media item references it).
        var referencer = await CreateFolderMediaAsync("Referencer");
        var mediaToTrash = await CreateFolderMediaAsync("Media To Trash");
        Relate(referencer, mediaToTrash);

        var moveAttempt = await MediaEditingService.MoveToRecycleBinAsync(mediaToTrash.Key, Constants.Security.SuperUserKey);
        Assert.IsFalse(moveAttempt.Success);
        Assert.AreEqual(ContentEditingOperationStatus.CannotMoveToRecycleBinWhenReferenced, moveAttempt.Status);

        // Verify the item was not moved.
        var media = await MediaEditingService.GetAsync(mediaToTrash.Key);
        Assert.IsNotNull(media);
        Assert.IsFalse(media.Trashed);
    }
}
