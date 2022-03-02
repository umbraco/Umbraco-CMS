using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class ContentTypeHistoryCleanupTests : UmbracoTestBase
    {
        [Test]
        public void Changing_Keep_all_Makes_ContentType_Dirty()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();

            Assert.IsFalse(contentType.IsDirty());

            var newValue = 2;
            contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = newValue;
            Assert.IsTrue(contentType.IsDirty());
            Assert.AreEqual(newValue, contentType.HistoryCleanup.KeepAllVersionsNewerThanDays);
        }

        [Test]
        public void Changing_Keep_latest_Makes_ContentType_Dirty()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();

            Assert.IsFalse(contentType.IsDirty());

            var newValue = 2;
            contentType.HistoryCleanup.KeepLatestVersionPerDayForDays = newValue;
            Assert.IsTrue(contentType.IsDirty());
            Assert.AreEqual(newValue, contentType.HistoryCleanup.KeepLatestVersionPerDayForDays);
        }

        [Test]
        public void Changing_Prevent_Cleanup_Makes_ContentType_Dirty()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();

            Assert.IsFalse(contentType.IsDirty());

            var newValue = true;
            contentType.HistoryCleanup.PreventCleanup = newValue;
            Assert.IsTrue(contentType.IsDirty());
            Assert.AreEqual(newValue, contentType.HistoryCleanup.PreventCleanup);
        }

        [Test]
        public void Replacing_History_Cleanup_Registers_As_Dirty()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            Assert.IsFalse(contentType.IsDirty());

            contentType.HistoryCleanup = new HistoryCleanup();

            Assert.IsTrue(contentType.IsDirty());
            Assert.IsTrue(contentType.IsPropertyDirty(nameof(contentType.HistoryCleanup)));
        }

        [Test]
        public void Replacing_History_Cleanup_Removes_Old_Dirty_History_Properties()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();

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

        [Test]
        public void Old_History_Cleanup_Reference_Doesnt_Make_Content_Type_Dirty()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            var oldHistoryCleanup = contentType.HistoryCleanup;

            contentType.HistoryCleanup = new HistoryCleanup();
            contentType.ResetDirtyProperties();
            contentType.ResetWereDirtyProperties();

            oldHistoryCleanup.KeepAllVersionsNewerThanDays = 2;

            Assert.IsFalse(contentType.IsDirty());
            Assert.IsFalse(contentType.WasDirty());
        }
    }
}
