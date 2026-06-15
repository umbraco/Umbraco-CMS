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
        Assert.That(content.Published, Is.False);
        content.ResetDirtyProperties(false); // resets
        Assert.That(content.PublishedState, Is.EqualTo(PublishedState.Unpublished));
        Assert.That(content.Published, Is.False);
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
        Assert.That(content.IsPropertyDirty("Published"), Is.False);
        content.Published = true;
        Assert.That(content.IsPropertyDirty("Published"), Is.True);
        content.ResetDirtyProperties(false);
        Assert.That(content.IsPropertyDirty("Published"), Is.False);
        content.Published = true;
        Assert.That(content.IsPropertyDirty("Published"), Is.False);
        content.Published = false;
        content.Published = true;
        Assert.That(content.IsPropertyDirty("Published"), Is.True);
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
        Assert.That(prop.IsDirty(), Is.False);
        prop.SetValue("B");
        Assert.That(prop.IsDirty(), Is.True);
        content.ResetDirtyProperties(false);
        Assert.That(prop.IsDirty(), Is.False);
        prop.SetValue("B");
        Assert.That(prop.IsDirty(), Is.False);
        prop.SetValue("A");
        prop.SetValue("B");
        Assert.That(prop.IsDirty(), Is.True);
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
        Assert.That(content.IsAnyUserPropertyDirty(), Is.True);
        Assert.That(content.IsEntityDirty(), Is.False);
        Assert.That(content.UpdateDate, Is.EqualTo(d));

        content.UpdateDate = DateTime.UtcNow;
        Assert.That(content.IsEntityDirty(), Is.True);
        Assert.That(content.UpdateDate, Is.Not.EqualTo(d));

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
        Assert.That(content.IsDirty(), Is.False);
        Assert.That(content.WasDirty(), Is.False);
        content.Published = false;
        content.Published = true;
        Assert.That(content.IsDirty(), Is.True);
        Assert.That(content.WasDirty(), Is.False);
        content.ResetDirtyProperties(false);
        Assert.That(content.IsDirty(), Is.False);
        Assert.That(content.WasDirty(), Is.False);
        content.Published = false;
        content.Published = true;
        content.ResetDirtyProperties(true); // what PersistUpdatedItem does
        Assert.That(content.IsDirty(), Is.False);
        Assert.That(content.WasDirty(), Is.True);
        content.Published = false;
        content.Published = true;
        content.ResetDirtyProperties(); // what PersistUpdatedItem does
        Assert.That(content.IsDirty(), Is.False);
        Assert.That(content.WasDirty(), Is.True);
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
        Assert.That(content.IsDirty(), Is.False);
        Assert.That(content.WasDirty(), Is.False);
        content.SortOrder = 0;
        content.SortOrder = 1;
        Assert.That(content.IsDirty(), Is.True);
        Assert.That(content.WasDirty(), Is.False);
        content.ResetDirtyProperties(false);
        Assert.That(content.IsDirty(), Is.False);
        Assert.That(content.WasDirty(), Is.False);
        content.SortOrder = 0;
        content.SortOrder = 1;
        content.ResetDirtyProperties(true); // what PersistUpdatedItem does
        Assert.That(content.IsDirty(), Is.False);
        Assert.That(content.WasDirty(), Is.True);
        content.SortOrder = 0;
        content.SortOrder = 1;
        content.ResetDirtyProperties(); // what PersistUpdatedItem does
        Assert.That(content.IsDirty(), Is.False);
        Assert.That(content.WasDirty(), Is.True);
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
        Assert.That(content.IsDirty(), Is.False);
        Assert.That(content.WasDirty(), Is.False);
        prop.SetValue("a");
        prop.SetValue("b");
        Assert.That(content.IsDirty(), Is.True);
        Assert.That(content.WasDirty(), Is.False);
        content.ResetDirtyProperties(false);
        Assert.That(content.IsDirty(), Is.False);
        Assert.That(content.WasDirty(), Is.False);
        prop.SetValue("a");
        prop.SetValue("b");
        content.ResetDirtyProperties(true); // what PersistUpdatedItem does
        Assert.That(content.IsDirty(), Is.False);
        //// Assert.IsFalse(content.WasDirty()); // not impacted by user properties
        Assert.That(content.WasDirty(), Is.True); // now it is!
        prop.SetValue("a");
        prop.SetValue("b");
        content.ResetDirtyProperties(); // what PersistUpdatedItem does
        Assert.That(content.IsDirty(), Is.False);
        //// Assert.IsFalse(content.WasDirty()); // not impacted by user properties
        Assert.That(content.WasDirty(), Is.True); // now it is!
    }
}
