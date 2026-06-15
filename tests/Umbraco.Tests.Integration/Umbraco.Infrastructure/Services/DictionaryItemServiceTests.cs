using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DictionaryItemServiceTests : UmbracoIntegrationTest
{
    private Guid _parentItemId;
    private Guid _childItemId;

    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    [SetUp]
    public async Task SetUp() => await CreateTestData();

    [Test]
    public async Task Can_Get_Root_Dictionary_Items()
    {
        var rootItems = await DictionaryItemService.GetAtRootAsync();

        Assert.That(rootItems, Is.Not.Null);
        Assert.That(rootItems.Any(), Is.True);
    }

    [Test]
    public async Task Can_Determine_If_DictionaryItem_Exists()
    {
        var exists = await DictionaryItemService.ExistsAsync("Parent");
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task Can_Get_Dictionary_Item_By_Id()
    {
        var parentItem = await DictionaryItemService.GetAsync(_parentItemId);
        Assert.That(parentItem, Is.Not.Null);

        var childItem = await DictionaryItemService.GetAsync(_childItemId);
        Assert.That(childItem, Is.Not.Null);
    }

    [Test]
    public async Task Can_Get_Dictionary_Items_By_Ids()
    {
        var items = await DictionaryItemService.GetManyAsync(_parentItemId, _childItemId);
        Assert.That(items.Count(), Is.EqualTo(2));
        Assert.That(items.FirstOrDefault(i => i.Key == _parentItemId), Is.Not.Null);
        Assert.That(items.FirstOrDefault(i => i.Key == _childItemId), Is.Not.Null);
    }

    [Test]
    public async Task Can_Get_Dictionary_Item_By_Key()
    {
        var parentItem = await DictionaryItemService.GetAsync("Parent");
        Assert.That(parentItem, Is.Not.Null);

        var childItem = await DictionaryItemService.GetAsync("Child");
        Assert.That(childItem, Is.Not.Null);
    }

    [Test]
    public async Task Can_Get_Dictionary_Items_By_Keys()
    {
        var items = await DictionaryItemService.GetManyAsync("Parent", "Child");
        Assert.That(items.Count(), Is.EqualTo(2));
        Assert.That(items.FirstOrDefault(i => i.ItemKey == "Parent"), Is.Not.Null);
        Assert.That(items.FirstOrDefault(i => i.ItemKey == "Child"), Is.Not.Null);
    }

    [Test]
    public async Task Does_Not_Fail_When_DictionaryItem_Doesnt_Exist()
    {
        var item = await DictionaryItemService.GetAsync("RandomKey");
        Assert.That(item, Is.Null);
    }

    [Test]
    public async Task Can_Get_Dictionary_Item_Children()
    {
        var item = await DictionaryItemService.GetChildrenAsync(_parentItemId);
        Assert.That(item, Is.Not.Null);
        Assert.That(item.Count(), Is.EqualTo(1));

        foreach (var dictionaryItem in item)
        {
            Assert.That(dictionaryItem.ParentId, Is.EqualTo(_parentItemId));
            Assert.That(string.IsNullOrEmpty(dictionaryItem.ItemKey), Is.False);
        }
    }

    [Test]
    public async Task Can_Get_Dictionary_Item_Descendants()
    {
        using (var scope = ScopeProvider.CreateScope())
        {
            var en = await LanguageService.GetAsync("en-GB");
            var dk = await LanguageService.GetAsync("da-DK");

            var currParentId = _childItemId;
            for (var i = 0; i < 25; i++)
            {
                // Create 2 per level
                var result = await DictionaryItemService.CreateAsync(
                    new DictionaryItem(currParentId, "D1" + i)
                    {
                        Translations = new List<IDictionaryTranslation>
                        {
                            new DictionaryTranslation(en, "ChildValue1 " + i),
                            new DictionaryTranslation(dk, "BørnVærdi1 " + i)
                        }
                    },
                    Constants.Security.SuperUserKey);

                Assert.That(result.Success, Is.True);

                await DictionaryItemService.CreateAsync(
                    new DictionaryItem(currParentId, "D2" + i)
                    {
                        Translations = new List<IDictionaryTranslation>
                        {
                            new DictionaryTranslation(en, "ChildValue2 " + i),
                            new DictionaryTranslation(dk, "BørnVærdi2 " + i)
                        }
                    },
                    Constants.Security.SuperUserKey);

                currParentId = result.Result!.Key;
            }

            ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
            ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = true;

            var items = (await DictionaryItemService.GetDescendantsAsync(_parentItemId)).ToArray();

            Debug.WriteLine("SQL CALLS: " + ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().SqlCount);

            Assert.That(items, Has.Length.EqualTo(51));

            // There's a call or two to get languages, so apart from that there should only be one call per level.
            Assert.That(ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().SqlCount, Is.LessThan(30));
        }
    }

    [Test]
    public async Task Can_Create_DictionaryItem_At_Root()
    {
        var english = await LanguageService.GetAsync("en-US");

        var result = await DictionaryItemService.CreateAsync(
            new DictionaryItem("Testing123")
            {
                Translations = new List<IDictionaryTranslation> { new DictionaryTranslation(english, "Hello world") }
            },
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // re-get
        var item = await DictionaryItemService.GetAsync(result.Result!.Key);
        Assert.That(item, Is.Not.Null);

        Assert.That(item.Id, Is.GreaterThan(0));
        Assert.That(item.HasIdentity, Is.True);
        Assert.That(item.ParentId.HasValue, Is.False);
        Assert.That(item.ItemKey, Is.EqualTo("Testing123"));
        Assert.That(item.Translations.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Can_Create_DictionaryItem_Under_Parent_DictionaryItem()
    {
        var english = await LanguageService.GetAsync("en-US");

        var result = await DictionaryItemService.CreateAsync(
            new DictionaryItem("Testing123")
            {
                Translations = new List<IDictionaryTranslation> { new DictionaryTranslation(english, "Hello parent") }
            },
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var parentKey = result.Result.Key;

        result = await DictionaryItemService.CreateAsync(
            new DictionaryItem("Testing456")
            {
                Translations = new List<IDictionaryTranslation> { new DictionaryTranslation(english, "Hello child") },
                ParentId = parentKey
            },
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // re-get
        var item = await DictionaryItemService.GetAsync(result.Result!.Key);
        Assert.That(item, Is.Not.Null);

        Assert.That(item.Id, Is.GreaterThan(0));
        Assert.That(item.HasIdentity, Is.True);
        Assert.That(item.ParentId.HasValue, Is.True);
        Assert.That(item.ItemKey, Is.EqualTo("Testing456"));
        Assert.That(item.Translations.Count(), Is.EqualTo(1));
        Assert.That(item.ParentId, Is.EqualTo(parentKey));
    }

    [Test]
    public async Task Can_Create_DictionaryItem_At_Root_With_All_Languages()
    {
        var allLangs = (await LanguageService.GetAllAsync()).ToArray();
        Assert.That(allLangs, Is.Not.Empty);

        var translations = allLangs.Select(language => new DictionaryTranslation(language, $"Translation for: {language.IsoCode}")).ToArray();
        var result = await DictionaryItemService.CreateAsync(
            new DictionaryItem("Testing12345") { Translations = translations },
            Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);

        // re-get
        var item = await DictionaryItemService.GetAsync(result.Result!.Key);

        Assert.That(item, Is.Not.Null);
        Assert.That(item.Id, Is.GreaterThan(0));
        Assert.That(item.HasIdentity, Is.True);
        Assert.That(item.ParentId.HasValue, Is.False);
        Assert.That(item.ItemKey, Is.EqualTo("Testing12345"));
        foreach (var language in allLangs)
        {
            Assert.That(
                item.Translations.Single(x => x.LanguageIsoCode == language.IsoCode).Value, Is.EqualTo($"Translation for: {language.IsoCode}"));
        }
    }

    [Test]
    public async Task Can_Create_DictionaryItem_At_Root_With_Some_Languages()
    {
        var allLangs = (await LanguageService.GetAllAsync()).ToArray();
        Assert.That(allLangs, Has.Length.GreaterThan(1));

        var firstLanguage = allLangs.First();
        var translations = new[] { new DictionaryTranslation(firstLanguage, $"Translation for: {firstLanguage.IsoCode}") };
        var result = await DictionaryItemService.CreateAsync(
            new DictionaryItem("Testing12345") { Translations = translations }, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);

        // re-get
        var item = await DictionaryItemService.GetAsync(result.Result!.Key);

        Assert.That(item, Is.Not.Null);
        Assert.That(item.Id, Is.GreaterThan(0));
        Assert.That(item.HasIdentity, Is.True);
        Assert.That(item.ParentId.HasValue, Is.False);
        Assert.That(item.ItemKey, Is.EqualTo("Testing12345"));
        Assert.That(item.Translations.Count(), Is.EqualTo(1));
        Assert.That(item.Translations.First().LanguageIsoCode, Is.EqualTo(firstLanguage.IsoCode));
    }

    [Test]
    public async Task Can_Create_DictionaryItem_With_Explicit_Key()
    {
        var english = await LanguageService.GetAsync("en-US");
        // the package install needs to be able to create dictionary items with explicit keys
        var key = Guid.NewGuid();

        var result = await DictionaryItemService.CreateAsync(
            new DictionaryItem("Testing123")
            {
                Key = key,
                Translations = new List<IDictionaryTranslation> { new DictionaryTranslation(english, "Hello world") }
            },
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result.Key, Is.EqualTo(key));

        // re-get
        var item = await DictionaryItemService.GetAsync(result.Result!.Key);
        Assert.That(item, Is.Not.Null);
        Assert.That(item.Key, Is.EqualTo(key));
    }

    [Test]
    public async Task Can_Add_Translation_To_Existing_Dictionary_Item()
    {
        var english = await LanguageService.GetAsync("en-US");

        var result = await DictionaryItemService.CreateAsync(new DictionaryItem("Testing12345"), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // re-get
        var item = await DictionaryItemService.GetAsync(result.Result!.Key);
        Assert.That(item, Is.Not.Null);

        item.Translations = new List<IDictionaryTranslation> { new DictionaryTranslation(english, "Hello world") };

        result = await DictionaryItemService.UpdateAsync(item, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        Assert.That(item.Translations.Count(), Is.EqualTo(1));
        foreach (var translation in item.Translations)
        {
            Assert.That(translation.Value, Is.EqualTo("Hello world"));
        }

        item.Translations = new List<IDictionaryTranslation>(item.Translations)
        {
            new DictionaryTranslation(
                await LanguageService.GetAsync("en-GB"),
                "My new value")
        };

        result = await DictionaryItemService.UpdateAsync(item, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // re-get
        item = await DictionaryItemService.GetAsync(item.Key);
        Assert.That(item, Is.Not.Null);

        Assert.That(item.Translations.Count(), Is.EqualTo(2));
        Assert.That(item.Translations.First().Value, Is.EqualTo("Hello world"));
        Assert.That(item.Translations.Last().Value, Is.EqualTo("My new value"));
    }

    [Test]
    public async Task Can_Delete_DictionaryItem()
    {
        var item = await DictionaryItemService.GetAsync("Child");
        Assert.That(item, Is.Not.Null);

        var result = await DictionaryItemService.DeleteAsync(item.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.Success));

        var deletedItem = await DictionaryItemService.GetAsync("Child");
        Assert.That(deletedItem, Is.Null);
    }

    [Test]
    public async Task Can_Update_Existing_DictionaryItem()
    {
        var item = await DictionaryItemService.GetAsync("Child");
        foreach (var translation in item.Translations)
        {
            translation.Value += "UPDATED";
        }

        var result = await DictionaryItemService.UpdateAsync(item, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Verify that the create and update dates can be used to distinguish between creates
        // and updates (as these fields are used in ServerEventSender to emit a "Created" or "Updated"
        // event.
        Assert.That(result.Result.UpdateDate, Is.GreaterThan(result.Result.CreateDate));

        var updatedItem = await DictionaryItemService.GetAsync("Child");
        Assert.That(updatedItem, Is.Not.Null);

        foreach (var translation in updatedItem.Translations)
        {
            Assert.That(translation.Value, Does.EndWith("UPDATED"));
        }
    }

    [Test]
    public async Task Can_Move_DictionaryItem_To_Root()
    {
        var rootOneKey = (await DictionaryItemService.CreateAsync(new DictionaryItem("RootOne"), Constants.Security.SuperUserKey)).Result.Key;
        var childKey = (await DictionaryItemService.CreateAsync(new DictionaryItem("ChildOne") { ParentId = rootOneKey }, Constants.Security.SuperUserKey)).Result.Key;

        var child = await DictionaryItemService.GetAsync(childKey);
        Assert.That(child.ParentId, Is.EqualTo(rootOneKey));

        var result = await DictionaryItemService.MoveAsync(child, null, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.Success));

        child = await DictionaryItemService.GetAsync(childKey);
        Assert.That(child.ParentId, Is.EqualTo(null));

        var rootItemKeys = (await DictionaryItemService.GetAtRootAsync()).Select(item => item.Key);
        Assert.That(rootItemKeys, Does.Contain(childKey));

        var rootOneChildren = await DictionaryItemService.GetChildrenAsync(rootOneKey);
        Assert.That(rootOneChildren.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task Can_Move_DictionaryItem_To_Other_Parent()
    {
        var rootOneKey = (await DictionaryItemService.CreateAsync(new DictionaryItem("RootOne"), Constants.Security.SuperUserKey)).Result.Key;
        var rootTwoKey = (await DictionaryItemService.CreateAsync(new DictionaryItem("RootTwo"), Constants.Security.SuperUserKey)).Result.Key;
        var childKey = (await DictionaryItemService.CreateAsync(new DictionaryItem("ChildOne") { ParentId = rootOneKey }, Constants.Security.SuperUserKey)).Result.Key;

        var child = await DictionaryItemService.GetAsync(childKey);
        Assert.That(child.ParentId, Is.EqualTo(rootOneKey));

        var result = await DictionaryItemService.MoveAsync(child, rootTwoKey, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.Success));

        child = await DictionaryItemService.GetAsync(childKey);
        Assert.That(child.ParentId, Is.EqualTo(rootTwoKey));
    }

    [Test]
    public async Task Can_Move_DictionaryItem_From_Root()
    {
        var rootOneKey = (await DictionaryItemService.CreateAsync(new DictionaryItem("RootOne"), Constants.Security.SuperUserKey)).Result.Key;
        var rootTwoKey = (await DictionaryItemService.CreateAsync(new DictionaryItem("RootTwo"), Constants.Security.SuperUserKey)).Result.Key;
        var childKey = (await DictionaryItemService.CreateAsync(new DictionaryItem("ChildOne") { ParentId = rootOneKey }, Constants.Security.SuperUserKey)).Result.Key;

        var rootTwo = await DictionaryItemService.GetAsync(rootTwoKey);
        Assert.That(rootTwo.ParentId, Is.Null);

        var result = await DictionaryItemService.MoveAsync(rootTwo, childKey, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.Success));

        rootTwo = await DictionaryItemService.GetAsync(rootTwoKey);
        Assert.That(rootTwo.ParentId, Is.EqualTo(childKey));

        var rootItemKeys = (await DictionaryItemService.GetAtRootAsync()).Select(item => item.Key);
        Assert.That(rootItemKeys, Does.Not.Contain(rootTwoKey));
    }

    [Test]
    public async Task Cannot_Add_Duplicate_DictionaryItem_ItemKey()
    {
        var item = await DictionaryItemService.GetAsync("Child");
        Assert.That(item, Is.Not.Null);

        item.ItemKey = "Parent";

        var result = await DictionaryItemService.UpdateAsync(item, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.DuplicateItemKey));

        var item2 = await DictionaryItemService.GetAsync("Child");
        Assert.That(item2, Is.Not.Null);
        Assert.That(item2.Key, Is.EqualTo(item.Key));
    }

    [Test]
    public async Task Cannot_Create_Child_DictionaryItem_Under_Missing_Parent()
    {
        var itemKey = Guid.NewGuid().ToString("N");

        var result = await DictionaryItemService.CreateAsync(new DictionaryItem(Guid.NewGuid(), itemKey), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.ParentNotFound));

        var item = await DictionaryItemService.GetAsync(itemKey);
        Assert.That(item, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_Multiple_DictionaryItems_With_Same_ItemKey()
    {
        var itemKey = Guid.NewGuid().ToString("N");
        var result = await DictionaryItemService.CreateAsync(new DictionaryItem(itemKey), Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);

        result = await DictionaryItemService.CreateAsync(new DictionaryItem(itemKey), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.DuplicateItemKey));
    }

    [Test]
    public async Task Cannot_Update_Non_Existant_DictionaryItem()
    {
        var result = await DictionaryItemService.UpdateAsync(new DictionaryItem("NoSuchItemKey"), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.ItemNotFound));
    }

    [Test]
    public async Task Cannot_Update_DictionaryItem_With_Empty_Id()
    {
        var item = await DictionaryItemService.GetAsync("Child");
        Assert.That(item, Is.Not.Null);

        item = new DictionaryItem(item.ParentId, item.ItemKey) { Key = item.Key, Translations = item.Translations };

        var result = await DictionaryItemService.UpdateAsync(item, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.ItemNotFound));
    }

    [Test]
    public async Task Cannot_Delete_Non_Existant_DictionaryItem()
    {
        var result = await DictionaryItemService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.ItemNotFound));
    }

    [Test]
    public async Task Cannot_Create_DictionaryItem_With_Duplicate_Key()
    {
        var english = await LanguageService.GetAsync("en-US");
        var key = Guid.NewGuid();

        var result = await DictionaryItemService.CreateAsync(
            new DictionaryItem("Testing123")
            {
                Key = key,
                Translations = new List<IDictionaryTranslation> { new DictionaryTranslation(english, "Hello world") }
            },
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        result = await DictionaryItemService.CreateAsync(
            new DictionaryItem("Testing456")
            {
                Key = key,
                Translations = new List<IDictionaryTranslation> { new DictionaryTranslation(english, "Hello world") }
            },
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.DuplicateKey));

        // re-get
        var item = await DictionaryItemService.GetAsync("Testing123");
        Assert.That(item, Is.Not.Null);
        Assert.That(item.Key, Is.EqualTo(key));

        item = await DictionaryItemService.GetAsync("Testing456");
        Assert.That(item, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_DictionaryItem_With_Reused_DictionaryItem_Model()
    {
        var childItem = await DictionaryItemService.GetAsync("Child");
        Assert.That(childItem, Is.Not.Null);

        childItem.ItemKey = "Something";
        childItem.Translations.First().Value = "Something Edited";
        childItem.Translations.Last().Value = "Something Also Edited";

        var result = await DictionaryItemService.CreateAsync(childItem, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.InvalidId));
    }

    [Test]
    public async Task Cannot_Move_DictionaryItem_To_Itself()
    {
        var root = (await DictionaryItemService.CreateAsync(new DictionaryItem("RootOne"), Constants.Security.SuperUserKey)).Result;

        var result = await DictionaryItemService.MoveAsync(root, root.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.InvalidParent));

        root = await DictionaryItemService.GetAsync(root.Key);
        Assert.That(root.ParentId, Is.Null);
    }

    [Test]
    public async Task Cannot_Move_DictionaryItem_To_Child()
    {
        var root = (await DictionaryItemService.CreateAsync(new DictionaryItem("RootOne"), Constants.Security.SuperUserKey)).Result;
        var child = (await DictionaryItemService.CreateAsync(new DictionaryItem("ChildOne") { ParentId = root.Key }, Constants.Security.SuperUserKey)).Result;

        var result = await DictionaryItemService.MoveAsync(root, child.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.InvalidParent));

        root = await DictionaryItemService.GetAsync(root.Key);
        Assert.That(root.ParentId, Is.Null);
    }

    [Test]
    public async Task Cannot_Move_DictionaryItem_To_Descendant()
    {
        var root = (await DictionaryItemService.CreateAsync(new DictionaryItem("RootOne"), Constants.Security.SuperUserKey)).Result;
        var child = (await DictionaryItemService.CreateAsync(new DictionaryItem("ChildOne") { ParentId = root.Key }, Constants.Security.SuperUserKey)).Result;
        var grandChild = (await DictionaryItemService.CreateAsync(new DictionaryItem("GrandChildOne") { ParentId = child.Key }, Constants.Security.SuperUserKey)).Result;

        var result = await DictionaryItemService.MoveAsync(root, grandChild.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DictionaryItemOperationStatus.InvalidParent));

        root = await DictionaryItemService.GetAsync(root.Key);
        Assert.That(root.ParentId, Is.Null);
    }

    private async Task CreateTestData()
    {
        var languageDaDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        var languageEnGb = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .Build();

        var languageResult = await LanguageService.CreateAsync(languageDaDk, Constants.Security.SuperUserKey);
        Assert.That(languageResult.Success, Is.True);
        languageResult = await LanguageService.CreateAsync(languageEnGb, Constants.Security.SuperUserKey);
        Assert.That(languageResult.Success, Is.True);

        var dictionaryResult = await DictionaryItemService.CreateAsync(
            new DictionaryItem("Parent")
            {
                Translations = new List<IDictionaryTranslation>
                {
                    new DictionaryTranslation(languageEnGb, "ParentValue"),
                    new DictionaryTranslation(languageDaDk, "ForældreVærdi")
                }
            },
            Constants.Security.SuperUserKey);
        Assert.That(dictionaryResult.Success, Is.True);
        IDictionaryItem parentItem = dictionaryResult.Result!;

        _parentItemId = parentItem.Key;

        dictionaryResult = await DictionaryItemService.CreateAsync(
            new DictionaryItem(
                parentItem.Key,
                "Child")
            {
                Translations = new List<IDictionaryTranslation>
                {
                    new DictionaryTranslation(languageEnGb, "ChildValue"),
                    new DictionaryTranslation(languageDaDk, "BørnVærdi")
                }
            },
            Constants.Security.SuperUserKey);
        Assert.That(dictionaryResult.Success, Is.True);
        IDictionaryItem childItem = dictionaryResult.Result!;

        _childItemId = childItem.Key;
    }
}
