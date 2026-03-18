// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Contains unit tests for methods in the <see cref="ContentExtensions"/> class.
/// </summary>
[TestFixture]
public class ContentExtensionsTests
{
    /// <summary>
    /// Tests that resetting dirty properties clears the saved published state.
    /// </summary>
    [Test]
    public void DirtyProperty_Reset_Clears_SavedPublishedState()
    {
        var contentTypeService = Mock.Of<IContentTypeService>();
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        content.PublishedState = PublishedState.Publishing;
        Assert.IsFalse(content.Published);
        content.ResetDirtyProperties(false); // resets
        Assert.AreEqual(PublishedState.Unpublished, content.PublishedState);
        Assert.IsFalse(content.Published);
    }

    /// <summary>
    /// Tests that the dirty property flag on content is only set when the property value actually changes.
    /// Verifies that assigning the same value does not mark the property as dirty,
    /// but changing the value and then changing it back does.
    /// </summary>
    [Test]
    public void DirtyProperty_OnlyIfActuallyChanged_Content()
    {
        var contentTypeService = Mock.Of<IContentTypeService>();
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

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

    /// <summary>
    /// Verifies that a property is only marked as dirty when its value actually changes.
    /// This test focuses on user property value assignments and ensures the dirty state
    /// is set only when the value transitions to something different, not when reassigning
    /// the same value.
    /// </summary>
    [Test]
    public void DirtyProperty_OnlyIfActuallyChanged_User()
    {
        var contentTypeService = Mock.Of<IContentTypeService>();
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);
        var prop = content.Properties.First();

        // if you assign a user property with its value it is not dirty
        // if you assign it with another value then back, it is dirty
        prop.SetValue("A");
        content.ResetDirtyProperties(false);
        Assert.IsFalse(prop.IsDirty());
        prop.SetValue("B");
        Assert.IsTrue(prop.IsDirty());
        content.ResetDirtyProperties(false);
        Assert.IsFalse(prop.IsDirty());
        prop.SetValue("B");
        Assert.IsFalse(prop.IsDirty());
        prop.SetValue("A");
        prop.SetValue("B");
        Assert.IsTrue(prop.IsDirty());
    }

    /// <summary>
    /// Tests the behavior of the UpdateDate property in relation to dirty property tracking.
    /// Verifies that changing a property marks the content as having dirty user properties without marking the entity dirty,
    /// and that directly changing UpdateDate marks the entity as dirty.
    /// </summary>
    [Test]
    public void DirtyProperty_UpdateDate()
    {
        var contentTypeService = Mock.Of<IContentTypeService>();
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);
        var prop = content.Properties.First();

        content.ResetDirtyProperties(false);
        var d = content.UpdateDate;
        prop.SetValue("A");
        Assert.IsTrue(content.IsAnyUserPropertyDirty());
        Assert.IsFalse(content.IsEntityDirty());
        Assert.AreEqual(d, content.UpdateDate);

        content.UpdateDate = DateTime.UtcNow;
        Assert.IsTrue(content.IsEntityDirty());
        Assert.AreNotEqual(d, content.UpdateDate);

        // so... changing UpdateDate would count as a content property being changed
        // however in ContentRepository.PersistUpdatedItem, we change UpdateDate AFTER
        // we've tested for RequiresSaving & RequiresNewVersion so it's OK
    }

    /// <summary>
    /// Verifies the behavior of the <c>IsDirty</c> and <c>WasDirty</c> flags on a content property
    /// as it is modified and reset. Ensures that the dirty state is tracked correctly and that
    /// the 'was dirty' state is set appropriately after property changes and resets.
    /// </summary>
    [Test]
    public void DirtyProperty_WasDirty_ContentProperty()
    {
        var contentTypeService = Mock.Of<IContentTypeService>();
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);
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

    /// <summary>
    /// Tests the dirty and was dirty state behavior of the Content SortOrder property.
    /// </summary>
    [Test]
    public void DirtyProperty_WasDirty_ContentSortOrder()
    {
        var contentTypeService = Mock.Of<IContentTypeService>();
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);
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

    /// <summary>
    /// Tests the dirty state behavior of a user property on content.
    /// Verifies that setting values and resetting dirty properties
    /// correctly updates the IsDirty and WasDirty flags.
    /// </summary>
    [Test]
    public void DirtyProperty_WasDirty_UserProperty()
    {
        var contentTypeService = Mock.Of<IContentTypeService>();
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);
        var prop = content.Properties.First();
        content.ResetDirtyProperties(false);
        Assert.IsFalse(content.IsDirty());
        Assert.IsFalse(content.WasDirty());
        prop.SetValue("a");
        prop.SetValue("b");
        Assert.IsTrue(content.IsDirty());
        Assert.IsFalse(content.WasDirty());
        content.ResetDirtyProperties(false);
        Assert.IsFalse(content.IsDirty());
        Assert.IsFalse(content.WasDirty());
        prop.SetValue("a");
        prop.SetValue("b");
        content.ResetDirtyProperties(true); // what PersistUpdatedItem does
        Assert.IsFalse(content.IsDirty());
        //// Assert.IsFalse(content.WasDirty()); // not impacted by user properties
        Assert.IsTrue(content.WasDirty()); // now it is!
        prop.SetValue("a");
        prop.SetValue("b");
        content.ResetDirtyProperties(); // what PersistUpdatedItem does
        Assert.IsFalse(content.IsDirty());
        //// Assert.IsFalse(content.WasDirty()); // not impacted by user properties
        Assert.IsTrue(content.WasDirty()); // now it is!
    }
}
