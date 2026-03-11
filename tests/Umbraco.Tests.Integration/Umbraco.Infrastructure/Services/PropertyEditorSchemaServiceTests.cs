// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
/// Integration tests for the <see cref="IPropertyEditorSchemaService"/>.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PropertyEditorSchemaServiceTests : UmbracoIntegrationTest
{
    private IPropertyEditorSchemaService PropertyEditorSchemaService => GetRequiredService<IPropertyEditorSchemaService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IDataValueEditorFactory DataValueEditorFactory => GetRequiredService<IDataValueEditorFactory>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

    [Test]
    public void SupportsSchema_Returns_True_For_Integer_Editor()
    {
        // Act
        var result = PropertyEditorSchemaService.SupportsSchema(Constants.PropertyEditors.Aliases.Integer);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void SupportsSchema_Returns_True_For_ContentPicker_Editor()
    {
        // Act
        var result = PropertyEditorSchemaService.SupportsSchema(Constants.PropertyEditors.Aliases.ContentPicker);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task GetSchemaAsync_Returns_Success_For_Integer_DataType()
    {
        // Arrange
        var dataType = new DataType(
            new IntegerPropertyEditor(DataValueEditorFactory),
            ConfigurationEditorJsonSerializer)
        {
            Name = "Test Integer GetSchemaAsync",
            DatabaseType = ValueStorageType.Integer,
            ConfigurationData = new Dictionary<string, object>
            {
                { "min", 0 },
                { "max", 100 },
            },
        };
        var createResult = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);

        // Act
        var result = await PropertyEditorSchemaService.GetSchemaAsync(dataType.Key);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(PropertyEditorSchemaOperationStatus.Success));
        Assert.That(result.Result.ValueType, Is.EqualTo(typeof(int?)));
        Assert.That(result.Result.JsonSchema, Is.Not.Null);
        Assert.That(result.Result.JsonSchema!["minimum"]?.GetValue<int>(), Is.EqualTo(0));
        Assert.That(result.Result.JsonSchema["maximum"]?.GetValue<int>(), Is.EqualTo(100));
    }

    [Test]
    public async Task GetSchemaAsync_Returns_DataTypeNotFound_For_NonExistent_DataType()
    {
        // Act
        var result = await PropertyEditorSchemaService.GetSchemaAsync(Guid.NewGuid());

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(PropertyEditorSchemaOperationStatus.DataTypeNotFound));
    }

    [Test]
    public async Task GetSchemaAsync_Returns_SchemaNotSupported_For_Editor_Without_Schema()
    {
        // Arrange - Void editor doesn't implement IValueSchemaProvider
        var dataType = new DataType(
            new VoidEditor(DataValueEditorFactory),
            ConfigurationEditorJsonSerializer)
        {
            Name = "Test Void GetSchemaAsync",
            DatabaseType = ValueStorageType.Nvarchar,
        };
        var createResult = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);

        // Act
        var result = await PropertyEditorSchemaService.GetSchemaAsync(dataType.Key);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(PropertyEditorSchemaOperationStatus.SchemaNotSupported));
    }
}
