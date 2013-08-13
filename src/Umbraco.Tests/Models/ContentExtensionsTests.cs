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
        public void Should_Create_New_Version_When_Publishing()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);
            
            content.ChangePublishedState(PublishedState.Published);
            Assert.IsTrue(content.ShouldCreateNewVersion());
        }

        [Test]
        public void Should_Create_New_Version_When_Saving_After_Publishing()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);
            content.ChangePublishedState(PublishedState.Published);
            content.ResetDirtyProperties(false);

            content.ChangePublishedState(PublishedState.Saved);
            Assert.IsTrue(content.ShouldCreateNewVersion());
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
        public void Should_Create_New_Version_When_Any_Property_Value_Changed_And_Its_Already_Published()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.Properties.First().Value = "hello world";
            Assert.IsTrue(content.ShouldCreateNewVersion(PublishedState.Published));
        }
        
        [Test]
        public void Should_Not_Create_New_Version_When_Published_Status_Not_Changed()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            Assert.IsFalse(content.ShouldCreateNewVersion(PublishedState.Unpublished));
        }

        [Test]
        public void Should_Not_Create_New_Version_When_Not_Published_And_Property_Changed()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.Properties.First().Value = "hello world";
            Assert.IsFalse(content.ShouldCreateNewVersion(PublishedState.Unpublished));
        }



    }
}