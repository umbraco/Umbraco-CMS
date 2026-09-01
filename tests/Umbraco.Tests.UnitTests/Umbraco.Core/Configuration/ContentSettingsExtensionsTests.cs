// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration;

[TestFixture]
public class ContentSettingsExtensionsTests
{
    [TestCase("jpg")]
    [TestCase("JPG")]
    [TestCase("jpg ")]
    public void IsFileAllowedForUpload_Allows_File_In_Allow_List(string extension)
    {
        var contentSettings = new ContentSettings
        {
            AllowedUploadedFileExtensions = new[] { "jpg", "png" }.ToHashSet(),
        };

        Assert.IsTrue(contentSettings.IsFileAllowedForUpload(extension));
    }

    [TestCase("gif")]
    [TestCase("GIF")]
    [TestCase("gif ")]
    public void IsFileAllowedForUpload_Rejects_File_Not_In_Allow_List(string extension)
    {
        var contentSettings = new ContentSettings
        {
            AllowedUploadedFileExtensions = new[] { "jpg", "png" }.ToHashSet(),
        };

        Assert.IsFalse(contentSettings.IsFileAllowedForUpload(extension));
    }

    [TestCase("jpg")]
    [TestCase("JPG")]
    [TestCase("jpg ")]
    public void IsFileAllowedForUpload_Allows_File_Not_In_Disallow_List(string extension)
    {
        var contentSettings = new ContentSettings
        {
            DisallowedUploadedFileExtensions = new[] { "gif", "png" }.ToHashSet(),
        };

        Assert.IsTrue(contentSettings.IsFileAllowedForUpload(extension));
    }

    [TestCase("gif")]
    [TestCase("GIF")]
    [TestCase("gif ")]
    public void IsFileAllowedForUpload_Rejects_File_In_Disallow_List(string extension)
    {
        var contentSettings = new ContentSettings
        {
            DisallowedUploadedFileExtensions = new[] { "gif", "png" }.ToHashSet(),
        };

        Assert.IsFalse(contentSettings.IsFileAllowedForUpload(extension));
    }

    [Test]
    public void IsFileAllowedForUpload_Allows_File_In_Allow_List_Even_If_Also_In_Disallow_List()
    {
        var contentSettings = new ContentSettings
        {
            AllowedUploadedFileExtensions = new[] { "jpg", "png" }.ToHashSet(),
            DisallowedUploadedFileExtensions = new[] { "jpg", }.ToHashSet(),
        };

        Assert.IsTrue(contentSettings.IsFileAllowedForUpload("jpg"));
    }

    [TestCase("png")]
    [TestCase("jpg")]
    [TestCase("webp")]
    [TestCase("PNG")]
    [TestCase(" png ")]
    public void IsAllowedImageFileType_Allows_Default_Image_Type(string extension)
        => Assert.IsTrue(new ContentSettings().IsAllowedImageFileType(extension));

    [TestCase("exe")]
    [TestCase("")]
    [TestCase("if")] // "if" is a substring of "tiff" in the default image file types but not a valid extension itself.
    [TestCase(".png")] // The extension is expected without a leading period, so a dotted value is not a match.
    public void IsAllowedImageFileType_Rejects_Invalid_Extension(string extension)
        => Assert.IsFalse(new ContentSettings().IsAllowedImageFileType(extension));

    [TestCase("heic")]
    [TestCase("png")]
    public void IsAllowedImageFileType_Allows_Custom_Configured_Image_Type(string extension)
    {
        var contentSettings = new ContentSettings
        {
            Imaging = new ContentImagingSettings { ImageFileTypes = new HashSet<string> { "png", "heic" } },
        };

        Assert.IsTrue(contentSettings.IsAllowedImageFileType(extension));
    }

    [TestCase("jpg")]
    [TestCase("gif")]
    public void IsAllowedImageFileType_Rejects_Type_Not_In_Custom_Configured_Image_Types(string extension)
    {
        var contentSettings = new ContentSettings
        {
            Imaging = new ContentImagingSettings { ImageFileTypes = new HashSet<string> { "png", "heic" } },
        };

        Assert.IsFalse(contentSettings.IsAllowedImageFileType(extension));
    }

    [TestCase("png")]
    [TestCase("PNG")]
    public void IsAllowedImageFileType_Rejects_Disallowed_Extension(string extension)
    {
        var contentSettings = new ContentSettings
        {
            DisallowedUploadedFileExtensions = new HashSet<string> { "png" },
        };

        Assert.IsFalse(contentSettings.IsAllowedImageFileType(extension));
    }
}
