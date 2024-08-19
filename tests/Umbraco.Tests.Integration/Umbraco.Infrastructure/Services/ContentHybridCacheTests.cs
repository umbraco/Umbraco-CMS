using Microsoft.Extensions.Caching.Hybrid;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ContentHybridCacheTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IPublishedContentHybridCache PublishedContentHybridCache => GetRequiredService<IPublishedContentHybridCache>();

    private ContentEditingService ContentEditingService => (ContentEditingService)GetRequiredService<IContentEditingService>();


    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    // Create CRUD Tests for Content, Also cultures.

    [Test]
    public async Task Can_Get_Draft_Content_By_Id()
    {
    //Act
    var textPage = await PublishedContentHybridCache.GetById(Textpage.Key.Value, true);

    var test = ContentService.GetById(Textpage.Key.Value);

    var tester = await ContentEditingService.GetAsync(Textpage.Key.Value);


    //Assert
        AssertTextPage(textPage);
    }

    // [Test]
    // public async Task Can_Get_Draft_Content_By_Key()
    // {
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Key, true);
    //
    //     // Assert
    //     AssertTextPage(textPage);
    // }
    //
    // [Test]
    // public async Task Can_Get_Published_Content_By_Id()
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Id);
    //
    //     // Assert
    //     AssertTextPage(textPage);
    // }
    //
    // [Test]
    // public async Task Can_Get_Published_Content_By_Key()
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Key);
    //
    //     // Assert
    //     AssertTextPage(textPage);
    // }
    //
    // [Test]
    // public async Task Has_Content_By_Id_Returns_False_If_Not_In_Cache()
    // {
    //     // Arrange
    //     // Act
    //     var hasContent = await PublishedContentHybridCache.HasById(Textpage.Id);
    //
    //     // Assert
    //     Assert.IsFalse(hasContent);
    // }
    //
    // [Test]
    // public async Task Has_Content_By_Id_Returns_True_If_In_Cache()
    // {
    //     // Act
    //     var hasContent = await PublishedContentHybridCache.HasById(Textpage.Id, true);
    //
    //     // Assert
    //     Assert.IsTrue(hasContent);
    // }
    //
    // [Test]
    // public async Task Has_Content_By_Id_Has_Content_After_Load()
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //     var hybridCache = GetRequiredService<HybridCache>();
    //     await hybridCache.RemoveAsync(Textpage.Key.ToString());
    //     var hasContent = await PublishedContentHybridCache.HasById(Textpage.Id);
    //     Assert.IsFalse(hasContent);
    //     await PublishedContentHybridCache.GetById(Textpage.Id);
    //
    //     // Act
    //     var hasContent2 = await PublishedContentHybridCache.HasById(Textpage.Id);
    //
    //     // Assert
    //     Assert.IsTrue(hasContent2);
    // }
    //
    // [Test]
    // public async Task Can_Get_Draft_Of_Published_Content_By_Id()
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Id, true);
    //
    //     // Assert
    //     AssertTextPage(textPage);
    //     Assert.IsFalse(textPage.IsPublished());
    // }
    //
    // [Test]
    // public async Task Can_Get_Draft_Of_Published_Content_By_Key()
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Key, true);
    //
    //     // Assert
    //     AssertTextPage(textPage);
    //     Assert.IsFalse(textPage.IsPublished());
    // }
    //
    // [Test]
    // public async Task Can_Get_Updated_Draft_Content_By_Id()
    // {
    //     // Arrange
    //     await PublishedContentHybridCache.GetById(Textpage.Id, true);
    //     string newName = "New Name";
    //     Textpage.Name = newName;
    //     ContentService.Save(Textpage, -1);
    //
    //     // Act
    //     var updatedPage = await PublishedContentHybridCache.GetById(Textpage.Id, true);
    //
    //     // Assert
    //     Assert.AreEqual(newName, updatedPage.Name);
    // }
    //
    // [Test]
    // public async Task Can_Get_Updated_Draft_Content_By_Key()
    // {
    //     // Arrange
    //     await PublishedContentHybridCache.GetById(Textpage.Id, true);
    //     string newName = "New Name";
    //     Textpage.Name = newName;
    //     ContentService.Save(Textpage, -1);
    //
    //     // Act
    //     var updatedPage = await PublishedContentHybridCache.GetById(Textpage.Key, true);
    //
    //     // Assert
    //     Assert.AreEqual(newName, updatedPage.Name);
    // }
    //
    // [Test]
    // [TestCase(true, true)]
    // [TestCase(false, false)]
    // // BETTER NAMING, CURRENTLY THIS IS TESTING BOTH THE PUBLISHED AND THE DRAFT OF THE PUBLISHED.
    // public async Task Can_Get_Updated_Draft_Published_Content_By_Id(bool preview, bool result)
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //     string newName = "New Name";
    //     Textpage.Name = newName;
    //     ContentService.Save(Textpage, -1);
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Id, preview);
    //
    //     // Assert
    //     Assert.AreEqual(result, newName.Equals(textPage.Name));
    // }
    //
    // [Test]
    // [TestCase(true, true)]
    // [TestCase(false, false)]
    // // BETTER NAMING, CURRENTLY THIS IS TESTING BOTH THE PUBLISHED AND THE DRAFT OF THE PUBLISHED.
    // public async Task Can_Get_Updated_Draft_Published_Content_By_Key(bool preview, bool result)
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //     string newName = "New Name";
    //     Textpage.Name = newName;
    //     ContentService.Save(Textpage, -1);
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Key, preview);
    //
    //     // Assert
    //     Assert.AreEqual(result, newName.Equals(textPage.Name));
    // }
    //
    // [Test]
    // public async Task Can_Get_Draft_Content_Property_By_Id()
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //     var titleValue = Textpage.GetValue("title");
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Id, true);
    //
    //     // Assert
    //     using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
    //     Assert.AreEqual(titleValue, textPage.Value("title"));
    // }
    //
    // [Test]
    // public async Task Can_Get_Draft_Content_Property_By_Key()
    // {
    //     // Arrange
    //     ContentService.Save(Textpage, -1);
    //     var titleValue = Textpage.GetValue("title");
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Key, true);
    //
    //     // Assert
    //     using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
    //     Assert.AreEqual(titleValue, textPage.Value("title"));
    // }
    //
    // [Test]
    // public async Task Can_Get_Published_Content_Property_By_Id()
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //     var titleValue = Textpage.GetValue("title");
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Id, true);
    //
    //     // Assert
    //     using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
    //     Assert.AreEqual(titleValue, textPage.Value("title"));
    // }
    //
    // [Test]
    // public async Task Can_Get_Published_Content_Property_By_Key()
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //     var titleValue = Textpage.GetValue("title");
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Key, true);
    //
    //     // Assert
    //     using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
    //     Assert.AreEqual(titleValue, textPage.Value("title"));
    // }
    //
    // [Test]
    // public async Task Can_Get_Draft_Of_Published_Content_Property_By_Id()
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //     var titleValue = Textpage.GetValue("title");
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Id, true);
    //
    //     // Assert
    //     using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
    //     Assert.AreEqual(titleValue, textPage.Value("title"));
    // }
    //
    // [Test]
    // public async Task Can_Get_Draft_Of_Published_Content_Property_By_Key()
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //     var titleValue = Textpage.GetValue("title");
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Key, true);
    //
    //     // Assert
    //     using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
    //     Assert.AreEqual(titleValue, textPage.Value("title"));
    // }
    //
    // [Test]
    // public async Task Can_Get_Updated_Draft_Content_Property_By_Id()
    // {
    //     // Arrange
    //     string newTitle = "New Name";
    //     Textpage.SetValue("title", newTitle);
    //     ContentService.Save(Textpage, -1);
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Id, true);
    //
    //     // Assert
    //     using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
    //     Assert.AreEqual(newTitle, textPage.Value("title"));
    // }
    //
    // [Test]
    // public async Task Can_Get_Updated_Draft_Content_Property_By_Key()
    // {
    //     // Arrange
    //     string newTitle = "New Name";
    //     Textpage.SetValue("title", newTitle);
    //     ContentService.Save(Textpage, -1);
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Key, true);
    //
    //     // Assert
    //     using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
    //     Assert.AreEqual(newTitle, textPage.Value("title"));
    // }
    //
    // [Test]
    // public async Task Can_Get_Updated_Published_Content_Property_By_Key()
    // {
    //     // Arrange
    //     string newTitle = "New Name";
    //     Textpage.SetValue("title", newTitle);
    //     ContentService.Save(Textpage, -1);
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Key);
    //
    //     // Assert
    //     using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
    //     Assert.AreEqual(newTitle, textPage.Value("title"));
    // }
    //
    // [Test]
    // [TestCase(true, "New Name")]
    // [TestCase(false, "Welcome to our Home page")]
    // public async Task Can_Get_Updated_Draft_Of_Published_Content_Property_By_Id(bool preview, string titleName)
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //     Textpage.SetValue("title", "New Name");
    //     ContentService.Save(Textpage, -1);
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Id, preview);
    //
    //     // Assert
    //     using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
    //     Assert.AreEqual(titleName, textPage.Value("title"));
    // }
    //
    // [Test]
    // public async Task Can_Get_Updated_Draft_Of_Published_Content_Property_By_Key()
    // {
    //     // Arrange
    //     string newTitle = "New Name";
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //     Textpage.SetValue("title", newTitle);
    //     ContentService.Save(Textpage, -1);
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Key, true);
    //
    //     // Assert
    //     using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
    //     Assert.AreEqual(newTitle, textPage.Value("title"));
    // }
    //
    // [Test]
    // public async Task Can_Not_Get_Deleted_Content_By_Id()
    // {
    //     // Arrange
    //     var content = await PublishedContentHybridCache.GetById(Subpage3.Id, true);
    //
    //     Assert.IsNotNull(content);
    //     ContentService.Delete(Textpage);
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Id, true);
    //
    //     // Assert
    //     Assert.IsNull(textPage);
    // }
    //
    // [Test]
    // public async Task Can_Not_Get_Deleted_Content_By_Key()
    // {
    //     // Arrange
    //     await PublishedContentHybridCache.GetById(Subpage3.Key, true);
    //     ContentService.Delete(Textpage);
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Key, true);
    //
    //     // Assert
    //     Assert.AreEqual(null, textPage);
    // }
    //
    // [Test]
    // [TestCase(true)]
    // [TestCase(false)]
    // public async Task Can_Not_Get_Deleted_Published_Content_By_Id(bool preview)
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //     ContentService.Delete(Textpage);
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Id, preview);
    //
    //     // Assert
    //     Assert.AreEqual(null, textPage);
    // }
    //
    // [Test]
    // [TestCase(true)]
    // [TestCase(false)]
    // public async Task Can_Not_Get_Deleted_Published_Content_By_Key(bool preview)
    // {
    //     // Arrange
    //     ContentService.Publish(Textpage, Array.Empty<string>());
    //     ContentService.Delete(Textpage);
    //
    //     // Act
    //     var textPage = await PublishedContentHybridCache.GetById(Textpage.Key, preview);
    //
    //     // Assert
    //     Assert.AreEqual(null, textPage);
    // }

    private void AssertTextPage(IPublishedContent textPage)
    {
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(textPage);
            Assert.AreEqual(Textpage.Key, textPage.Key);
            Assert.AreEqual(Textpage.InvariantName, textPage.Name);
        });
        // AssertProperties(Textpage.InvariantProperties, textPage.Properties);
    }

    private void AssertProperties(IPropertyCollection propertyCollection, IEnumerable<IPublishedProperty> publishedProperties)
    {
        foreach (var prop in propertyCollection)
        {
            AssertProperty(prop, publishedProperties.First(x => x.Alias == prop.Alias));
        }
    }

    private void AssertProperty(IProperty property, IPublishedProperty publishedProperty)
    {
        Assert.Multiple(() =>
        {
            Assert.AreEqual(property.Alias, publishedProperty.Alias);
            Assert.AreEqual(property.PropertyType.Alias, publishedProperty.PropertyType.Alias);
        });
    }
}
