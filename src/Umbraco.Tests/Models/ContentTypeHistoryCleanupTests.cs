using NUnit.Framework;
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
    }
}
