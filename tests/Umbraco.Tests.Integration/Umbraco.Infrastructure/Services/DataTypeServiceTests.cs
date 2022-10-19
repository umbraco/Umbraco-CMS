// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
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

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

    [Test]
    public void DataTypeService_Can_Persist_New_DataTypeDefinition()
    {
        // Act
        IDataType dataType =
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext
            };
        DataTypeService.Save(dataType);

        // Assert
        Assert.That(dataType, Is.Not.Null);
        Assert.That(dataType.HasIdentity, Is.True);

        dataType = DataTypeService.GetDataType(dataType.Id);
        Assert.That(dataType, Is.Not.Null);
    }

    [Test]
    public void DataTypeService_Can_Delete_Textfield_DataType_And_Clear_Usages()
    {
        // Arrange
        var textfieldId = "Umbraco.Textbox";
        var dataTypeDefinitions = DataTypeService.GetByEditorAlias(textfieldId);
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var doctype =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        ContentTypeService.Save(doctype);

        // Act
        var definition = dataTypeDefinitions.First();
        var definitionId = definition.Id;
        DataTypeService.Delete(definition);

        var deletedDefinition = DataTypeService.GetDataType(definitionId);

        // Assert
        Assert.That(deletedDefinition, Is.Null);

        // Further assertions against the ContentType that contains PropertyTypes based on the TextField
        var contentType = ContentTypeService.Get(doctype.Id);
        Assert.That(contentType.Alias, Is.EqualTo("umbTextpage"));
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(1));
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
}
