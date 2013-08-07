using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class ContentExtensionsTests
    {
        [Test]
        public void Should_Create_New_Version_When_Publish_Status_Changed()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.Published = true;
            Assert.IsTrue(content.ShouldCreateNewVersion(PublishedState.Published));
        }

        [Test]
        public void Should_Create_New_Version_When_Language_Changed()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.Language = "en-AU";
            Assert.IsTrue(content.ShouldCreateNewVersion(PublishedState.Unpublished));
        }

        [Test]
        public void Should_Create_New_Version_When_Any_Property_Value_Changed()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.Properties.First().Value = "hello world";
            Assert.IsTrue(content.ShouldCreateNewVersion(PublishedState.Unpublished));
        }

        [Test]
        public void Should_Not_Create_New_Version_When_Anything_Other_Than_Published_Language_Or_Property_Vals_Changed()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            Assert.IsFalse(content.ShouldCreateNewVersion(PublishedState.Unpublished));
        }

    }
}