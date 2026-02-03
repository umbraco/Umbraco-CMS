// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Relations;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal class TrackedReferencesServiceElementTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementService ElementService => GetRequiredService<IElementService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

    private IElement Element1 { get; set; } = null!;

    private IElement Element2 { get; set; } = null!;

    private IElement Element3 { get; set; } = null!;

    private EntityContainer Folder1 { get; set; } = null!;

    private IElement ElementInFolder { get; set; } = null!;

    private IContentType ElementType { get; set; } = null!;

    private IDataType ElementPickerDataType { get; set; } = null!;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder
            .AddNotificationHandler<ElementSavedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ElementPublishedNotification, ContentRelationsUpdate>();
    }

    [SetUp]
    public async Task Setup() => await CreateTestDataAsync();

    [Test]
    public async Task Get_Elements_That_Reference_This()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedRelationsForItemAsync(Element1.Key, UmbracoObjectTypes.Element, 0, 10, true);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(actual.Success);
            Assert.AreEqual(1, actual.Result.Total);
            var item = actual.Result.Items.First();
            Assert.AreEqual(ElementType.Alias, item.ContentTypeAlias);
            Assert.AreEqual(Element3.Key, item.NodeKey);
        });
    }

    [Test]
    public async Task Get_Relations_For_Non_Existing_Element_Returns_Not_Found()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedRelationsForItemAsync(
            Guid.NewGuid(),
            UmbracoObjectTypes.Element,
            0,
            10,
            true);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(actual.Success);
            Assert.AreEqual(GetReferencesOperationStatus.ContentNotFound, actual.Status);
        });
    }

    [Test]
    public async Task Does_Not_Return_References_If_Element_Is_Not_Referenced()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedRelationsForItemAsync(
            Element3.Key,
            UmbracoObjectTypes.Element,
            0,
            10,
            true);

        Assert.IsTrue(actual.Success);
        Assert.AreEqual(0, actual.Result.Total);
    }

    [Test]
    public async Task Get_Elements_That_Reference_Recycle_Bin_Contents()
    {
        await ElementEditingService.MoveToRecycleBinAsync(Element1.Key, Constants.Security.SuperUserKey);

        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedRelationsForRecycleBinAsync(
            UmbracoObjectTypes.Element,
            0,
            10,
            true);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, actual.Total);
            var item = actual.Items.First();
            Assert.AreEqual(ElementType.Alias, item.ContentTypeAlias);
            Assert.AreEqual(Element3.Key, item.NodeKey);
        });
    }

    [Test]
    public async Task Are_Referenced_Returns_Referenced_Element_Keys()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        // Check if Element1 and Element2 are referenced (they are by Element3)
        // Also include Element3 which is NOT referenced
        var keysToCheck = new HashSet<Guid> { Element1.Key, Element2.Key, Element3.Key };

        var actual = await sut.GetPagedKeysWithDependentReferencesAsync(
            keysToCheck,
            Constants.ObjectTypes.Element,
            0,
            10);

        Assert.Multiple(() =>
        {
            // Element1 and Element2 should be returned as they are referenced
            Assert.AreEqual(2, actual.Total);
            Assert.IsTrue(actual.Items.Contains(Element1.Key));
            Assert.IsTrue(actual.Items.Contains(Element2.Key));

            // Element3 should NOT be in the result as it is not referenced
            Assert.IsFalse(actual.Items.Contains(Element3.Key));
        });
    }

    [Test]
    public async Task Element_In_Folder_Is_Referenced_By_Element3()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        // Verify that ElementInFolder is referenced by Element3
        var actual = await sut.GetPagedRelationsForItemAsync(
            ElementInFolder.Key,
            UmbracoObjectTypes.Element,
            0,
            10,
            true);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(actual.Success);
            Assert.AreEqual(1, actual.Result.Total);
            var item = actual.Result.Items.First();
            Assert.AreEqual(Element3.Key, item.NodeKey);
        });
    }

    [Test]
    public async Task Get_Descendants_In_References_For_Existing_Folder_Returns_Expected_Results()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedDescendantsInReferencesAsync(
            Folder1.Key,
            UmbracoObjectTypes.ElementContainer,
            0,
            10,
            true);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(actual.Success);
            Assert.AreEqual(GetReferencesOperationStatus.Success, actual.Status);

            var itemKeys = actual.Result.Items.Select(x => x.NodeKey).ToList();

            // Should *only* return the element inside the folder that is referenced
            Assert.AreEqual(1, itemKeys.Count);
            Assert.AreEqual(ElementInFolder.Key, itemKeys[0]);
        });
    }

    [Test]
    public async Task Get_Descendants_In_References_For_Non_Existing_Folder_Returns_Not_Found()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedDescendantsInReferencesAsync(
            Guid.NewGuid(),
            UmbracoObjectTypes.ElementContainer,
            0,
            10,
            true);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(actual.Success);
            Assert.AreEqual(GetReferencesOperationStatus.ContentNotFound, actual.Status);
        });
    }

    private async Task CreateTestDataAsync()
    {
        // Create ElementPicker data type
        var elementPickerEditor = GetRequiredService<ElementPickerPropertyEditor>();
        ElementPickerDataType = new DataType(elementPickerEditor, ConfigurationEditorJsonSerializer)
        {
            Name = "Element Picker",
            DatabaseType = ValueStorageType.Ntext,
        };
        await DataTypeService.CreateAsync(ElementPickerDataType, Constants.Security.SuperUserKey);

        // Create element type with element picker property
        ElementType = new ContentTypeBuilder()
            .WithName("TestElement")
            .WithAlias("testElement")
            .WithIsElement(true)
            .AddPropertyType()
                .WithAlias("elementPicker")
                .WithName("ElementPicker")
                .WithDataTypeId(ElementPickerDataType.Id)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ElementPicker)
                .Done()
            .AddPropertyType()
                .WithAlias("elementPicker2")
                .WithName("ElementPicker2")
                .WithDataTypeId(ElementPickerDataType.Id)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ElementPicker)
                .Done()
            .Build();

        await ContentTypeService.CreateAsync(ElementType, Constants.Security.SuperUserKey);

        // Create Element1 (will be referenced by Element3)
        Element1 = ElementService.Create("Element 1", ElementType.Alias);
        ElementService.Save(Element1);
        ElementService.Publish(Element1, ["*"]);

        // Create Element2 (will be referenced by Element3)
        Element2 = ElementService.Create("Element 2", ElementType.Alias);
        ElementService.Save(Element2);
        ElementService.Publish(Element2, ["*"]);

        // Create a folder with an element inside it
        var folderResult = await ElementContainerService.CreateAsync(
            null,
            "Folder 1",
            null,
            Constants.Security.SuperUserKey);
        Folder1 = folderResult.Result!;

        // Create an element directly inside the folder using ElementCreateModel with ParentKey
        var createModel = new ElementCreateModel
        {
            ContentTypeKey = ElementType.Key,
            ParentKey = Folder1.Key,
            Variants = [new VariantModel { Name = "Element In Folder" }],
        };
        var createResult = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success, $"Create failed with status: {createResult.Status}");
        ElementInFolder = createResult.Result.Content!;
        ElementService.Publish(ElementInFolder, ["*"]);

        // Create Element3 that references Element1, Element2 and the element in the folder
        Element3 = ElementService.Create("Element 3", ElementType.Alias);
        Element3.SetValue("elementPicker", $"[\"{Element1.Key}\", \"{ElementInFolder.Key}\"]");
        Element3.SetValue("elementPicker2", $"[\"{Element2.Key}\"]");
        ElementService.Save(Element3);
        ElementService.Publish(Element3, ["*"]);
    }
}
