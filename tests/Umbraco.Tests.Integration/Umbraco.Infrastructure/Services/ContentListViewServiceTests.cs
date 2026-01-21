using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

internal sealed class ContentListViewServiceTests : ContentListViewServiceTestsBase
{
    private static readonly Guid _customListViewKey = new("AD8E2AAF-6801-408A-8CCF-EFAC0312729B");

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IContentListViewService ContentListViewService => GetRequiredService<IContentListViewService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IUser SuperUser { get; set; }

    [SetUp]
    public async Task Setup()
        => SuperUser = await GetSuperUser();

    [Test]
    public async Task Cannot_Get_List_View_Items_Of_Non_Existing_Content()
    {
        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            Guid.NewGuid(),
            null,
            "updateDate",
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.ContentNotFound, result.Status);
            Assert.IsNull(result.Result);
        });
    }

    [Test]
    public async Task Cannot_Get_List_View_Items_When_Content_Is_Not_Configured_As_List_View()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            Variants = [new () { Name = "Page" }]
        };

        var createResult = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            createResult.Result.Content.Key,
            null,
            "updateDate",
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            // Assert the content type is not configured as list view
            Assert.IsNull(createResult.Result.Content.ContentType.ListView);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.ContentNotCollection, result.Status);
            Assert.IsNull(result.Result);
        });
    }

    [Test]
    public async Task Can_Get_List_View_Items_By_Key()
    {
        // Arrange
        var root = await CreateRootContentWithFiveChildrenAsListViewItems();
        var descendants = ContentService.GetPagedDescendants(root.Id, 0, int.MaxValue, out _);

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            null,
            "updateDate",
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            // Assert the content type is configured as list view
            Assert.IsNotNull(root.ContentType.ListView);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);
        });

        PagedModel<IContent> collectionItemsResult = result.Result.Items;

        Assert.Multiple(() =>
        {
            Assert.AreEqual(5, collectionItemsResult.Total);
            CollectionAssert.AreEquivalent(descendants, collectionItemsResult.Items);
        });
    }

    [Test]
    public async Task Can_Get_Associated_List_View_Configuration_From_Content_Type()
    {
        // Arrange
        var listViewConfiguration = new Dictionary<string, object>
        {
            ["includeProperties"] = new[]
            {
                new Dictionary<string, object> { { "alias", "contentTypeAlias" }, { "isSystem", true } },
                new Dictionary<string, object> { { "alias", "createDate" }, { "isSystem", true } },
                new Dictionary<string, object> { { "alias", "creator" }, { "isSystem", true } },
                new Dictionary<string, object> { { "alias", "published" }, { "isSystem", true } },
                new Dictionary<string, object> { { "alias", "sortOrder" }, { "isSystem", true } },
                new Dictionary<string, object> { { "alias", "updateDate" }, { "isSystem", true } },
                new Dictionary<string, object> { { "alias", "updater" }, { "isSystem", true } },
            },
        };

        var customListView = await CreateCustomListViewDataType(listViewConfiguration);

        var root = await CreateRootContentWithFiveChildrenAsListViewItems(customListView.Key);

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            null,
            "updateDate",
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);

            await AssertListViewConfiguration(result.Result.ListViewConfiguration, customListView.Key);
        });
    }

    [Test]
    public async Task Cannot_Get_Items_With_List_View_Configuration_From_Content_Type_When_List_View_Gets_Deleted()
    {
        // Arrange
        var dataType = new DataTypeBuilder()
            .WithId(0)
            .WithName("Custom list view")
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.ListView)
            .WithConfigurationEditor(new ListViewConfigurationEditor(IOHelper))
            .Done()
            .Build();

        var dataTypeCreateResult = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        // Guard Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(dataTypeCreateResult.Success);
            Assert.IsTrue(dataTypeCreateResult.Result.ConfigurationObject is ListViewConfiguration);
        });

        var listViewKey = dataTypeCreateResult.Result.Key;
        var root = await CreateRootContentWithFiveChildrenAsListViewItems(listViewKey);

        var dataTypeDeleteResult = await DataTypeService.DeleteAsync(listViewKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(dataTypeDeleteResult.Success); // Guard Assert

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            null,
            "updateDate",
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.CollectionNotFound, result.Status);
            Assert.IsNull(result.Result);
        });
    }

    [Test]
    public async Task Cannot_Get_Items_With_List_View_Configuration_From_Content_Type_When_No_Configured_List_View_Properties()
    {
        // Arrange
        // Overwrite default IncludeProperties added by ListViewConfiguration
        var listViewConfiguration = new Dictionary<string, object>
        {
            ["includeProperties"] = Array.Empty<Dictionary<string, object>>(),
        };

        var configurationEditor = new ListViewConfigurationEditor(IOHelper) { DefaultConfiguration = listViewConfiguration };

        var dataType = new DataTypeBuilder()
            .WithId(0)
            .WithName("Custom list view")
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.ListView)
            .WithConfigurationEditor(configurationEditor)
            .Done()
            .Build();

        var dataTypeCreateResult = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        // Guard Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(dataTypeCreateResult.Success);
            Assert.IsTrue(dataTypeCreateResult.Result.ConfigurationObject is ListViewConfiguration);
        });

        var root = await CreateRootContentWithFiveChildrenAsListViewItems(dataTypeCreateResult.Result.Key);

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            null,
            "updateDate",
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.MissingPropertiesInCollectionConfiguration, result.Status);
            Assert.IsNull(result.Result);
        });
    }

    [Test]
    public async Task Can_Get_Items_For_List_View_Property_On_Content()
    {
        // Arrange
        var root = await CreateRootContentWithFiveChildrenAndListViewProperty();
        var descendants = ContentService.GetPagedDescendants(root.Id, 0, int.MaxValue, out _);

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            _customListViewKey,
            "updateDate",
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            // Assert the content type is not configured as list view
            Assert.IsNull(root.ContentType.ListView);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);
        });

        PagedModel<IContent> collectionItemsResult = result.Result.Items;

        Assert.Multiple(() =>
        {
            Assert.AreEqual(5, collectionItemsResult.Total);
            CollectionAssert.AreEquivalent(descendants, collectionItemsResult.Items);
        });
    }

    [Test]
    public async Task Can_Get_Associated_List_View_Configuration_From_List_View_Property_On_Content()
    {
        // Arrange
        var root = await CreateRootContentWithFiveChildrenAndListViewProperty();

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            _customListViewKey,
            "updateDate",
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);

            await AssertListViewConfiguration(result.Result.ListViewConfiguration, _customListViewKey);
        });

    }

    [Test]
    public async Task Cannot_Get_Items_For_List_View_Property_On_Content_When_Data_Type_Is_Not_Content_Type_Property()
    {
        // Arrange
        var root = await CreateRootContentWithFiveChildrenAndListViewProperty();

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            Constants.DataTypes.Guids.ListViewContentGuid,
            "updateDate",
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            // Assert the content type is not configured as list view
            Assert.IsNull(root.ContentType.ListView);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.DataTypeNotContentProperty, result.Status);
            Assert.IsNull(result.Result);
        });
    }

    [Test]
    public async Task Cannot_Get_Items_For_List_View_Property_On_Content_When_Non_Existing_Data_Type()
    {
        // Arrange
        var root = await CreateRootContentWithFiveChildrenAsListViewItems();

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            Guid.NewGuid(),
            "updateDate",
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.DataTypeNotFound, result.Status);
            Assert.IsNull(result.Result);
        });
    }

    [Test]
    public async Task Cannot_Get_Items_For_List_View_Property_On_Content_When_Data_Type_In_Not_List_View()
    {
        // Arrange
        var root = await CreateRootContentWithFiveChildrenAndListViewProperty();

        IDataType? textstringDataType = await DataTypeService.GetAsync(Constants.DataTypes.Guids.TextstringGuid);

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            textstringDataType.Key,
            "updateDate",
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.DataTypeNotCollection, result.Status);
            Assert.IsNull(result.Result);
        });
    }

    [Test]
    public async Task Cannot_Order_List_View_Items_By_Field_Not_Part_Of_Configured_List_View_Properties()
    {
        // Arrange
        var root = await CreateRootContentWithFiveChildrenAsListViewItems();

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            null,
            "testFieldNotPartOfListViewConfiguration",
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.OrderByNotPartOfCollectionConfiguration, result.Status);
            Assert.IsNull(result.Result);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Order_List_View_Items_By_Name(bool orderAscending) // "name" is a special field - never part of configured list view properties
    {
        // Arrange
        var root = await CreateRootContentWithFiveChildrenAsListViewItems();
        var descendants = ContentService.GetPagedDescendants(root.Id, 0, int.MaxValue, out _);

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            null,
            "name",
            null,
            orderAscending ? Direction.Ascending : Direction.Descending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);
        });

        PagedModel<IContent> collectionItemsResult = result.Result.Items;
        var expectedNames = descendants
            .Select(c => c.Name)
            .OrderByDescending(name => orderAscending ? null : name);
        var actualNames = collectionItemsResult.Items.Select(c => c.Name);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(5, collectionItemsResult.Total);
            CollectionAssert.AreEquivalent(descendants, collectionItemsResult.Items);
            Assert.IsTrue(expectedNames.SequenceEqual(actualNames));
        });
    }

    [TestCase("en-US", true, "Child item 1", "Child item 5")]
    [TestCase("en-US", false, "Child item 5", "Child item 1")]
    [TestCase("da-DK", true, "Child item 5", "Child item 1")]
    [TestCase("da-DK", false, "Child item 1", "Child item 5")]
    public async Task Can_Order_List_View_Items_By_Culture(string culture, bool orderAscending, string expectedFirstItemName, string expectedLastItemName)
    {
        // Arrange
        var root = await CreateVariantRootContentWithFiveChildrenAsListViewItems(Constants.DataTypes.Guids.ListViewContentGuid);
        var descendants = ContentService.GetPagedDescendants(root.Id, 0, int.MaxValue, out _);

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            null,
            "name",
            culture,
            orderAscending ? Direction.Ascending : Direction.Descending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);
        });

        PagedModel<IContent> collectionItemsResult = result.Result.Items;

        Assert.Multiple(() =>
        {
            Assert.AreEqual(5, collectionItemsResult.Total);
            CollectionAssert.AreEquivalent(descendants, collectionItemsResult.Items);
            Assert.AreEqual(expectedFirstItemName, collectionItemsResult.Items.First().Name);
            Assert.AreEqual(expectedLastItemName, collectionItemsResult.Items.Last().Name);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Order_List_View_Items_By_System_List_View_Field(bool orderAscending)
    {
        // Arrange
        const string orderByField = "sortOrder";
        var root = await CreateRootContentWithFiveChildrenAndListViewProperty();
        var descendants = ContentService.GetPagedDescendants(root.Id, 0, int.MaxValue, out _);

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            _customListViewKey,
            orderByField,
            null,
            orderAscending ? Direction.Ascending : Direction.Descending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);
        });

        PagedModel<IContent> collectionItemsResult = result.Result.Items;
        ListViewConfiguration collectionConfiguration = result.Result.ListViewConfiguration;
        var expectedNames = descendants
            .OrderByDescending(c => orderAscending ? default : c.SortOrder)
            .Select(c => c.Name);
        var actualNames = collectionItemsResult.Items.Select(c => c.Name);

        var sortOrderProperty = collectionConfiguration.IncludeProperties.FirstOrDefault(p => p.Alias == "sortOrder");

        Assert.Multiple(() =>
        {
            Assert.IsTrue(sortOrderProperty.IsSystem);
            Assert.AreEqual(5, collectionItemsResult.Total);
            CollectionAssert.AreEquivalent(descendants, collectionItemsResult.Items);
            Assert.IsTrue(expectedNames.SequenceEqual(actualNames));
        });
    }

    [Test]
    public async Task Can_Order_List_View_Items_By_Custom_List_View_Field()
    {
        // Arrange
        const string orderByField = "price";
        var root = await CreateRootContentWithFiveChildrenAndListViewProperty();
        var descendants = ContentService.GetPagedDescendants(root.Id, 0, int.MaxValue, out _);

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            _customListViewKey,
            orderByField,
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);
        });

        PagedModel<IContent> collectionItemsResult = result.Result.Items;
        ListViewConfiguration collectionConfiguration = result.Result.ListViewConfiguration;
        var expectedNames = descendants.Select(c => c.Name);
        var actualNames = collectionItemsResult.Items.Select(c => c.Name);

        var priceProperty = collectionConfiguration.IncludeProperties.FirstOrDefault(p => p.Alias == "price");

        Assert.Multiple(() =>
        {
            Assert.IsFalse(priceProperty.IsSystem);
            Assert.AreEqual(5, collectionItemsResult.Total);
            CollectionAssert.AreEquivalent(descendants, collectionItemsResult.Items);
            Assert.IsTrue(expectedNames.SequenceEqual(actualNames));
        });
    }

    [TestCase("00000003-0000-0000-0000-000000000000", "Item 3")]
    [TestCase("Item 3", "Item 3")]
    public async Task Can_Filter_List_View_Items(string filter, string expectedName)
    {
        // Arrange
        var root = await CreateRootContentWithFiveChildrenAsListViewItems();

        var allChildren = ContentService.GetPagedChildren(root.Id, 0, 10, out _).ToArray();

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            null,
            "updateDate",
            null,
            Direction.Ascending,
            filter,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);
        });

        PagedModel<IContent> filteredCollectionItems = result.Result.Items;
        var actualFirst = filteredCollectionItems.Items.First();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(5, allChildren.Length);
            Assert.AreEqual(1, filteredCollectionItems.Total);
            Assert.AreEqual(expectedName, actualFirst.Name);
        });
    }

    [Test]
    public async Task Can_Filter_List_View_Items_By_Random_Key_And_Get_Zero_Matches()
    {
        // Arrange
        var root = await CreateRootContentWithFiveChildrenAsListViewItems();

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            null,
            "updateDate",
            null,
            Direction.Ascending,
            Guid.NewGuid().ToString(),
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);
        });
        Assert.AreEqual(0, result.Result.Items.Total);
    }

    [Test]
    public async Task Cannot_Get_List_View_Items_That_The_User_Does_Not_Have_Access_To()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            Variants = [new () { Name = "Page" }]
        };

        // Content that serves as a start node
        var contentCreateResult = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // New user and user group
        var userGroup = new UserGroupBuilder()
            .WithAlias("test")
            .WithName("Test")
            .WithAllowedSections(new[] { "packages" })
            .WithStartContentId(contentCreateResult.Result.Content.Id)
            .Build();
        var userGroupCreateResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        var userCreateModel = new UserCreateModel
        {
            UserName = "testUser@mail.com",
            Email = "testUser@mail.com",
            Name = "Test user",
            UserGroupKeys = new HashSet<Guid> { userGroupCreateResult.Result.Key },
        };

        var userCreateResult = await UserService.CreateAsync(Constants.Security.SuperUserKey, userCreateModel, true);

        // Content with items that the user doesn't have access to
        var root = await CreateRootContentWithFiveChildrenAsListViewItems();

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            userCreateResult.Result.CreatedUser,
            root.Key,
            null,
            "updateDate",
            null,
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);
        });
        Assert.AreEqual(0, result.Result.Items.Items.Count());
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Order_List_View_Items_By_Date_Property_With_Json_Storage(bool orderAscending)
    {
        // Arrange
        // - create content items with dates intentionally out of order to verify sorting works
        var dates = new[]
        {
            new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero),  // Middle date
            new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero),    // Earliest date
            new DateTimeOffset(2024, 12, 25, 14, 45, 0, TimeSpan.Zero), // Latest date
            new DateTimeOffset(2024, 3, 10, 12, 0, 0, TimeSpan.Zero),  // Second earliest
            new DateTimeOffset(2024, 9, 20, 16, 30, 0, TimeSpan.Zero), // Second latest
        };

        var root = await CreateRootContentWithChildrenHavingDateProperty(dates);

        // Act
        var result = await ContentListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            _customListViewKey,
            "eventDate",
            null,
            orderAscending ? Direction.Ascending : Direction.Descending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);
        });

        PagedModel<IContent> collectionItemsResult = result.Result.Items;

        // Expected chronological order (by date)
        var expectedOrder = orderAscending
            ? new[] { "Event 2", "Event 4", "Event 1", "Event 5", "Event 3" }  // Jan, Mar, Jun, Sep, Dec
            : new[] { "Event 3", "Event 5", "Event 1", "Event 4", "Event 2" }; // Dec, Sep, Jun, Mar, Jan

        var actualOrder = collectionItemsResult.Items.Select(c => c.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(5, collectionItemsResult.Total);
            Assert.AreEqual(expectedOrder.Length, actualOrder.Length);
            CollectionAssert.AreEqual(expectedOrder, actualOrder, $"Expected order: [{string.Join(", ", expectedOrder)}], Actual order: [{string.Join(", ", actualOrder)}]");
        });
    }

    private async Task<IContent> CreateRootContentWithChildrenHavingDateProperty(DateTimeOffset[] dates)
    {
        // Create custom list view configuration that includes our date property.
        var listViewConfiguration = new Dictionary<string, object>
        {
            ["includeProperties"] = new[]
            {
                new Dictionary<string, object> { { "alias", "sortOrder" }, { "isSystem", true } },
                new Dictionary<string, object> { { "alias", "updateDate" }, { "isSystem", true } },
                new Dictionary<string, object> { { "alias", "eventDate" }, { "isSystem", false } },
            },
        };

        var customListView = await CreateCustomListViewDataType(listViewConfiguration);

        // Create a data type for the DateTimeWithTimeZone property editor.
        var dateTimeDataType = await CreateDateTimeWithTimeZoneDataType();

        // Create child content type with a date property.
        var childContentType = new ContentTypeBuilder()
            .WithAlias("event")
            .WithName("Event")
            .AddPropertyType()
                .WithAlias("eventDate")
                .WithName("Event Date")
                .WithDataTypeId(dateTimeDataType.Id)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone)
                .WithValueStorageType(ValueStorageType.Ntext)
                .WithSortOrder(1)
                .Done()
            .Build();

        var createdContentTypeResult = await ContentTypeService.CreateAsync(childContentType, Constants.Security.SuperUserKey);
        Assert.IsTrue(createdContentTypeResult.Success);
        Assert.IsTrue(createdContentTypeResult.Success, "Failed to create child content type.");

        // Create root content type with list view property
        var contentTypeWithListViewPropertyType = new ContentTypeBuilder()
            .WithAlias("events")
            .WithName("Events")
            .WithContentVariation(ContentVariation.Nothing)
            .AddPropertyType()
                .WithAlias("items")
                .WithName("Items")
                .WithDataTypeId(customListView.Id)
                .WithPropertyEditorAlias(customListView.EditorAlias)
                .Done()
            .Build();

        contentTypeWithListViewPropertyType.AllowedAsRoot = true;
        contentTypeWithListViewPropertyType.AllowedContentTypes = new[]
        {
            new ContentTypeSort(childContentType.Key, 1, childContentType.Alias),
        };
        createdContentTypeResult = await ContentTypeService.CreateAsync(contentTypeWithListViewPropertyType, Constants.Security.SuperUserKey);
        Assert.IsTrue(createdContentTypeResult.Success, "Failed to create root content type.");

        // Create root content
        var rootContentCreateModel = new ContentCreateModel
        {
            ContentTypeKey = contentTypeWithListViewPropertyType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new () { Name = "Events" }]
        };

        var rootResult = await ContentEditingService.CreateAsync(rootContentCreateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(rootResult.Success, "Failed to create root content.");
        var root = rootResult.Result.Content;

        // Create child content items with date values.
        for (var i = 0; i < dates.Length; i++)
        {
            // Pass the value in editor format (DateTimeEditorValue serialized as JSON).
            // The DateTimeDataValueEditor.FromEditor will convert it to storage format.
            var editorValue = new DateTimeEditorValue
            {
                Date = dates[i].ToString("O"), // ISO 8601 format.
            };
            var jsonEditorValue = JsonSerializer.Serialize(editorValue);

            var createModel = new ContentCreateModel
            {
                ContentTypeKey = childContentType.Key,
                ParentKey = root.Key,
                Variants = [new() { Name = $"Event {i + 1}" }],
                Key = (i + 1).ToGuid(),
                Properties =
                [
                    new PropertyValueModel { Alias = "eventDate", Value = jsonEditorValue }
                ],
            };

            await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        }

        return root;
    }

    private async Task<IDataType> CreateDateTimeWithTimeZoneDataType()
    {
        // Get the real DateTimeWithTimeZone property editor from the collection
        // This ensures we use the editor that implements IDataValueSortable
        var propertyEditor = PropertyEditorCollection[Constants.PropertyEditors.Aliases.DateTimeWithTimeZone];
        Assert.IsNotNull(propertyEditor, "DateTimeWithTimeZone property editor not found in collection");

        var serializer = GetRequiredService<IConfigurationEditorJsonSerializer>();
        var dataType = new DataType(propertyEditor, serializer)
        {
            Name = "DateTime With TimeZone (Test)",
            DatabaseType = ValueStorageType.Ntext,
        };

        var result = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create data type: {result.Status}");
        return result.Result;
    }

    private async Task<IDataType> CreateCustomListViewDataType(IDictionary<string, object> listViewConfiguration)
    {
        // Overwrite default IncludeProperties added by ListViewConfiguration
        var configurationEditor = new ListViewConfigurationEditor(IOHelper) { DefaultConfiguration = listViewConfiguration };

        var dataType = new DataTypeBuilder()
            .WithId(0)
            .WithKey(_customListViewKey)
            .WithName("Custom list view")
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.ListView)
            .WithConfigurationEditor(configurationEditor)
            .Done()
            .Build();

        var result = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Result.ConfigurationObject is ListViewConfiguration);
        });

        return result.Result;
    }

    private async Task<IContent> CreateRootContentWithFiveChildrenAndListViewProperty()
    {
        var listViewConfiguration = new Dictionary<string, object>
        {
            ["includeProperties"] = new[]
            {
                new Dictionary<string, object> { { "alias", "sortOrder" }, { "isSystem", true } },
                new Dictionary<string, object> { { "alias", "updateDate" }, { "isSystem", true } },
                new Dictionary<string, object> { { "alias", "owner" }, { "isSystem", true } },
                new Dictionary<string, object> { { "alias", "itemName" }, { "isSystem", false } },
                new Dictionary<string, object> { { "alias", "price" }, { "isSystem", false } },
            },
        };

        var customListView = await CreateCustomListViewDataType(listViewConfiguration);

        var childContentType = new ContentTypeBuilder()
            .WithAlias("product")
            .WithName("Product")
            .AddPropertyType()
            .WithAlias("itemName")
            .WithName("Item Name")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithSortOrder(1)
            .Done()
            .AddPropertyType()
            .WithAlias("price")
            .WithName("Price")
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Integer)
            .WithValueStorageType(ValueStorageType.Integer)
            .WithSortOrder(2)
            .Done()
            .Build();

        ContentTypeService.Save(childContentType);

        var contentTypeWithListViewPropertyType = new ContentTypeBuilder()
            .WithAlias("products")
            .WithName("Products")
            .WithContentVariation(ContentVariation.Nothing)
            .AddPropertyType()
            .WithAlias("items")
            .WithName("Items")
            .WithDataTypeId(customListView.Id)
            .WithPropertyEditorAlias(customListView.EditorAlias)
            .Done()
            .Build();

        contentTypeWithListViewPropertyType.AllowedAsRoot = true;
        contentTypeWithListViewPropertyType.AllowedContentTypes = new[]
        {
            new ContentTypeSort(childContentType.Key, 1, childContentType.Alias),
        };
        ContentTypeService.Save(contentTypeWithListViewPropertyType);

        var rootContentCreateModel = new ContentCreateModel
        {
            ContentTypeKey = contentTypeWithListViewPropertyType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new () { Name = "Products" }]
        };

        var result = await ContentEditingService.CreateAsync(rootContentCreateModel, Constants.Security.SuperUserKey);
        var root = result.Result.Content;

        for (var i = 1; i < 6; i++)
        {
            var createModel = new ContentCreateModel
            {
                ContentTypeKey = childContentType.Key,
                ParentKey = root.Key,
                Variants = [new () { Name = $"Item {i}" }],
                Key = i.ToGuid(),
                Properties =
                [
                    new PropertyValueModel { Alias = "itemName", Value = $"Item {i}" },
                    new PropertyValueModel { Alias = "price", Value = i * 10 }
                ],
            };

            await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        }

        return root;
    }

    private async Task<IContent> CreateRootContentWithFiveChildrenAsListViewItems(Guid? listViewDataTypeKey = null)
    {
        var childContentType = new ContentTypeBuilder()
            .WithAlias("product")
            .WithName("Product")
            .Build();
        ContentTypeService.Save(childContentType);

        var contentTypeWithListView = new ContentTypeBuilder()
            .WithAlias("products")
            .WithName("Products")
            .WithContentVariation(ContentVariation.Nothing)
            .WithIsContainer(listViewDataTypeKey ?? Constants.DataTypes.Guids.ListViewContentGuid)
            .Build();

        contentTypeWithListView.AllowedAsRoot = true;
        contentTypeWithListView.AllowedContentTypes = new[]
        {
            new ContentTypeSort(childContentType.Key, 1, childContentType.Alias),
        };
        ContentTypeService.Save(contentTypeWithListView);

        var rootContentCreateModel = new ContentCreateModel
        {
            ContentTypeKey = contentTypeWithListView.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new () { Name = "Products" }]
        };

        var result = await ContentEditingService.CreateAsync(rootContentCreateModel, Constants.Security.SuperUserKey);
        var root = result.Result.Content;

        for (var i = 1; i < 6; i++)
        {
            var createModel = new ContentCreateModel
            {
                ContentTypeKey = childContentType.Key,
                ParentKey = root.Key,
                Variants = [new () { Name = $"Item {i}" }],
                Key = i.ToGuid(),
            };

            await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        }

        return root;
    }

    private async Task<IContent> CreateVariantRootContentWithFiveChildrenAsListViewItems(Guid? listViewDataTypeKey = null)
    {
        var language = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);

        var childContentType = new ContentTypeBuilder()
            .WithAlias("product")
            .WithName("Product")
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        ContentTypeService.Save(childContentType);

        var contentTypeWithListView = new ContentTypeBuilder()
            .WithAlias("products")
            .WithName("Products")
            .WithContentVariation(ContentVariation.Culture)
            .WithIsContainer(listViewDataTypeKey ?? Constants.DataTypes.Guids.ListViewContentGuid)
            .Build();

        contentTypeWithListView.AllowedAsRoot = true;
        contentTypeWithListView.AllowedContentTypes = new[]
        {
            new ContentTypeSort(childContentType.Key, 1, childContentType.Alias),
        };
        ContentTypeService.Save(contentTypeWithListView);

        var rootContentCreateModel = new ContentCreateModel
        {
            ContentTypeKey = contentTypeWithListView.Key,
            Properties = [],
            Variants = new[]
            {
                new VariantModel
                {
                    Culture = "en-US",
                    Name = "English Page",
                },
                new VariantModel
                {
                    Culture = "da-DK",
                    Name = "Danish Page",
                },
            },
        };

        var result = await ContentEditingService.CreateAsync(rootContentCreateModel, Constants.Security.SuperUserKey);
        var root = result.Result.Content;

        for (var i = 1; i < 6; i++)
        {
            var createModel = new ContentCreateModel
            {
                ContentTypeKey = childContentType.Key,
                ParentKey = root.Key,
                Properties = [],
                Variants = new[]
                {
                    new VariantModel
                    {
                        Culture = "en-US",
                        Name = $"Child item {i}",
                    },
                    new VariantModel
                    {
                        Culture = "da-DK",
                        Name = $"(DA) Child item {5 - i}",
                    },
                },
            };

            await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        }

        return root;
    }
}
