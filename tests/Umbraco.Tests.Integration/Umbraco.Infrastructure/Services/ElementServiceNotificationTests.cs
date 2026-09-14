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

            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures.ContainsKey(saved.Key), Is.True);
            Assert.That(notification.SavedCultures[saved.Key], Is.EquivalentTo(new[] { "*" }));

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(element);
            Assert.That(savedWasCalled, Is.True);
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

            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures.ContainsKey(saved.Key), Is.True);

            // captured at raise-time even though the entity's change tracking has been reset by persistence
            Assert.That(notification.SavedCultures[saved.Key], Is.EquivalentTo(new[] { "en-US" }));

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(element);
            Assert.That(savedWasCalled, Is.True);
        }
        finally
        {
            ElementNotificationHandler.SavedElement = null;
        }
    }

    [Test]
    public void Can_Read_Saved_Notification_When_Save_And_Publishing_Invariant()
    {
        // A combined save-and-publish must still raise the paired Saved notification, just like a plain Save does
        // (https://github.com/umbraco/Umbraco-CMS/issues/23523).
        IElement element = new Element("content", -1, _elementType);

        var savedWasCalled = false;
        var publishedWasCalled = false;

        ElementNotificationHandler.SavedElement = notification =>
        {
            IElement saved = notification.SavedEntities.First();

            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures.ContainsKey(saved.Key), Is.True);
            Assert.That(notification.SavedCultures[saved.Key], Is.EquivalentTo(new[] { "*" }));

            savedWasCalled = true;
        };

        ElementNotificationHandler.PublishedElement = _ => publishedWasCalled = true;

        try
        {
            PublishResult result = ElementService.SaveAndPublish(element, []);
            Assert.That(result.Success, Is.True);
            Assert.That(savedWasCalled, Is.True, "ElementSavedNotification should fire when saving and publishing.");
            Assert.That(publishedWasCalled, Is.True, "ElementPublishedNotification should fire when saving and publishing.");
        }
        finally
        {
            ElementNotificationHandler.SavedElement = null;
            ElementNotificationHandler.PublishedElement = null;
        }
    }

    [Test]
    public async Task Can_Read_Saved_Notification_When_Save_And_Publishing_Variant()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);
        await MakeElementTypeVariant();

        IElement element = new Element("content", -1, _elementType);
        element.SetCultureName("hello", "en-US");
        element.SetCultureName("bonjour", "fr-FR");

        var savedWasCalled = false;

        ElementNotificationHandler.SavedElement = notification =>
        {
            IElement saved = notification.SavedEntities.First();

            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures.ContainsKey(saved.Key), Is.True);

            // both cultures were changed as part of the save-and-publish, so both are reported as saved
            Assert.That(notification.SavedCultures[saved.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            savedWasCalled = true;
        };

        try
        {
            PublishResult result = ElementService.SaveAndPublish(element, ["en-US", "fr-FR"]);
            Assert.That(result.Success, Is.True);
            Assert.That(savedWasCalled, Is.True, "ElementSavedNotification should fire when saving and publishing.");
        }
        finally
        {
            ElementNotificationHandler.SavedElement = null;
        }
    }

    [Test]
    public async Task Can_Read_All_Changed_Cultures_As_Saved_When_Publishing_A_Subset()
    {
        // The saved cultures must reflect what was *changed*, not what was *published*: both cultures are edited here
        // but only one is published, so the Saved notification reports both while the Published notification reports one.
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);
        await MakeElementTypeVariant();

        IElement element = new Element("content", -1, _elementType);
        element.SetCultureName("hello", "en-US");
        element.SetCultureName("bonjour", "fr-FR");

        var savedWasCalled = false;
        var publishedWasCalled = false;

        ElementNotificationHandler.SavedElement = notification =>
        {
            IElement saved = notification.SavedEntities.First();

            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures.ContainsKey(saved.Key), Is.True);
            Assert.That(notification.SavedCultures[saved.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            savedWasCalled = true;
        };

        ElementNotificationHandler.PublishedElement = notification =>
        {
            IElement published = notification.PublishedEntities.First();

            Assert.That(notification.PublishedCultures, Is.Not.Null);
            Assert.That(notification.PublishedCultures.ContainsKey(published.Key), Is.True);
            Assert.That(notification.PublishedCultures[published.Key], Is.EquivalentTo(new[] { "en-US" }));

            publishedWasCalled = true;
        };

        try
        {
            PublishResult result = ElementService.SaveAndPublish(element, ["en-US"]);
            Assert.That(result.Success, Is.True);
            Assert.That(savedWasCalled, Is.True, "ElementSavedNotification should fire when saving and publishing.");
            Assert.That(publishedWasCalled, Is.True, "ElementPublishedNotification should fire when saving and publishing.");
        }
        finally
        {
            ElementNotificationHandler.SavedElement = null;
            ElementNotificationHandler.PublishedElement = null;
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

            Assert.That(notification.PublishedCultures, Is.Not.Null);
            Assert.That(notification.PublishedCultures.ContainsKey(published.Key), Is.True);
            Assert.That(notification.PublishedCultures[published.Key], Is.EquivalentTo(new[] { "*" }));

            publishedWasCalled = true;
        };

        try
        {
            ElementService.Publish(element, ["*"]);
            Assert.That(publishedWasCalled, Is.True);
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

            Assert.That(notification.PublishedCultures, Is.Not.Null);
            Assert.That(notification.PublishedCultures.ContainsKey(published.Key), Is.True);
            Assert.That(notification.PublishedCultures[published.Key], Is.EquivalentTo(new[] { "fr-FR" }));

            publishedWasCalled = true;
        };

        try
        {
            ElementService.Publish(element, new[] { "fr-FR" });
            Assert.That(publishedWasCalled, Is.True);
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

            Assert.That(notification.UnpublishedCultures, Is.Not.Null);
            Assert.That(notification.UnpublishedCultures.ContainsKey(unpublished.Key), Is.True);
            Assert.That(notification.UnpublishedCultures[unpublished.Key], Is.EquivalentTo(new[] { "*" }));

            unpublishedWasCalled = true;
        };

        try
        {
            ElementService.Unpublish(element);
            Assert.That(unpublishedWasCalled, Is.True);
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
            Assert.That(notification.UnpublishedCultures, Is.Not.Null);
            Assert.That(notification.UnpublishedCultures.ContainsKey(published.Key), Is.True);
            Assert.That(notification.UnpublishedCultures[published.Key], Is.EquivalentTo(new[] { "fr-FR" }));

            publishedWasCalled = true;
        };

        try
        {
            ElementService.Unpublish(element, "fr-FR");
            Assert.That(publishedWasCalled, Is.True);
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

            Assert.That(notification.UnpublishedCultures, Is.Not.Null);
            Assert.That(notification.UnpublishedCultures.ContainsKey(unpublished.Key), Is.True);

            // unpublishing the whole element reports every culture that was published, not an empty/partial set
            Assert.That(notification.UnpublishedCultures[unpublished.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            unpublishedWasCalled = true;
        };

        try
        {
            ElementService.Unpublish(element, "*");
            Assert.That(unpublishedWasCalled, Is.True);
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
            Assert.That(notification.SavedCultures, Is.Not.Null);

            // the culture map is keyed per element, so each element reports only its own changed cultures
            Assert.That(notification.SavedCultures.ContainsKey(elementOne.Key), Is.True);
            Assert.That(notification.SavedCultures.ContainsKey(elementTwo.Key), Is.True);
            Assert.That(notification.SavedCultures[elementOne.Key], Is.EquivalentTo(new[] { "en-US" }));
            Assert.That(notification.SavedCultures[elementTwo.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(new[] { elementOne, elementTwo });
            Assert.That(savedWasCalled, Is.True);
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
            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures, Is.Empty);

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(element);
            Assert.That(savedWasCalled, Is.True);
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
            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures, Is.Empty);

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(element);
            Assert.That(savedWasCalled, Is.True);
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
            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures.ContainsKey(element.Key), Is.True);
            Assert.That(notification.SavedCultures[element.Key], Is.EquivalentTo(new[] { "*" }));

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(element);
            Assert.That(savedWasCalled, Is.True);
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
            Assert.That(notification.SavedCultures, Is.Not.Null);

            // the per-element map keeps each element's cultures separate - the invariant "*" marker is not
            // conflated with the variant element's specific cultures (which a flat list could not represent)
            Assert.That(notification.SavedCultures.ContainsKey(invariantElement.Key), Is.True);
            Assert.That(notification.SavedCultures.ContainsKey(variantElement.Key), Is.True);
            Assert.That(notification.SavedCultures[invariantElement.Key], Is.EquivalentTo(new[] { "*" }));
            Assert.That(notification.SavedCultures[variantElement.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            savedWasCalled = true;
        };

        try
        {
            ElementService.Save(new[] { invariantElement, variantElement });
            Assert.That(savedWasCalled, Is.True);
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

            Assert.That(notification.UnpublishedCultures, Is.Not.Null);
            Assert.That(notification.UnpublishedCultures.ContainsKey(unpublished.Key), Is.True);
            Assert.That(notification.UnpublishedCultures[unpublished.Key], Is.EquivalentTo(new[] { "*" }));

            unpublishedWasCalled = true;
        };

        try
        {
            ElementService.Delete(element);
            Assert.That(unpublishedWasCalled, Is.True);
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

            Assert.That(notification.UnpublishedCultures, Is.Not.Null);
            Assert.That(notification.UnpublishedCultures.ContainsKey(unpublished.Key), Is.True);

            // deleting a published element reports every culture that was published
            Assert.That(notification.UnpublishedCultures[unpublished.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            unpublishedWasCalled = true;
        };

        try
        {
            ElementService.Delete(element);
            Assert.That(unpublishedWasCalled, Is.True);
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

            Assert.That(notification.UnpublishedCultures, Is.Not.Null);
            Assert.That(notification.UnpublishedCultures.ContainsKey(unpublished.Key), Is.True);
            Assert.That(notification.UnpublishedCultures[unpublished.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            unpublishedWasCalled = true;
        };

        try
        {
            ElementService.DeleteOfTypes(new[] { _elementType.Id });
            Assert.That(unpublishedWasCalled, Is.True);
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
