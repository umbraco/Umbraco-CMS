using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.IO.MediaPathSchemes;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.IO;

[TestFixture]
public class MediaFileManagerTests
{
    private static readonly Guid _itemA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid _itemB = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid _propertyType = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static MediaFileManager CreateManager(IMediaPathScheme scheme)
    {
        var fileSystem = new Mock<IFileSystem>();

        // For a filesystem-relative path the real implementation returns it unchanged (bar a leading slash);
        // for a rooted media URL it strips the "/media/" prefix. Model both here.
        fileSystem
            .Setup(x => x.GetRelativePath(It.IsAny<string>()))
            .Returns<string>(path =>
            {
                var normalized = path.Replace('\\', '/');
                const string mediaRoot = "/media/";
                if (normalized.StartsWith(mediaRoot, StringComparison.OrdinalIgnoreCase))
                {
                    normalized = normalized[mediaRoot.Length..];
                }

                return normalized.TrimStart('/');
            });

        var shortStringHelper = new Mock<IShortStringHelper>();
        shortStringHelper
            .Setup(x => x.CleanStringForSafeFileName(It.IsAny<string>()))
            .Returns<string>(s => s);

        return new MediaFileManager(
            fileSystem.Object,
            scheme,
            NullLogger<MediaFileManager>.Instance,
            shortStringHelper.Object,
            Mock.Of<IServiceProvider>(),
            new Lazy<ICoreScopeProvider>(() => Mock.Of<ICoreScopeProvider>()));
    }

    [Test]
    public void IsFileOwnedBy_Owns_Its_Own_Canonical_Path()
    {
        MediaFileManager manager = CreateManager(new CombinedGuidsMediaPathScheme());
        var ownPath = manager.GetMediaPath("file.txt", _itemA, _propertyType);

        Assert.IsTrue(manager.IsFileOwnedBy(ownPath, _itemA, _propertyType));
    }

    [Test]
    public void IsFileOwnedBy_Does_Not_Own_A_Path_Belonging_To_Another_Item()
    {
        MediaFileManager manager = CreateManager(new CombinedGuidsMediaPathScheme());

        // The path was minted for item B; item A must not be considered its owner even for the same filename.
        var otherItemsPath = manager.GetMediaPath("secret.txt", _itemB, _propertyType);

        Assert.IsFalse(manager.IsFileOwnedBy(otherItemsPath, _itemA, _propertyType));
    }

    [Test]
    public void IsFileOwnedBy_Does_Not_Own_Its_Path_Under_A_Different_Property_Type()
    {
        MediaFileManager manager = CreateManager(new CombinedGuidsMediaPathScheme());
        var ownPath = manager.GetMediaPath("file.txt", _itemA, _propertyType);
        var otherPropertyType = Guid.Parse("44444444-4444-4444-4444-444444444444");

        Assert.IsFalse(manager.IsFileOwnedBy(ownPath, _itemA, otherPropertyType));
    }

    [Test]
    public void IsFileOwnedBy_Matches_When_Path_Is_Supplied_As_A_Media_Url()
    {
        MediaFileManager manager = CreateManager(new CombinedGuidsMediaPathScheme());
        var ownPath = manager.GetMediaPath("file.txt", _itemA, _propertyType);
        var asUrl = "/media/" + ownPath;

        Assert.IsTrue(manager.IsFileOwnedBy(asUrl, _itemA, _propertyType));
    }

    [Test]
    public void IsFileOwnedBy_Ownership_Holds_Across_Path_Schemes()
    {
        MediaFileManager manager = CreateManager(new TwoGuidsMediaPathScheme());
        var ownPath = manager.GetMediaPath("file.txt", _itemA, _propertyType);
        var otherItemsPath = manager.GetMediaPath("file.txt", _itemB, _propertyType);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(manager.IsFileOwnedBy(ownPath, _itemA, _propertyType));
            Assert.IsFalse(manager.IsFileOwnedBy(otherItemsPath, _itemA, _propertyType));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void IsFileOwnedBy_Rejects_Empty_Input(string? path)
    {
        MediaFileManager manager = CreateManager(new CombinedGuidsMediaPathScheme());

        Assert.IsFalse(manager.IsFileOwnedBy(path, _itemA, _propertyType));
    }

    [Test]
    public void IsFilePathOwnedBy_DefaultImplementation_Owns_Its_Own_Path()
    {
        IMediaPathScheme scheme = new CombinedGuidsMediaPathScheme();
        MediaFileManager manager = CreateManager(scheme);
        var ownPath = manager.GetMediaPath("file.txt", _itemA, _propertyType);

        Assert.IsTrue(scheme.IsFilePathOwnedBy(manager, ownPath, _itemA, _propertyType));
    }

    [Test]
    public void IsFilePathOwnedBy_DefaultImplementation_Rejects_A_Path_Belonging_To_Another_Item()
    {
        IMediaPathScheme scheme = new CombinedGuidsMediaPathScheme();
        MediaFileManager manager = CreateManager(scheme);
        var foreignPath = manager.GetMediaPath("secret.txt", _itemB, _propertyType);

        Assert.IsFalse(scheme.IsFilePathOwnedBy(manager, foreignPath, _itemA, _propertyType));
    }

    [Test]
    public void IsFilePathOwnedBy_DefaultImplementation_Rejects_A_Path_Without_A_Filename()
    {
        IMediaPathScheme scheme = new CombinedGuidsMediaPathScheme();
        MediaFileManager manager = CreateManager(scheme);

        Assert.IsFalse(scheme.IsFilePathOwnedBy(manager, "folder/", _itemA, _propertyType));
    }

    [Test]
    public void IsFileOwnedBy_Delegates_To_The_Path_Scheme()
    {
        // A scheme can answer authoritatively; the manager must honour its decision rather than recomputing itself.
        MediaFileManager manager = CreateManager(new OwnershipOverridingMediaPathScheme(ownsEverything: false));
        var ownPath = manager.GetMediaPath("file.txt", _itemA, _propertyType);

        Assert.IsFalse(manager.IsFileOwnedBy(ownPath, _itemA, _propertyType));
    }

    private sealed class OwnershipOverridingMediaPathScheme : IMediaPathScheme
    {
        private readonly bool _ownsEverything;

        public OwnershipOverridingMediaPathScheme(bool ownsEverything) => _ownsEverything = ownsEverything;

        public string GetFilePath(MediaFileManager fileManager, Guid itemGuid, Guid propertyGuid, string filename)
            => new CombinedGuidsMediaPathScheme().GetFilePath(fileManager, itemGuid, propertyGuid, filename);

        public string? GetDeleteDirectory(MediaFileManager fileManager, string filepath) => null;

        public bool IsFilePathOwnedBy(MediaFileManager fileManager, string filepath, Guid itemGuid, Guid propertyGuid)
            => _ownsEverything;
    }
}
