// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class ContentExtensionsTests
{
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
