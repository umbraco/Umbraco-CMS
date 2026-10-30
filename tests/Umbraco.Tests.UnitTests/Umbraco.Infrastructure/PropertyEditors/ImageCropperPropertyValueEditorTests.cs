using System.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.IO.MediaPathSchemes;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class ImageCropperPropertyValueEditorTests
{
    private static readonly Guid EditingItem = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherItem = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid PropertyType = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Test]
    public void Clearing_Value_Deletes_A_File_The_Property_Owns()
    {
        ImageCropperPropertyValueEditor editor = CreateEditor(out Mock<IFileSystem> fileSystem, out MediaFileManager manager);
        var ownedPath = manager.GetMediaPath("image.jpg", EditingItem, PropertyType);

        editor.FromEditor(
            new ContentPropertyData("{\"src\":\"\"}", null) { ContentKey = EditingItem, PropertyTypeKey = PropertyType },
            $"{{\"src\":\"{ownedPath}\"}}");

        fileSystem.Verify(x => x.DeleteFile(ownedPath), Times.Once);
    }

    [Test]
    public void Clearing_Value_Does_Not_Delete_A_File_Belonging_To_Another_Item()
    {
        ImageCropperPropertyValueEditor editor = CreateEditor(out Mock<IFileSystem> fileSystem, out MediaFileManager manager);

        // A tampered stored value referencing another item's file - clearing this property must not delete it.
        var foreignPath = manager.GetMediaPath("secret.txt", OtherItem, PropertyType);

        editor.FromEditor(
            new ContentPropertyData("{\"src\":\"\"}", null) { ContentKey = EditingItem, PropertyTypeKey = PropertyType },
            $"{{\"src\":\"{foreignPath}\"}}");

        fileSystem.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Posted_Source_That_Is_Not_A_Valid_File_Location_Is_Not_Stored()
    {
        ImageCropperPropertyValueEditor editor = CreateEditor(out _, out _);
        const string currentSource = "/media/abc123/image.jpg";
        const string tamperedSource = "@attacker.example.com/media/x.jpg\" ><script src=https://attacker.example.com/x.js></script>.svg";

        var result = editor.FromEditor(
            new ContentPropertyData(Serialize(new ImageCropperValue { Src = tamperedSource, FocalPoint = new ImageCropperValue.ImageCropperFocalPoint { Left = 0.1m, Top = 0.2m } }), null)
            {
                ContentKey = EditingItem,
                PropertyTypeKey = PropertyType,
            },
            Serialize(new ImageCropperValue { Src = currentSource }));

        ImageCropperValue? stored = Deserialize(result);
        Assert.AreEqual(currentSource, stored?.Src);
        Assert.AreEqual(0.1m, stored?.FocalPoint?.Left, "Other parts of the value are still stored.");
    }

    [Test]
    public void Posted_Source_That_Is_A_Valid_File_Location_Is_Stored()
    {
        ImageCropperPropertyValueEditor editor = CreateEditor(out _, out _);
        const string postedSource = "/media/def456/image.jpg";

        var result = editor.FromEditor(
            new ContentPropertyData(Serialize(new ImageCropperValue { Src = postedSource }), null) { ContentKey = EditingItem, PropertyTypeKey = PropertyType },
            null);

        Assert.AreEqual(postedSource, Deserialize(result)?.Src);
    }

    [Test]
    public void Posted_Source_Matching_The_Stored_Value_Is_Stored_Even_When_Not_Well_Formed()
    {
        ImageCropperPropertyValueEditor editor = CreateEditor(out _, out _);
        const string legacySource = "media/1234/legacy image.jpg";

        var result = editor.FromEditor(
            new ContentPropertyData(Serialize(new ImageCropperValue { Src = legacySource }), null) { ContentKey = EditingItem, PropertyTypeKey = PropertyType },
            Serialize(new ImageCropperValue { Src = legacySource }));

        Assert.AreEqual(legacySource, Deserialize(result)?.Src);
    }

    private static string Serialize(ImageCropperValue value)
        => new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()).Serialize(value);

    private static ImageCropperValue? Deserialize(object? value)
        => new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()).Deserialize<ImageCropperValue>((string)value!);

    private static ImageCropperPropertyValueEditor CreateEditor(out Mock<IFileSystem> fileSystem, out MediaFileManager manager)
    {
        fileSystem = new Mock<IFileSystem>();
        fileSystem
            .Setup(x => x.GetRelativePath(It.IsAny<string>()))
            .Returns<string>(path => path.Replace('\\', '/').TrimStart('/'));

        var shortStringHelper = new Mock<IShortStringHelper>();
        shortStringHelper
            .Setup(x => x.CleanStringForSafeFileName(It.IsAny<string>()))
            .Returns<string>(s => s);

        manager = new MediaFileManager(
            fileSystem.Object,
            new CombinedGuidsMediaPathScheme(),
            NullLogger<MediaFileManager>.Instance,
            shortStringHelper.Object,
            Mock.Of<IServiceProvider>(),
            new Lazy<ICoreScopeProvider>(() => Mock.Of<ICoreScopeProvider>()));

        var contentSettings = new Mock<IOptionsMonitor<ContentSettings>>();
        contentSettings.Setup(x => x.CurrentValue).Returns(new ContentSettings());

        var scopeProvider = new Mock<IScopeProvider>();
        scopeProvider
            .Setup(x => x.CreateScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<IScope>());

        return new ImageCropperPropertyValueEditor(
            new DataEditorAttribute("Umbraco.ImageCropper"),
            NullLogger<ImageCropperPropertyValueEditor>.Instance,
            manager,
            shortStringHelper.Object,
            contentSettings.Object,
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            Mock.Of<ITemporaryFileService>(),
            scopeProvider.Object,
            Mock.Of<IFileStreamSecurityValidator>(),
            Mock.Of<IDataTypeConfigurationCache>());
    }
}
