// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering the DataTypeService
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DataTypeServiceTests : UmbracoIntegrationTest
{
    private IDataValueEditorFactory DataValueEditorFactory => GetRequiredService<IDataValueEditorFactory>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

    [Test]
    public async Task Can_Create_New_DataTypeDefinition()
    {
        // Act
        IDataType dataType =
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext
            };
        var result = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.True(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.Success, result.Status);

        // Assert
        Assert.That(dataType, Is.Not.Null);
        Assert.That(dataType.HasIdentity, Is.True);

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.That(dataType, Is.Not.Null);
    }

    [Test]
    public async Task Can_Update_Existing_DataTypeDefinition()
    {
        // Arrange
        IDataType? dataType = (await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox)).FirstOrDefault();
        Assert.NotNull(dataType);

        // Act
        dataType.Name += " UPDATED";
        var result = await DataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.True(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.Success, result.Status);

        // Assert
        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.NotNull(dataType);
        Assert.True(dataType.Name.EndsWith(" UPDATED"));
    }

    [Test]
    public async Task Can_Get_All_By_Editor_Alias()
    {
        // Arrange
        async Task<IDataType> CreateTextBoxDataType()
        {
            IDataType dataType =
                new DataType(new TextboxPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
                {
                    Name = Guid.NewGuid().ToString(),
                    DatabaseType = ValueStorageType.Nvarchar
                };
            var result = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
            Assert.True(result.Success);
            return result.Result;
        }

        IDataType dataType1 = await CreateTextBoxDataType();
        IDataType dataType2 = await CreateTextBoxDataType();
        IDataType dataType3 = await CreateTextBoxDataType();

        // Act
        IEnumerable<IDataType> dataTypes = await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox);

        // Assert
        Assert.True(dataTypes.Count() >= 3);
        Assert.True(dataTypes.All(dataType => dataType.EditorAlias == Constants.PropertyEditors.Aliases.TextBox));
        Assert.NotNull(dataTypes.FirstOrDefault(dataType => dataType.Key == dataType1.Key));
        Assert.NotNull(dataTypes.FirstOrDefault(dataType => dataType.Key == dataType2.Key));
        Assert.NotNull(dataTypes.FirstOrDefault(dataType => dataType.Key == dataType3.Key));
    }

    [Test]
    public async Task Can_Get_By_Id()
    {
        // Arrange
        IDataType? dataType = (await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox)).FirstOrDefault();
        Assert.NotNull(dataType);

        // Act
        IDataType? actual = await DataTypeService.GetAsync(dataType.Key);

        // Assert
        Assert.NotNull(actual);
        Assert.AreEqual(dataType.Key, actual.Key);
    }

    [Test]
    public async Task Can_Get_By_Name()
    {
        // Arrange
        IDataType? dataType = (await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox)).FirstOrDefault();
        Assert.NotNull(dataType);

        // Act
        IDataType? actual = await DataTypeService.GetAsync(dataType.Name);

        // Assert
        Assert.NotNull(actual);
        Assert.AreEqual(dataType.Key, actual.Key);
    }

    [Test]
    public async Task DataTypeService_Can_Delete_DataType_And_Clear_Usages()
    {
        // Arrange
        var dataTypeDefinitions = await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.RichText);
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var doctype =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        ContentTypeService.Save(doctype);

        // validate the assumptions used for assertions later in this test
        var contentType = ContentTypeService.Get(doctype.Id);
        Assert.AreEqual(3, contentType.PropertyTypes.Count());
        Assert.IsNotNull(contentType.PropertyTypes.SingleOrDefault(pt => pt.PropertyEditorAlias is Constants.PropertyEditors.Aliases.RichText));

        // Act
        var definition = dataTypeDefinitions.First();
        var definitionKey = definition.Key;
        var result = await DataTypeService.DeleteAsync(definitionKey, Constants.Security.SuperUserKey);
        Assert.True(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.Success, result.Status);
        Assert.NotNull(result.Result);
        Assert.AreEqual(definitionKey, result.Result.Key);

        var deletedDefinition = await DataTypeService.GetAsync(definitionKey);

        // Assert
        Assert.That(deletedDefinition, Is.Null);

        // Further assertions against the ContentType that contains PropertyTypes based on the TextField
        contentType = ContentTypeService.Get(doctype.Id);
        Assert.That(contentType.Alias, Is.EqualTo("umbTextpage"));
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Can_Create_DataType_In_Container()
    {
        var container = (await DataTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext,
                ParentId = container.Id
            },
            Constants.Security.SuperUserKey);

        Assert.True(result.Success);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(DataTypeOperationStatus.Success, result.Status);

        var dataType = await DataTypeService.GetAsync(result.Result.Key);
        Assert.IsNotNull(dataType);
        Assert.AreEqual(container.Id, dataType.ParentId);
    }

    [Test]
    public async Task Can_Move_DataType_To_Container()
    {
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext
            },
            Constants.Security.SuperUserKey)).Result;

        var container = (await DataTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.IsNotNull(dataType);
        Assert.AreEqual(Constants.System.Root, dataType.ParentId);

        var result = await DataTypeService.MoveAsync(dataType, container.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.Success, result.Status);

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.IsNotNull(dataType);
        Assert.AreEqual(container.Id, dataType.ParentId);
    }

    [Test]
    public async Task Can_Move_DataType_To_Root()
    {
        var container = (await DataTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext,
                ParentId = container.Id
            },
            Constants.Security.SuperUserKey)).Result;

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.IsNotNull(dataType);
        Assert.AreEqual(container.Id, dataType.ParentId);

        var result = await DataTypeService.MoveAsync(dataType, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.Success, result.Status);

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.IsNotNull(dataType);
        Assert.AreEqual(Constants.System.Root, dataType.ParentId);
    }

    [Test]
    public async Task Can_Copy_DataType_To_Root()
    {
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext
            },
            Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeService.CopyAsync(dataType, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.AreNotEqual(dataType.Key, result.Result.Key);
        Assert.AreNotEqual(dataType.Name, result.Result.Name);

        IDataType copy = await DataTypeService.GetAsync(result.Result.Key);
        Assert.IsNotNull(copy);
        Assert.AreEqual(Constants.System.Root, copy.ParentId);
    }

    [Test]
    public async Task Can_Copy_DataType_To_Container()
    {
        var container = (await DataTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext
            },
            Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeService.CopyAsync(dataType, container.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.AreNotEqual(dataType.Key, result.Result.Key);
        Assert.AreNotEqual(dataType.Name, result.Result.Name);

        IDataType copy = await DataTypeService.GetAsync(result.Result.Key);
        Assert.IsNotNull(copy);
        Assert.AreEqual(container.Id, copy.ParentId);
    }

    [Test]
    public async Task Can_Copy_DataType_Between_Containers()
    {
        var container1 = (await DataTypeContainerService.CreateAsync(null, "Root Container 1", null, Constants.Security.SuperUserKey)).Result;
        var container2 = (await DataTypeContainerService.CreateAsync(null, "Root Container 2", null, Constants.Security.SuperUserKey)).Result;
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext,
                ParentId = container1.Id
            },
            Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeService.CopyAsync(dataType, container2.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.AreNotEqual(dataType.Key, result.Result.Key);
        Assert.AreNotEqual(dataType.Name, result.Result.Name);

        IDataType original = await DataTypeService.GetAsync(dataType.Key);
        Assert.IsNotNull(original);
        Assert.AreEqual(container1.Id, original.ParentId);

        IDataType copy = await DataTypeService.GetAsync(result.Result.Key);
        Assert.IsNotNull(copy);
        Assert.AreEqual(container2.Id, copy.ParentId);
    }

    [Test]
    public async Task Can_Copy_DataType_From_Container_To_Root()
    {
        var container1 = (await DataTypeContainerService.CreateAsync(null, "Root Container 1", null, Constants.Security.SuperUserKey)).Result;
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext,
                ParentId = container1.Id
            },
            Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeService.CopyAsync(dataType, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.AreNotEqual(dataType.Key, result.Result.Key);
        Assert.AreNotEqual(dataType.Name, result.Result.Name);

        IDataType original = await DataTypeService.GetAsync(dataType.Key);
        Assert.IsNotNull(original);
        Assert.AreEqual(container1.Id, original.ParentId);

        IDataType copy = await DataTypeService.GetAsync(result.Result.Key);
        Assert.IsNotNull(copy);
        Assert.AreEqual(Constants.System.Root, copy.ParentId);
    }

    [Test]
    public async Task Cannot_Create_DataType_With_Empty_Name()
    {
        // Act
        var dataTypeDefinition =
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = string.Empty,
                DatabaseType = ValueStorageType.Ntext
            };

        // Act
        var result = await DataTypeService.CreateAsync(dataTypeDefinition, Constants.Security.SuperUserKey);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.InvalidName, result.Status);
    }

    [Test]
    public async Task Cannot_Create_DataType_With_Too_Long_Name()
    {
        // Act
        var dataTypeDefinition =
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = new string('a', 256),
                DatabaseType = ValueStorageType.Ntext
            };

        // Act
        var result = await DataTypeService.CreateAsync(dataTypeDefinition, Constants.Security.SuperUserKey);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.InvalidName, result.Status);
    }

    [Test]
    public void Cannot_Save_DataType_With_Empty_Name()
    {
        // Act
        var dataTypeDefinition =
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = string.Empty,
                DatabaseType = ValueStorageType.Ntext
            };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => DataTypeService.Save(dataTypeDefinition));
    }

    [Test]
    public async Task Cannot_Move_DataType_To_Non_Existing_Container()
    {
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext
            },
            Constants.Security.SuperUserKey)).Result;

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.IsNotNull(dataType);
        Assert.AreEqual(Constants.System.Root, dataType.ParentId);

        var result = await DataTypeService.MoveAsync(dataType, Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.ParentNotFound, result.Status);

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.IsNotNull(dataType);
        Assert.AreEqual(Constants.System.Root, dataType.ParentId);
    }

    [TestCase(Constants.DataTypes.Guids.LabelDateTime)]
    [TestCase(Constants.DataTypes.Guids.Textstring)]
    [TestCase(Constants.DataTypes.Guids.Checkbox)]
    [TestCase(Constants.DataTypes.Guids.ListViewContent)]
    [TestCase(Constants.DataTypes.Guids.ListViewMedia)]
    public async Task Cannot_Delete_NonDeletable_DataType(string dataTypeKey)
    {
        var dataType = await DataTypeService.GetAsync(Guid.Parse(dataTypeKey));
        Assert.IsNotNull(dataType);

        var result = await DataTypeService.DeleteAsync(dataType.Key, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.NonDeletable, result.Status);
    }
}
