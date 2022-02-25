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
        [TestCase(true)]
        [TestCase(false)]
        public void Reset_DirtyProperties_Also_Resets_History_Cleanup(bool useOverload)
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 2;
            Assert.IsTrue(contentType.IsDirty());

            if (useOverload)
            {
                contentType.ResetDirtyProperties(false);
            }
            else
            {
                contentType.ResetDirtyProperties();
            }

            Assert.IsFalse(contentType.IsDirty());
        }

        [Test]
        public void Get_Dirty_Properties_Includes_History_Cleanup()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            Assert.IsEmpty(contentType.GetDirtyProperties());

            contentType.Alias = "SomethingNew";
            contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 2;

            var dirty = contentType.GetDirtyProperties().ToList();
            Assert.AreEqual(2, dirty.Count);
            Assert.Contains(nameof(contentType.Alias), dirty);
            Assert.Contains(PrefixHistoryCleanup(nameof(contentType.HistoryCleanup.KeepAllVersionsNewerThanDays)), dirty);
        }

        [Test]
        public void Get_Dirty_Properties_Works_If_Only_History_Cleanup_Is_Dirty()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            Assert.IsEmpty(contentType.GetDirtyProperties());

            contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 2;

            var dirty = contentType.GetDirtyProperties().ToList();
            Assert.AreEqual(1, dirty.Count);
            Assert.Contains(PrefixHistoryCleanup(nameof(contentType.HistoryCleanup.KeepAllVersionsNewerThanDays)), dirty);
        }

        [Test]
        public void Is_Property_Dirty_Includes_History_Cleanup()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();

            contentType.Alias = "SomethingNew";
            contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 2;

            Assert.IsTrue(contentType.IsPropertyDirty(nameof(contentType.Alias)));
            Assert.IsTrue(contentType.IsPropertyDirty(PrefixHistoryCleanup(nameof(contentType.HistoryCleanup.KeepAllVersionsNewerThanDays))));
        }

        [Test]
        public void Was_Dirty_Includes_History_Cleanup()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();

            contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 2;
            contentType.ResetDirtyProperties();

            Assert.IsTrue(contentType.WasDirty());
        }

        [Test]
        public void Was_Dirty_Property_Includes_History_Cleanup()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();

            contentType.Alias = "SomethingNew";
            contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 2;
            contentType.ResetDirtyProperties();

            Assert.IsTrue(contentType.WasPropertyDirty(nameof(contentType.Alias)));
            Assert.IsTrue(contentType.WasPropertyDirty(PrefixHistoryCleanup(nameof(contentType.HistoryCleanup.KeepAllVersionsNewerThanDays))));
        }

        [Test]
        public void Get_Were_Dirty_Properties_Includes_History_Cleanup()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();

            contentType.Alias = "SomethingNew";
            contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 2;
            contentType.ResetDirtyProperties();

            var wereDirty = contentType.GetWereDirtyProperties().ToList();
            Assert.AreEqual(2, wereDirty.Count);
            Assert.Contains(nameof(contentType.Alias), wereDirty);
            Assert.Contains(PrefixHistoryCleanup(nameof(contentType.HistoryCleanup.KeepAllVersionsNewerThanDays)), wereDirty);
        }

        [Test]
        public void Get_Were_Dirty_Works_If_Only_History_Cleanup_Is_Dirty()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();

            contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 2;
            contentType.ResetDirtyProperties();

            var wereDirty = contentType.GetWereDirtyProperties().ToList();
            Assert.AreEqual(1, contentType.GetWereDirtyProperties().Count());
            Assert.Contains(PrefixHistoryCleanup(nameof(contentType.HistoryCleanup.KeepAllVersionsNewerThanDays)), wereDirty);
        }

        [Test]
        public void Reset_Were_Dirty_Properties_Includes_History_Cleanup()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();

            contentType.Alias = "SomethingNew";
            contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 2;
            contentType.ResetDirtyProperties();
            Assert.AreEqual(2, contentType.GetWereDirtyProperties().Count());

            contentType.ResetWereDirtyProperties();
            Assert.IsEmpty(contentType.GetWereDirtyProperties());
        }

        [Test]
        public void Change_Tracking_Includes_History_Cleanup()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            contentType.EnableChangeTracking();

            var propertyChangeHasFired = false;
            contentType.PropertyChanged += (sender, args) =>
            {
                Assert.AreEqual(PrefixHistoryCleanup(nameof(contentType.HistoryCleanup.KeepAllVersionsNewerThanDays)), args.PropertyName);
                propertyChangeHasFired = true;
            };

            contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 2;
            Assert.IsTrue(propertyChangeHasFired);
        }

        [Test]
        public void Disable_Change_Tracking_Includes_History_Cleanup()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            contentType.DisableChangeTracking();

            var propertyChangeHasFired = false;
            contentType.PropertyChanged += (sender, args) =>
            {
                propertyChangeHasFired = true;
            };

            contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 2;
            Assert.IsFalse(propertyChangeHasFired);
            Assert.IsFalse(contentType.IsDirty());
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
                Assert.IsFalse(contentType.IsPropertyDirty(PrefixHistoryCleanup(nameof(contentType.HistoryCleanup.KeepAllVersionsNewerThanDays))));
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

        private static string PrefixHistoryCleanup(string propertyName) =>
            $"{nameof(ContentType.HistoryCleanup)}.{propertyName}";
    }
}
