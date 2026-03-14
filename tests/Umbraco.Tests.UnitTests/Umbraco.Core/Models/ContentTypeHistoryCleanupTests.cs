using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Contains unit tests that verify the cleanup functionality for content type history in the Umbraco CMS.
/// </summary>
[TestFixture]
public class ContentTypeHistoryCleanupTests
{
    /// <summary>
    /// Tests that changing the KeepAllVersionsNewerThanDays property
    /// marks the content type as dirty.
    /// </summary>
    [Test]
    public void Changing_Keep_all_Makes_ContentType_Dirty()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();

        Assert.IsFalse(contentType.IsDirty());

        var newValue = 2;
        contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = newValue;
        Assert.IsTrue(contentType.IsDirty());
        Assert.AreEqual(newValue, contentType.HistoryCleanup.KeepAllVersionsNewerThanDays);
    }

    /// <summary>
    /// Tests that changing the KeepLatestVersionPerDayForDays property
    /// marks the content type as dirty.
    /// </summary>
    [Test]
    public void Changing_Keep_latest_Makes_ContentType_Dirty()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();

        Assert.IsFalse(contentType.IsDirty());

        var newValue = 2;
        contentType.HistoryCleanup.KeepLatestVersionPerDayForDays = newValue;
        Assert.IsTrue(contentType.IsDirty());
        Assert.AreEqual(newValue, contentType.HistoryCleanup.KeepLatestVersionPerDayForDays);
    }

    /// <summary>
    /// Tests that changing the PreventCleanup property marks the content type as dirty.
    /// </summary>
    [Test]
    public void Changing_Prevent_Cleanup_Makes_ContentType_Dirty()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();

        Assert.IsFalse(contentType.IsDirty());

        var newValue = true;
        contentType.HistoryCleanup.PreventCleanup = newValue;
        Assert.IsTrue(contentType.IsDirty());
        Assert.AreEqual(newValue, contentType.HistoryCleanup.PreventCleanup);
    }

    /// <summary>
    /// Tests that replacing the HistoryCleanup property registers the content type as dirty.
    /// </summary>
    [Test]
    public void Replacing_History_Cleanup_Registers_As_Dirty()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        Assert.IsFalse(contentType.IsDirty());

        contentType.HistoryCleanup = new HistoryCleanup();

        Assert.IsTrue(contentType.IsDirty());
        Assert.IsTrue(contentType.IsPropertyDirty(nameof(contentType.HistoryCleanup)));
    }

    /// <summary>
    /// Tests that replacing the HistoryCleanup property removes old dirty history properties and marks the new HistoryCleanup as dirty.
    /// </summary>
    [Test]
    public void Replacing_History_Cleanup_Removes_Old_Dirty_History_Properties()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();

        contentType.Alias = "NewValue";
        contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 2;

        contentType.PropertyChanged += (sender, args) =>
        {
            // Ensure that property changed is only invoked for history cleanup
            Assert.AreEqual(nameof(contentType.HistoryCleanup), args.PropertyName);
        };

        // Since we're replacing the entire HistoryCleanup the changed property is no longer dirty, the entire HistoryCleanup is
        contentType.HistoryCleanup = new HistoryCleanup();

        Assert.Multiple(() =>
        {
            Assert.IsTrue(contentType.IsDirty());
            Assert.IsFalse(contentType.WasDirty());
            Assert.AreEqual(2, contentType.GetDirtyProperties().Count());
            Assert.IsTrue(contentType.IsPropertyDirty(nameof(contentType.HistoryCleanup)));
            Assert.IsTrue(contentType.IsPropertyDirty(nameof(contentType.Alias)));
        });
    }

    /// <summary>
    /// Tests that modifying the old history cleanup reference does not mark the content type as dirty.
    /// </summary>
    [Test]
    public void Old_History_Cleanup_Reference_Doesnt_Make_Content_Type_Dirty()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        var oldHistoryCleanup = contentType.HistoryCleanup;

        contentType.HistoryCleanup = new HistoryCleanup();
        contentType.ResetDirtyProperties();
        contentType.ResetWereDirtyProperties();

        oldHistoryCleanup.KeepAllVersionsNewerThanDays = 2;

        Assert.IsFalse(contentType.IsDirty());
        Assert.IsFalse(contentType.WasDirty());
    }
}
