// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration;

/// <summary>
/// Unit tests for the ContentSettingsExtensions class.
/// </summary>
[TestFixture]
public class ContentSettingsExtensionsTests
{
    /// <summary>
    /// Tests that a file with an extension in the allowed list is permitted for upload.
    /// </summary>
    /// <param name="extension">The file extension to test.</param>
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

    /// <summary>
    /// Verifies that <see cref="ContentSettingsExtensions.IsFileAllowedForUpload"/> returns <c>false</c> for file extensions not present in the allowed list.
    /// Specifically, tests that extensions such as "gif", "GIF", and "gif " are rejected when only "jpg" and "png" are allowed.
    /// </summary>
    /// <param name="extension">The file extension to test for upload permission.</param>
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

    /// <summary>
    /// Tests that a file with the specified extension is allowed for upload when it is not in the disallow list.
    /// </summary>
    /// <param name="extension">The file extension to test.</param>
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

    /// <summary>
    /// Tests that a file with the specified extension is rejected when it is in the disallow list.
    /// </summary>
    /// <param name="extension">The file extension to test.</param>
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

    /// <summary>
    /// Tests that a file extension is allowed for upload if it is in the allow list,
    /// even if it is also present in the disallow list.
    /// </summary>
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
