using Microsoft.Extensions.Caching.Hybrid;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ContentHybridCacheTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IPublishedContentHybridCache PublishedContentHybridCache => GetRequiredService<IPublishedContentHybridCache>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    // Create CRUD Tests for Content, Also cultures.

    [Test]
    public async Task Can_Get_Draft_Content_By_Id()
    {
        //Act
        var textPage = await PublishedContentHybridCache.GetById(TextpageId, true);

        //Assert
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Draft_Content_By_Key()
    {
        // Act
        var textPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value, true);

        // Assert
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Published_Content_By_Id()
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule,
            Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(TextpageId);

        // Assert
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Published_Content_By_Key()
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value);

        // Assert
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Has_Content_By_Id_Returns_False_If_Not_In_Cache()
    {
        // Act
        var hasContent = await PublishedContentHybridCache.HasById(TextpageId);

        // Assert
        Assert.IsFalse(hasContent);
    }

    [Test]
    public async Task Has_Content_By_Id_Returns_True_If_In_Cache()
    {
        // Act
        var hasContent = await PublishedContentHybridCache.HasById(TextpageId, true);

        // Assert
        Assert.IsTrue(hasContent);
    }

    [Test]
    public async Task Has_Content_By_Id_Has_Content_After_Load()
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);
        var hybridCache = GetRequiredService<HybridCache>();
        await hybridCache.RemoveAsync(Textpage.Key.ToString());
        var hasContent = await PublishedContentHybridCache.HasById(TextpageId);
        Assert.IsFalse(hasContent);
        await PublishedContentHybridCache.GetById(TextpageId);

        // Act
        var hasContent2 = await PublishedContentHybridCache.HasById(TextpageId);

        // Assert
        Assert.IsTrue(hasContent2);
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Content_By_Id()
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(TextpageId, true);

        // Assert
        AssertTextPage(textPage);
        Assert.IsFalse(textPage.IsPublished());
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Content_By_Key()
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value, true);

        // Assert
        AssertTextPage(textPage);
        Assert.IsFalse(textPage.IsPublished());
    }

    [Test]
    public async Task Can_Get_Updated_Draft_Content_By_Id()
    {
        // Arrange
        await PublishedContentHybridCache.GetById(TextpageId, true);
        const string newName = "New Name";
        Textpage.InvariantName = newName;

        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            InvariantName = newName,
            InvariantProperties = Textpage.InvariantProperties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };

        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var updatedPage = await PublishedContentHybridCache.GetById(TextpageId, true);

        // Assert
        Assert.AreEqual(newName, updatedPage.Name);
    }

    [Test]
    public async Task Can_Get_Updated_Draft_Content_By_Key()
    {
        // Arrange
        await PublishedContentHybridCache.GetById(Textpage.Key.Value, true);
        const string newName = "New Name";
        Textpage.InvariantName = newName;

        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            InvariantName = newName,
            InvariantProperties = Textpage.InvariantProperties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };

        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var updatedPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value, true);

        // Assert
        Assert.AreEqual(newName, updatedPage.Name);
    }

    [Test]
    [TestCase(true, true)]
    [TestCase(false, false)]
    // BETTER NAMING, CURRENTLY THIS IS TESTING BOTH THE PUBLISHED AND THE DRAFT OF THE PUBLISHED.
    public async Task Can_Get_Updated_Draft_Published_Content_By_Id(bool preview, bool result)
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);

        const string newName = "New Name";
        Textpage.InvariantName = newName;

        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            InvariantName = newName,
            InvariantProperties = Textpage.InvariantProperties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };

        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(TextpageId, preview);

        // Assert
        Assert.AreEqual(result, newName.Equals(textPage.Name));
    }

    [Test]
    [TestCase(true, true)]
    [TestCase(false, false)]
    // BETTER NAMING, CURRENTLY THIS IS TESTING BOTH THE PUBLISHED AND THE DRAFT OF THE PUBLISHED.
    public async Task Can_Get_Updated_Draft_Published_Content_By_Key(bool preview, bool result)
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);

        const string newName = "New Name";
        Textpage.InvariantName = newName;

        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            InvariantName = newName,
            InvariantProperties = Textpage.InvariantProperties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };

        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value, preview);

        // Assert
        Assert.AreEqual(result, newName.Equals(textPage.Name));
    }

    [Test]
    public async Task Can_Get_Draft_Content_Property_By_Id()
    {
        // Arrange
        var titleValue = Textpage.InvariantProperties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetById(TextpageId, true);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Draft_Content_Property_By_Key()
    {
        // Arrange
        var titleValue = Textpage.InvariantProperties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value, true);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Published_Content_Property_By_Id()
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);
        var titleValue = Textpage.InvariantProperties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetById(TextpageId);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Published_Content_Property_By_Key()
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);
        var titleValue = Textpage.InvariantProperties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Content_Property_By_Id()
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);
        var titleValue = Textpage.InvariantProperties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetById(TextpageId, true);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Content_Property_By_Key()
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);
        var titleValue = Textpage.InvariantProperties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value, true);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Updated_Draft_Content_Property_By_Id()
    {
        // Arrange
        const string newTitle = "New Name";
        Textpage.InvariantProperties.First(x => x.Alias == "title").Value = newTitle;

        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            InvariantName = Textpage.InvariantName,
            InvariantProperties = Textpage.InvariantProperties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };

        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(TextpageId, true);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(newTitle, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Updated_Draft_Content_Property_By_Key()
    {
        // Arrange
        const string newTitle = "New Name";
        Textpage.InvariantProperties.First(x => x.Alias == "title").Value = newTitle;

        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            InvariantName = Textpage.InvariantName,
            InvariantProperties = Textpage.InvariantProperties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };

        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value, true);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(newTitle, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Updated_Published_Content_Property_By_Id()
    {
        // Arrange
        const string newTitle = "New Name";
        Textpage.InvariantProperties.First(x => x.Alias == "title").Value = newTitle;

        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            InvariantName = Textpage.InvariantName,
            InvariantProperties = Textpage.InvariantProperties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };

        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(TextpageId);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(newTitle, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Updated_Published_Content_Property_By_Key()
    {
        // Arrange
        const string newTitle = "New Name";
        Textpage.InvariantProperties.First(x => x.Alias == "title").Value = newTitle;

        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            InvariantName = Textpage.InvariantName,
            InvariantProperties = Textpage.InvariantProperties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };

        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(newTitle, textPage.Value("title"));
    }

    [Test]
    [TestCase(true, "New Name")]
    [TestCase(false, "Welcome to our Home page")]
    public async Task Can_Get_Updated_Draft_Of_Published_Content_Property_By_Id(bool preview, string titleName)
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);
        const string newTitle = "New Name";
        Textpage.InvariantProperties.First(x => x.Alias == "title").Value = newTitle;

        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            InvariantName = Textpage.InvariantName,
            InvariantProperties = Textpage.InvariantProperties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };

        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(TextpageId, preview);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(titleName, textPage.Value("title"));
    }

    [Test]
    [TestCase(true, "New Name")]
    [TestCase(false, "Welcome to our Home page")]
    public async Task Can_Get_Updated_Draft_Of_Published_Content_Property_By_Key(bool preview, string titleName)
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);
        const string newTitle = "New Name";
        Textpage.InvariantProperties.First(x => x.Alias == "title").Value = newTitle;

        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            InvariantName = Textpage.InvariantName,
            InvariantProperties = Textpage.InvariantProperties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };

        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value, preview);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(titleName, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Not_Get_Deleted_Content_By_Id()
    {
        // Arrange
        var content = await PublishedContentHybridCache.GetById(Subpage3.Key.Value, true);
        Assert.IsNotNull(content);
        await ContentEditingService.DeleteAsync(Textpage.Key.Value, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(TextpageId, true);

        // Assert
        Assert.IsNull(textPage);
    }

    [Test]
    public async Task Can_Not_Get_Deleted_Content_By_Key()
    {
        // Arrange
        var content = await PublishedContentHybridCache.GetById(Subpage3.Key.Value, true);

        Assert.IsNotNull(content);
        await ContentEditingService.DeleteAsync(Textpage.Key.Value, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value, true);

        // Assert
        Assert.IsNull(textPage);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Not_Get_Deleted_Published_Content_By_Id(bool preview)
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);
        await ContentEditingService.DeleteAsync(Textpage.Key.Value, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(TextpageId, preview);

        // Assert
        Assert.AreEqual(null, textPage);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Not_Get_Deleted_Published_Content_By_Key(bool preview)
    {
        // Arrange
        await ContentPublishingService.PublishAsync(Textpage.Key.Value, cultureAndSchedule, Constants.Security.SuperUserKey);
        await ContentEditingService.DeleteAsync(Textpage.Key.Value, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value, preview);

        // Assert
        Assert.AreEqual(null, textPage);
    }

    private void AssertTextPage(IPublishedContent textPage)
    {
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(textPage);
            Assert.AreEqual(Textpage.Key, textPage.Key);
            Assert.AreEqual(Textpage.InvariantName, textPage.Name);
        });

        AssertProperties(Textpage.InvariantProperties, textPage.Properties);
    }

    private void AssertProperties(IEnumerable<PropertyValueModel> propertyCollection, IEnumerable<IPublishedProperty> publishedProperties)
    {
        foreach (var prop in propertyCollection)
        {
            // var properties = publishedProperties as IPublishedProperty[] ?? publishedProperties.ToArray();
            AssertProperty(prop, publishedProperties.First(x => x.Alias == prop.Alias));
        }
    }

    private void AssertProperty(PropertyValueModel property, IPublishedProperty publishedProperty)
    {
        Assert.Multiple(() =>
        {
            Assert.AreEqual(property.Alias, publishedProperty.Alias);
            Assert.AreEqual(property.Value, publishedProperty.GetSourceValue());
        });
    }
}
