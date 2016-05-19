using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class ContentExtensionsTests : BaseUmbracoConfigurationTest
    {
        #region RequiresSaving

        // when Published...

        [Test]
        public void RequireSaving_PublishedAndThatsAll_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            Assert.IsTrue(content.RequiresSaving());
        }

        [Test]
        public void RequireSaving_PublishedAndSavingAndNothingChanged_ShouldNot()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.ChangePublishedState(PublishedState.Saving); // saving

            Assert.IsFalse(content.RequiresSaving());
        }

        [Test]
        public void RequireSaving_PublishedAndSavingAndUserPropertyChanged_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.Properties.First().Value = "hello world"; // change data
            content.ChangePublishedState(PublishedState.Saving); // saving

            Assert.IsTrue(content.RequiresSaving());
        }

        [Test]
        public void RequireSaving_PublishedAndSavingAndContentPropertyChanged_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.ReleaseDate = DateTime.Now; // change data
            content.ChangePublishedState(PublishedState.Saving); // saving

            Assert.IsTrue(content.RequiresSaving());
        }

        [Test]
        public void RequireSaving_PublishedAndUnpublishing_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.ChangePublishedState(PublishedState.Unpublishing); // unpublishing

            Assert.IsTrue(content.RequiresSaving());
        }

        [Test]
        public void RequireSaving_PublishedAndPublishingAndNothingChanged_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.ChangePublishedState(PublishedState.Publishing); // publishing

            Assert.IsTrue(content.RequiresSaving());
        }

        // when Unpublished...

        [Test]
        public void RequireSaving_UnpublishedAndSavingAndNothingChanged_ShouldNot()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            Assert.IsFalse(content.RequiresSaving());
        }

        [Test]
        public void RequireSaving_UnpublishedAndSavingAndUserPropertyChanged_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.Properties.First().Value = "hello world"; // change data

            Assert.IsTrue(content.RequiresSaving());
        }

        [Test]
        public void RequireSaving_UnpublishedAndSavingAndContentPropertyChanged_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.ReleaseDate = DateTime.Now; // change data

            Assert.IsTrue(content.RequiresSaving());
        }

        [Test]
        public void RequireSaving_UnpublishedAndPublishing_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.ChangePublishedState(PublishedState.Publishing); // publishing

            Assert.IsTrue(content.RequiresSaving());
        }

        [Test]
        public void RequireSaving_When_UnpublishedAndUnpublishingAndNothingChanged_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.ChangePublishedState(PublishedState.Unpublishing); // unpublishing

            Assert.IsTrue(content.RequiresSaving());
        }

        #endregion

        #region RequiresNewVersion

        // when language...

        [Test]
        public void RequireNewVersion_LanguageChanged_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.Language = "en-AU";
            Assert.IsTrue(content.RequiresNewVersion());
        }

        // when Published...

        [Test]
        public void RequireNewVersion_PublishedAndThatsAll_ShouldNot()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            Assert.IsFalse(content.RequiresNewVersion());
        }

        [Test]
        public void RequireNewVersion_PublishedAndPublishingAndNothingChanged_ShouldNot()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.ChangePublishedState(PublishedState.Publishing);

            Assert.IsFalse(content.RequiresNewVersion());
        }

        [Test]
        public void RequireNewVersion_PublishedAndPublishdingAndUserPropertyChanged_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.Properties.First().Value = "hello world"; // change data
            content.ChangePublishedState(PublishedState.Publishing);

            Assert.IsTrue(content.RequiresNewVersion());
        }

        [Test]
        public void RequireNewVersion_PublishedAndPublishdingAndContentPropertyChanged_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.ReleaseDate = DateTime.Now; // change content property
            content.ChangePublishedState(PublishedState.Publishing);

            Assert.IsTrue(content.RequiresNewVersion());
        }

        [Test]
        public void RequireNewVersion_PublishedAndSavingAndNothingChanged_ShouldNot()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.ChangePublishedState(PublishedState.Saving); // saving

            Assert.IsFalse(content.RequiresNewVersion());
        }

        [Test]
        public void RequireNewVersion_PublishedAndSavingAndUserPropertyChanged_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.Properties.First().Value = "hello world"; // change data
            content.ChangePublishedState(PublishedState.Saving); // saving

            Assert.IsTrue(content.RequiresNewVersion());
        }

        [Test]
        public void RequireNewVersion_PublishedAndSavingAndContentPropertyChanged_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.ReleaseDate = DateTime.Now; // change content property
            content.ChangePublishedState(PublishedState.Saving); // saving

            Assert.IsTrue(content.RequiresNewVersion());
        }

        [Test]
        public void RequireNewVersion_PublishedAndUnpublishing_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.ChangePublishedState(PublishedState.Unpublishing); // unpublishing

            Assert.IsTrue(content.RequiresNewVersion());
        }

        // when Unpublished...

        [Test]
        public void RequireNewVersion_UnpublishedAndSavingAndNothingChanged_ShouldNot()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            Assert.IsFalse(content.RequiresNewVersion());
        }

        [Test] // debatable
        public void RequireNewVersion_UnpublishedAndSavingAndUserPropertyChanged_ShouldNot()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.Properties.First().Value = "hello world"; // change user property

            Assert.IsFalse(content.RequiresNewVersion());
        }

        [Test]
        public void RequireNewVersion_UnpublishedAndSavingAndContentPropertyChanged_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.ReleaseDate = DateTime.Now; // change content property

            Assert.IsTrue(content.RequiresNewVersion());
        }

        [Test]
        public void RequireNewVersion_UnpublishedAndPublishingAndNothingChanged_ShouldNot()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.ChangePublishedState(PublishedState.Publishing);

            Assert.IsFalse(content.RequiresNewVersion());
        }

        [Test]
        public void RequireNewVersion_UnpublishedAndPublishingAndUserPropertyChanged_ShouldNot()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.Properties.First().Value = "hello world"; // change user property
            content.ChangePublishedState(PublishedState.Publishing); // publishing

            Assert.IsFalse(content.RequiresNewVersion());
        }

        [Test]
        public void RequireNewVersion_UnpublishedAndPublishingAndContentPropertyChanged_ShouldNot()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.ReleaseDate = DateTime.Now; // change content property
            content.ChangePublishedState(PublishedState.Publishing); // publishing

            Assert.IsFalse(content.RequiresNewVersion());
        }

        [Test]
        public void RequireNewVersion_UnpublishedAndUnpublishing_ShouldNot()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.ChangePublishedState(PublishedState.Unpublishing); // unpublishing

            Assert.IsFalse(content.RequiresNewVersion());
        }

        #endregion

        #region ClearPublishedFlag

        [Test]
        public void ClearPublishedFlag_UnpublishedAndPublishing_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.ChangePublishedState(PublishedState.Publishing);
            Assert.IsTrue(content.RequiresClearPublishedFlag());
        }

        [Test]
        public void ClearPublishedFlag_UnpublishedAndUnpublishing_Should()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.ChangePublishedState(PublishedState.Unpublishing); // unpublishing - does not "change it"
            Assert.IsTrue(content.RequiresClearPublishedFlag());
        }

        [Test]
        public void ClearPublishedFlag_UnpublishedAndSaving_ShouldNot()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            content.ChangePublishedState(PublishedState.Saving); // saving
            Assert.IsFalse(content.RequiresClearPublishedFlag());
        }

        [Test]
        public void ClearPublishedFlag_PublishedAndSaving_ShouldNot()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.ChangePublishedState(PublishedState.Saving); // saving
            Assert.IsFalse(content.RequiresClearPublishedFlag());
        }

        [Test]
        public void ClearPublishedFlag_PublishedAndUnpublishing_Not()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.ChangePublishedState(PublishedState.Unpublishing); // unpublishing
            Assert.IsTrue(content.RequiresClearPublishedFlag());
        }

        [Test]
        public void ClearPublishedFlag_When_PublishedAndPublishing_Not()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            content.ChangePublishedState(PublishedState.Publishing); // publishing - does not "change it"
            Assert.IsTrue(content.RequiresClearPublishedFlag());
        }

        #endregion

        #region Others

        [Test]
        public void DirtyProperty_Reset_Clears_SavedPublishedState()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ChangePublishedState(PublishedState.Saving); // saved
            content.ResetDirtyProperties(false); // reset to .Unpublished
            Assert.AreEqual(PublishedState.Unpublished, content.PublishedState);
        }

        [Test]
        public void DirtyProperty_OnlyIfActuallyChanged_Content()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // if you assign a content property with its value it is not dirty
            // if you assign it with another value then back, it is dirty

            content.ResetDirtyProperties(false);
            Assert.IsFalse(content.IsPropertyDirty("Published"));
            content.Published = true;
            Assert.IsTrue(content.IsPropertyDirty("Published"));
            content.ResetDirtyProperties(false);
            Assert.IsFalse(content.IsPropertyDirty("Published"));
            content.Published = true;
            Assert.IsFalse(content.IsPropertyDirty("Published"));
            content.Published = false;
            content.Published = true;
            Assert.IsTrue(content.IsPropertyDirty("Published"));
        }

        [Test]
        public void DirtyProperty_OnlyIfActuallyChanged_User()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);
            var prop = content.Properties.First();

            // if you assign a user property with its value it is not dirty
            // if you assign it with another value then back, it is dirty

            prop.Value = "A";
            content.ResetDirtyProperties(false);
            Assert.IsFalse(prop.IsDirty());
            prop.Value = "B";
            Assert.IsTrue(prop.IsDirty());
            content.ResetDirtyProperties(false);
            Assert.IsFalse(prop.IsDirty());
            prop.Value = "B";
            Assert.IsFalse(prop.IsDirty());
            prop.Value = "A";
            prop.Value = "B";
            Assert.IsTrue(prop.IsDirty());
        }

        [Test]
        public void DirtyProperty_UpdateDate()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);
            var prop = content.Properties.First();

            content.ResetDirtyProperties(false);
            var d = content.UpdateDate;
            prop.Value = "A";
            Assert.IsTrue(content.IsAnyUserPropertyDirty());
            Assert.IsFalse(content.IsEntityDirty());
            Assert.AreEqual(d, content.UpdateDate);

            content.UpdateDate = DateTime.Now;
            Assert.IsTrue(content.IsEntityDirty());
            Assert.AreNotEqual(d, content.UpdateDate);

            // so... changing UpdateDate would count as a content property being changed
            // however in ContentRepository.PersistUpdatedItem, we change UpdateDate AFTER
            // we've tested for RequiresSaving & RequiresNewVersion so it's OK
        }

        [Test]
        public void DirtyProperty_WasDirty_ContentProperty()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);
            content.ResetDirtyProperties(false);
            Assert.IsFalse(content.IsDirty());
            Assert.IsFalse(content.WasDirty());
            content.Published = false;
            content.Published = true;
            Assert.IsTrue(content.IsDirty());
            Assert.IsFalse(content.WasDirty());
            content.ResetDirtyProperties(false);
            Assert.IsFalse(content.IsDirty());
            Assert.IsFalse(content.WasDirty());
            content.Published = false;
            content.Published = true;
            content.ResetDirtyProperties(true); // what PersistUpdatedItem does
            Assert.IsFalse(content.IsDirty());
            Assert.IsTrue(content.WasDirty());
            content.Published = false;
            content.Published = true;
            content.ResetDirtyProperties(); // what PersistUpdatedItem does
            Assert.IsFalse(content.IsDirty());
            Assert.IsTrue(content.WasDirty());
        }

        [Test]
        public void DirtyProperty_WasDirty_ContentSortOrder()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);
            content.ResetDirtyProperties(false);
            Assert.IsFalse(content.IsDirty());
            Assert.IsFalse(content.WasDirty());
            content.SortOrder = 0;
            content.SortOrder = 1;
            Assert.IsTrue(content.IsDirty());
            Assert.IsFalse(content.WasDirty());
            content.ResetDirtyProperties(false);
            Assert.IsFalse(content.IsDirty());
            Assert.IsFalse(content.WasDirty());
            content.SortOrder = 0;
            content.SortOrder = 1;
            content.ResetDirtyProperties(true); // what PersistUpdatedItem does
            Assert.IsFalse(content.IsDirty());
            Assert.IsTrue(content.WasDirty());
            content.SortOrder = 0;
            content.SortOrder = 1;
            content.ResetDirtyProperties(); // what PersistUpdatedItem does
            Assert.IsFalse(content.IsDirty());
            Assert.IsTrue(content.WasDirty());
        }

        [Test]
        public void DirtyProperty_WasDirty_UserProperty()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);
            var prop = content.Properties.First();
            content.ResetDirtyProperties(false);
            Assert.IsFalse(content.IsDirty());
            Assert.IsFalse(content.WasDirty());
            prop.Value = "a";
            prop.Value = "b";
            Assert.IsTrue(content.IsDirty());
            Assert.IsFalse(content.WasDirty());
            content.ResetDirtyProperties(false);
            Assert.IsFalse(content.IsDirty());
            Assert.IsFalse(content.WasDirty());
            prop.Value = "a";
            prop.Value = "b";
            content.ResetDirtyProperties(true); // what PersistUpdatedItem does
            Assert.IsFalse(content.IsDirty());
            //Assert.IsFalse(content.WasDirty()); // not impacted by user properties
            Assert.IsTrue(content.WasDirty()); // now it is!
            prop.Value = "a";
            prop.Value = "b";
            content.ResetDirtyProperties(); // what PersistUpdatedItem does
            Assert.IsFalse(content.IsDirty());
            //Assert.IsFalse(content.WasDirty()); // not impacted by user properties
            Assert.IsTrue(content.WasDirty()); // now it is!
        }
        
        #endregion
    }
}