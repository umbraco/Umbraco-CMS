using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    protected IRelationService RelationService => GetRequiredService<IRelationService>();
    public static void ConfigureDisableDeleteWhenReferenced(IUmbracoBuilder builder)
    {
        builder.Services.Configure<ContentSettings>(config =>
            config.DisableDeleteWhenReferenced = true);
    }

    public void Relate(IContent child, IContent parent)
    {
        var relatedContentRelType = RelationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelatedDocumentAlias);

        var relation = RelationService.Relate(child.Id, parent.Id, relatedContentRelType);
        RelationService.Save(relation);
    }


    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisableDeleteWhenReferenced))]
    public async Task Cannot_Delete_Referenced_Content()
    {
        var moveAttempt = await ContentEditingService.MoveToRecycleBinAsync(Subpage.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveAttempt.Success);

        Relate(Subpage, Subpage2);
        var result = await ContentEditingService.DeleteFromRecycleBinAsync(Subpage.Key, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.CannotDeleteWhenReferenced, result.Status);

        // re-get and verify not deleted
        var subpage = await ContentEditingService.GetAsync(Subpage.Key);
        Assert.IsNotNull(subpage);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Delete_FromRecycleBin(bool variant)
    {
        var content = await (variant ? CreateCultureVariantContent() : CreateInvariantContent());
        await ContentEditingService.MoveToRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.DeleteFromRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        // re-get and verify deletion
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNull(content);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Delete_FromOutsideOfRecycleBin(bool variant)
    {
        var content = await (variant ? CreateCultureVariantContent() : CreateInvariantContent());

        var result = await ContentEditingService.DeleteAsync(content.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        // re-get and verify deletion
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNull(content);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing()
    {
        var result = await ContentEditingService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result.Status);
    }
}
