using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class MediaPresentationFactoryFileExtensionTests
{
    [TestCase("/media/abc123/holiday-photo.jpg", "jpg")]
    [TestCase("/media/abc123/HOLIDAY-PHOTO.JPG", "jpg")]
    [TestCase("/media/abc123/archive.tar.gz", "gz")]
    public void Item_Response_Reads_The_Extension_From_The_File_Path(string mediaPath, string expected)
        => Assert.AreEqual(expected, CreateItemResponseModel(mediaPath).Extension);

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Item_Response_Has_No_Extension_When_The_Entity_Holds_No_File(string? mediaPath)
        => Assert.IsNull(CreateItemResponseModel(mediaPath).Extension);

    [Test]
    public void Item_Response_Has_No_Extension_When_The_File_Path_Has_None()
        => Assert.IsNull(CreateItemResponseModel("/media/abc123/README").Extension);

    [Test]
    public void Item_Response_Ignores_Dots_In_The_Item_Name()
    {
        // The name is editor-owned prose and is routinely renamed after upload; only the stored file decides.
        MediaItemResponseModel model = CreateItemResponseModel("/media/abc123/file.pdf", "Version 2.0 mockup");

        Assert.AreEqual("pdf", model.Extension);
    }

    private static MediaItemResponseModel CreateItemResponseModel(string? mediaPath, string? name = "Some media")
    {
        var entity = new Mock<IMediaEntitySlim>();
        entity.SetupGet(x => x.MediaPath).Returns(mediaPath);
        entity.SetupGet(x => x.Name).Returns(name);
        entity.SetupGet(x => x.Key).Returns(Guid.NewGuid());

        var mapper = new Mock<IUmbracoMapper>();
        mapper
            .Setup(x => x.Map<MediaTypeReferenceResponseModel>(It.IsAny<IMediaEntitySlim>()))
            .Returns(new MediaTypeReferenceResponseModel());

        var factory = new MediaPresentationFactory(mapper.Object, Mock.Of<IIdKeyMap>());

        return factory.CreateItemResponseModel(entity.Object);
    }
}
