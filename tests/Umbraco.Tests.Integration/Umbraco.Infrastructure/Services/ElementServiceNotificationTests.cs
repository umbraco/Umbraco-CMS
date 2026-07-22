// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true,
    Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class ElementServiceNotificationTests : UmbracoIntegrationTest
{
    [SetUp]
    public async Task SetupTest()
    {
        ContentRepositoryBase.ThrowOnWarning = true;
        await CreateTestData();
    }

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ElementService ElementService => (ElementService)GetRequiredService<IElementService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IContentType _elementType;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<ElementSavedNotification, ElementNotificationHandler>()
        .AddNotificationHandler<ElementPublishedNotification, ElementNotificationHandler>()
        .AddNotificationHandler<ElementUnpublishedNotification, ElementNotificationHandler>();

    private async Task CreateTestData()
    {
        _elementType = ContentTypeBuilder.CreateSimpleElementType();
        await ContentTypeService.CreateAsync(_elementType, Constants.Security.SuperUserKey);
    }

    private async Task MakeElementTypeVariant()
    {
        _elementType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in _elementType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_elementType, Constants.Security.SuperUserKey);
    }

    [Test]
    public void Can_Read_Saved_Cultures_For_Invariant()
    {
        IElement element = new Element("content", -1, _elementType);

        var savedWasCalled = false;

        ElementNotificationHandler.SavedElement = notification =>
        {
            IElement saved = notification.SavedEntities.First();

            Assert.IsNotNull(notification.SavedCultures);
            Assert.IsTrue(notification.SavedCultures.ContainsKey(saved.Key));
            CollectionAssert.AreEquivalent(new[] { "*" }, notification.SavedCultures[saved.Key]);

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(element);
            Assert.IsTrue(savedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.SavedElement = null;
        }
    }

    [Test]
    public async Task Can_Read_Only_Changed_Saved_Cultures()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);
        await MakeElementTypeVariant();

        IElement element = new Element("content", -1, _elementType);
        element.SetCultureName("hello", "en-US");
        element.SetCultureName("bonjour", "fr-FR");
        ElementService.Save(element);

        // re-get - dirty properties need resetting
        element = ElementService.GetById(element.Id);

        // only change the en-US culture
        element.SetValue("title", "title-en", "en-US");

        var savedWasCalled = false;

        ElementNotificationHandler.SavedElement = notification =>
        {
            IElement saved = notification.SavedEntities.First();

            Assert.IsNotNull(notification.SavedCultures);
            Assert.IsTrue(notification.SavedCultures.ContainsKey(saved.Key));

            // captured at raise-time even though the entity's change tracking has been reset by persistence
            CollectionAssert.AreEquivalent(new[] { "en-US" }, notification.SavedCultures[saved.Key]);

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(element);
            Assert.IsTrue(savedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.SavedElement = null;
        }
    }

    [Test]
    public void Can_Read_Published_Cultures_For_Invariant()
    {
        IElement element = new Element("content", -1, _elementType);
        ElementService.Save(element);

        var publishedWasCalled = false;

        ElementNotificationHandler.PublishedElement = notification =>
        {
            IElement published = notification.PublishedEntities.First();

            Assert.IsNotNull(notification.PublishedCultures);
            Assert.IsTrue(notification.PublishedCultures.ContainsKey(published.Key));
            CollectionAssert.AreEquivalent(new[] { "*" }, notification.PublishedCultures[published.Key]);

            publishedWasCalled = true;
        };

        try
        {
            ElementService.Publish(element, ["*"]);
            Assert.IsTrue(publishedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.PublishedElement = null;
        }
    }

    [Test]
    public async Task Can_Read_Only_Published_Cultures()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);
        await MakeElementTypeVariant();

        IElement element = new Element("content", -1, _elementType);
        element.SetCultureName("hello", "en-US");
        element.SetCultureName("bonjour", "fr-FR");
        ElementService.Save(element);

        // re-get - dirty properties need resetting
        element = ElementService.GetById(element.Id);

        var publishedWasCalled = false;

        ElementNotificationHandler.PublishedElement = notification =>
        {
            IElement published = notification.PublishedEntities.First();

            Assert.IsNotNull(notification.PublishedCultures);
            Assert.IsTrue(notification.PublishedCultures.ContainsKey(published.Key));
            CollectionAssert.AreEquivalent(new[] { "fr-FR" }, notification.PublishedCultures[published.Key]);

            publishedWasCalled = true;
        };

        try
        {
            ElementService.Publish(element, new[] { "fr-FR" });
            Assert.IsTrue(publishedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.PublishedElement = null;
        }
    }

    [Test]
    public void Can_Read_Unpublished_Cultures_For_Invariant()
    {
        IElement element = new Element("content", -1, _elementType);
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        var unpublishedWasCalled = false;

        ElementNotificationHandler.UnpublishedElement = notification =>
        {
            IElement unpublished = notification.UnpublishedEntities.First();

            Assert.IsNotNull(notification.UnpublishedCultures);
            Assert.IsTrue(notification.UnpublishedCultures.ContainsKey(unpublished.Key));
            CollectionAssert.AreEquivalent(new[] { "*" }, notification.UnpublishedCultures[unpublished.Key]);

            unpublishedWasCalled = true;
        };

        try
        {
            ElementService.Unpublish(element);
            Assert.IsTrue(unpublishedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.UnpublishedElement = null;
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Read_Unpublished_Cultures_When_Unpublishing_A_Culture()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);
        await MakeElementTypeVariant();

        IElement element = new Element("content", -1, _elementType);
        element.SetCultureName("hello", "en-US");
        element.SetCultureName("bonjour", "fr-FR");
        ElementService.Save(element);
        ElementService.Publish(element, element.AvailableCultures.ToArray());

        // re-get - dirty properties need resetting
        element = ElementService.GetById(element.Id);

        var publishedWasCalled = false;

        ElementNotificationHandler.PublishedElement = notification =>
        {
            IElement published = notification.PublishedEntities.First();

            // unpublishing a single culture is performed as a publish operation
            Assert.IsNotNull(notification.UnpublishedCultures);
            Assert.IsTrue(notification.UnpublishedCultures.ContainsKey(published.Key));
            CollectionAssert.AreEquivalent(new[] { "fr-FR" }, notification.UnpublishedCultures[published.Key]);

            publishedWasCalled = true;
        };

        try
        {
            ElementService.Unpublish(element, "fr-FR");
            Assert.IsTrue(publishedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.PublishedElement = null;
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Read_All_Unpublished_Cultures_When_Unpublishing_Whole_Variant_Element()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);
        await MakeElementTypeVariant();

        IElement element = new Element("content", -1, _elementType);
        element.SetCultureName("hello", "en-US");
        element.SetCultureName("bonjour", "fr-FR");
        ElementService.Save(element);
        ElementService.Publish(element, element.AvailableCultures.ToArray());

        // re-get - dirty properties need resetting
        element = ElementService.GetById(element.Id);

        var unpublishedWasCalled = false;

        ElementNotificationHandler.UnpublishedElement = notification =>
        {
            IElement unpublished = notification.UnpublishedEntities.First();

            Assert.IsNotNull(notification.UnpublishedCultures);
            Assert.IsTrue(notification.UnpublishedCultures.ContainsKey(unpublished.Key));

            // unpublishing the whole element reports every culture that was published, not an empty/partial set
            CollectionAssert.AreEquivalent(new[] { "en-US", "fr-FR" }, notification.UnpublishedCultures[unpublished.Key]);

            unpublishedWasCalled = true;
        };

        try
        {
            ElementService.Unpublish(element, "*");
            Assert.IsTrue(unpublishedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.UnpublishedElement = null;
        }
    }

    [Test]
    public async Task Can_Read_Per_Element_Saved_Cultures_For_Bulk_Save()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);
        await MakeElementTypeVariant();

        IElement elementOne = new Element("one", -1, _elementType);
        elementOne.SetCultureName("one-en", "en-US");

        IElement elementTwo = new Element("two", -1, _elementType);
        elementTwo.SetCultureName("two-en", "en-US");
        elementTwo.SetCultureName("two-fr", "fr-FR");

        var savedWasCalled = false;

        ElementNotificationHandler.SavedElement = notification =>
        {
            Assert.IsNotNull(notification.SavedCultures);

            // the culture map is keyed per element, so each element reports only its own changed cultures
            Assert.IsTrue(notification.SavedCultures.ContainsKey(elementOne.Key));
            Assert.IsTrue(notification.SavedCultures.ContainsKey(elementTwo.Key));
            CollectionAssert.AreEquivalent(new[] { "en-US" }, notification.SavedCultures[elementOne.Key]);
            CollectionAssert.AreEquivalent(new[] { "en-US", "fr-FR" }, notification.SavedCultures[elementTwo.Key]);

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(new[] { elementOne, elementTwo });
            Assert.IsTrue(savedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.SavedElement = null;
        }
    }

    [Test]
    public async Task Can_Read_Empty_Not_Null_Saved_Cultures_For_No_Op_Variant_Re_Save()
    {
        await MakeElementTypeVariant();

        IElement element = new Element("content", -1, _elementType);
        element.SetCultureName("hello", "en-US");
        ElementService.Save(element);

        // re-get so nothing is dirty, then re-save without changes
        element = ElementService.GetById(element.Id);

        var savedWasCalled = false;

        ElementNotificationHandler.SavedElement = notification =>
        {
            // the save tracked cultures and found none changed, so the map is present but empty - not null
            Assert.IsNotNull(notification.SavedCultures);
            Assert.IsEmpty(notification.SavedCultures);

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(element);
            Assert.IsTrue(savedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.SavedElement = null;
        }
    }

    [Test]
    public void Can_Read_Empty_Saved_Cultures_For_No_Op_Invariant_Re_Save()
    {
        IElement element = new Element("content", -1, _elementType);
        ElementService.Save(element);

        // re-get so nothing is dirty, then re-save without changes
        element = ElementService.GetById(element.Id);

        var savedWasCalled = false;

        ElementNotificationHandler.SavedElement = notification =>
        {
            // invariant elements report the "*" marker only when they changed; a no-op re-save reports nothing
            Assert.IsNotNull(notification.SavedCultures);
            Assert.IsEmpty(notification.SavedCultures);

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(element);
            Assert.IsTrue(savedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.SavedElement = null;
        }
    }

    [Test]
    public void Can_Read_Star_Marker_Saved_Cultures_For_Changed_Invariant_Save()
    {
        IElement element = new Element("content", -1, _elementType);
        ElementService.Save(element);

        // re-get so nothing is dirty, then make a genuine change before re-saving
        element = ElementService.GetById(element.Id);
        element.SetValue("title", "changed");

        var savedWasCalled = false;

        ElementNotificationHandler.SavedElement = notification =>
        {
            Assert.IsNotNull(notification.SavedCultures);
            Assert.IsTrue(notification.SavedCultures.ContainsKey(element.Key));
            CollectionAssert.AreEquivalent(new[] { "*" }, notification.SavedCultures[element.Key]);

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(element);
            Assert.IsTrue(savedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.SavedElement = null;
        }
    }

    [Test]
    public async Task Can_Read_Separate_Saved_Cultures_Per_Element_For_Mixed_Variance_Bulk_Save()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        // _elementType stays invariant; add a second, culture-variant element type so the bulk save is heterogeneous
        IContentType variantElementType = ContentTypeBuilder.CreateSimpleElementType("variantElement", "Variant Element");
        variantElementType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in variantElementType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.CreateAsync(variantElementType, Constants.Security.SuperUserKey);

        IElement invariantElement = new Element("invariant", -1, _elementType);

        IElement variantElement = new Element("variant", -1, variantElementType);
        variantElement.SetCultureName("hello", "en-US");
        variantElement.SetCultureName("bonjour", "fr-FR");

        var savedWasCalled = false;

        ElementNotificationHandler.SavedElement = notification =>
        {
            Assert.IsNotNull(notification.SavedCultures);

            // the per-element map keeps each element's cultures separate - the invariant "*" marker is not
            // conflated with the variant element's specific cultures (which a flat list could not represent)
            Assert.IsTrue(notification.SavedCultures.ContainsKey(invariantElement.Key));
            Assert.IsTrue(notification.SavedCultures.ContainsKey(variantElement.Key));
            CollectionAssert.AreEquivalent(new[] { "*" }, notification.SavedCultures[invariantElement.Key]);
            CollectionAssert.AreEquivalent(new[] { "en-US", "fr-FR" }, notification.SavedCultures[variantElement.Key]);

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(new[] { invariantElement, variantElement });
            Assert.IsTrue(savedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.SavedElement = null;
        }
    }

    [Test]
    public void Can_Read_Unpublished_Cultures_When_Deleting_Invariant()
    {
        IElement element = new Element("content", -1, _elementType);
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        // re-get - dirty properties need resetting
        element = ElementService.GetById(element.Id);

        var unpublishedWasCalled = false;

        ElementNotificationHandler.UnpublishedElement = notification =>
        {
            IElement unpublished = notification.UnpublishedEntities.First();

            Assert.IsNotNull(notification.UnpublishedCultures);
            Assert.IsTrue(notification.UnpublishedCultures.ContainsKey(unpublished.Key));
            CollectionAssert.AreEquivalent(new[] { "*" }, notification.UnpublishedCultures[unpublished.Key]);

            unpublishedWasCalled = true;
        };

        try
        {
            ElementService.Delete(element);
            Assert.IsTrue(unpublishedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.UnpublishedElement = null;
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Read_Unpublished_Cultures_When_Deleting_Variant()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);
        await MakeElementTypeVariant();

        IElement element = new Element("content", -1, _elementType);
        element.SetCultureName("hello", "en-US");
        element.SetCultureName("bonjour", "fr-FR");
        ElementService.Save(element);
        ElementService.Publish(element, element.AvailableCultures.ToArray());

        // re-get - dirty properties need resetting
        element = ElementService.GetById(element.Id);

        var unpublishedWasCalled = false;

        ElementNotificationHandler.UnpublishedElement = notification =>
        {
            IElement unpublished = notification.UnpublishedEntities.First();

            Assert.IsNotNull(notification.UnpublishedCultures);
            Assert.IsTrue(notification.UnpublishedCultures.ContainsKey(unpublished.Key));

            // deleting a published element reports every culture that was published
            CollectionAssert.AreEquivalent(new[] { "en-US", "fr-FR" }, notification.UnpublishedCultures[unpublished.Key]);

            unpublishedWasCalled = true;
        };

        try
        {
            ElementService.Delete(element);
            Assert.IsTrue(unpublishedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.UnpublishedElement = null;
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Read_Unpublished_Cultures_When_Deleting_Of_Types()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);
        await MakeElementTypeVariant();

        IElement element = new Element("content", -1, _elementType);
        element.SetCultureName("hello", "en-US");
        element.SetCultureName("bonjour", "fr-FR");
        ElementService.Save(element);
        ElementService.Publish(element, element.AvailableCultures.ToArray());

        var unpublishedWasCalled = false;

        ElementNotificationHandler.UnpublishedElement = notification =>
        {
            IElement unpublished = notification.UnpublishedEntities.First();

            Assert.IsNotNull(notification.UnpublishedCultures);
            Assert.IsTrue(notification.UnpublishedCultures.ContainsKey(unpublished.Key));
            CollectionAssert.AreEquivalent(new[] { "en-US", "fr-FR" }, notification.UnpublishedCultures[unpublished.Key]);

            unpublishedWasCalled = true;
        };

        try
        {
            ElementService.DeleteOfTypes(new[] { _elementType.Id });
            Assert.IsTrue(unpublishedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.UnpublishedElement = null;
        }
    }

    internal sealed class ElementNotificationHandler :
        INotificationHandler<ElementSavedNotification>,
        INotificationHandler<ElementPublishedNotification>,
        INotificationHandler<ElementUnpublishedNotification>
    {
        public static Action<ElementSavedNotification> SavedElement { get; set; }

        public static Action<ElementPublishedNotification> PublishedElement { get; set; }

        public static Action<ElementUnpublishedNotification> UnpublishedElement { get; set; }

        public void Handle(ElementSavedNotification notification) => SavedElement?.Invoke(notification);

        public void Handle(ElementPublishedNotification notification) => PublishedElement?.Invoke(notification);

        public void Handle(ElementUnpublishedNotification notification) => UnpublishedElement?.Invoke(notification);
    }
}
