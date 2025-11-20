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
}
