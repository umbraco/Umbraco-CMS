using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class ContentTypeHistoryCleanupTests
{
    [Test]
    public void Changing_Keep_all_Makes_ContentType_Dirty()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();

        Assert.That(contentType.IsDirty(), Is.False);

        var newValue = 2;
        contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = newValue;
        Assert.That(contentType.IsDirty(), Is.True);
        Assert.That(contentType.HistoryCleanup.KeepAllVersionsNewerThanDays, Is.EqualTo(newValue));
    }

    [Test]
    public void Changing_Keep_latest_Makes_ContentType_Dirty()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();

        Assert.That(contentType.IsDirty(), Is.False);

        var newValue = 2;
        contentType.HistoryCleanup.KeepLatestVersionPerDayForDays = newValue;
        Assert.That(contentType.IsDirty(), Is.True);
        Assert.That(contentType.HistoryCleanup.KeepLatestVersionPerDayForDays, Is.EqualTo(newValue));
    }

    [Test]
    public void Changing_Prevent_Cleanup_Makes_ContentType_Dirty()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();

        Assert.That(contentType.IsDirty(), Is.False);

        var newValue = true;
        contentType.HistoryCleanup.PreventCleanup = newValue;
        Assert.That(contentType.IsDirty(), Is.True);
        Assert.That(contentType.HistoryCleanup.PreventCleanup, Is.EqualTo(newValue));
    }

    [Test]
    public void Replacing_History_Cleanup_Registers_As_Dirty()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        Assert.That(contentType.IsDirty(), Is.False);

        contentType.HistoryCleanup = new HistoryCleanup();

        Assert.That(contentType.IsDirty(), Is.True);
        Assert.That(contentType.IsPropertyDirty(nameof(contentType.HistoryCleanup)), Is.True);
    }

    [Test]
    public void Replacing_History_Cleanup_Removes_Old_Dirty_History_Properties()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();

        contentType.Alias = "NewValue";
        contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 2;

        contentType.PropertyChanged += (sender, args) =>
        {
            // Ensure that property changed is only invoked for history cleanup
            Assert.That(args.PropertyName, Is.EqualTo(nameof(contentType.HistoryCleanup)));
        };

        // Since we're replacing the entire HistoryCleanup the changed property is no longer dirty, the entire HistoryCleanup is
        contentType.HistoryCleanup = new HistoryCleanup();

        Assert.Multiple(() =>
        {
            Assert.That(contentType.IsDirty(), Is.True);
            Assert.That(contentType.WasDirty(), Is.False);
            Assert.That(contentType.GetDirtyProperties().Count(), Is.EqualTo(2));
            Assert.That(contentType.IsPropertyDirty(nameof(contentType.HistoryCleanup)), Is.True);
            Assert.That(contentType.IsPropertyDirty(nameof(contentType.Alias)), Is.True);
        });
    }

    [Test]
    public void Old_History_Cleanup_Reference_Doesnt_Make_Content_Type_Dirty()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        var oldHistoryCleanup = contentType.HistoryCleanup;

        contentType.HistoryCleanup = new HistoryCleanup();
        contentType.ResetDirtyProperties();
        contentType.ResetWereDirtyProperties();

        oldHistoryCleanup.KeepAllVersionsNewerThanDays = 2;

        Assert.That(contentType.IsDirty(), Is.False);
        Assert.That(contentType.WasDirty(), Is.False);
    }
}
