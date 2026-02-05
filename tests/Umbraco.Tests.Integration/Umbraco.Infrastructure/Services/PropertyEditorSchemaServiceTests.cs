// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
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
    public async Task GetValueSchemaAsync_Returns_Schema_For_Integer_DataType()
    {
        // Arrange
        var dataType = new DataType(
            new IntegerPropertyEditor(DataValueEditorFactory),
            ConfigurationEditorJsonSerializer)
        {
            Name = "Test Integer",
            DatabaseType = ValueStorageType.Integer,
            ConfigurationData = new Dictionary<string, object>
            {
                { "min", 0 },
                { "max", 100 },
                { "step", 1 },
            },
        };
        var createResult = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);

        // Act
        var schema = await PropertyEditorSchemaService.GetValueSchemaAsync(dataType.Key);

        // Assert
        Assert.That(schema, Is.Not.Null);
        Assert.That(schema!["$schema"]?.GetValue<string>(), Is.EqualTo("https://json-schema.org/draft/2020-12/schema"));
        Assert.That(schema["minimum"]?.GetValue<int>(), Is.EqualTo(0));
        Assert.That(schema["maximum"]?.GetValue<int>(), Is.EqualTo(100));
    }

    [Test]
    public async Task GetValueTypeAsync_Returns_Type_For_Integer_DataType()
    {
        // Arrange
        var dataType = new DataType(
            new IntegerPropertyEditor(DataValueEditorFactory),
            ConfigurationEditorJsonSerializer)
        {
            Name = "Test Integer Type",
            DatabaseType = ValueStorageType.Integer,
        };
        var createResult = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);

        // Act
        var valueType = await PropertyEditorSchemaService.GetValueTypeAsync(dataType.Key);

        // Assert
        Assert.That(valueType, Is.EqualTo(typeof(int?)));
    }

    [Test]
    public async Task GetValueSchemaAsync_Returns_Null_For_NonExistent_DataType()
    {
        // Act
        var schema = await PropertyEditorSchemaService.GetValueSchemaAsync(Guid.NewGuid());

        // Assert
        Assert.That(schema, Is.Null);
    }

    [Test]
    public async Task ValidateValueAsync_Returns_Empty_For_Valid_Integer_Value()
    {
        // Arrange
        var dataType = new DataType(
            new IntegerPropertyEditor(DataValueEditorFactory),
            ConfigurationEditorJsonSerializer)
        {
            Name = "Test Integer Validation",
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
        var results = await PropertyEditorSchemaService.ValidateValueAsync(dataType.Key, "50");

        // Assert
        Assert.That(results, Is.Empty);
    }

    [Test]
    public async Task ValidateValueAsync_Returns_Errors_For_Out_Of_Range_Integer()
    {
        // Arrange
        var dataType = new DataType(
            new IntegerPropertyEditor(DataValueEditorFactory),
            ConfigurationEditorJsonSerializer)
        {
            Name = "Test Integer Range Validation",
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
        var results = await PropertyEditorSchemaService.ValidateValueAsync(dataType.Key, "150");

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(r => r.Keyword == "maximum"), Is.True);
    }

    [Test]
    public async Task ValidateValueAsync_Returns_Errors_For_Invalid_Type()
    {
        // Arrange
        var dataType = new DataType(
            new IntegerPropertyEditor(DataValueEditorFactory),
            ConfigurationEditorJsonSerializer)
        {
            Name = "Test Integer Type Validation",
            DatabaseType = ValueStorageType.Integer,
        };
        var createResult = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);

        // Act
        var results = await PropertyEditorSchemaService.ValidateValueAsync(dataType.Key, "\"not an integer\"");

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(r => r.Keyword == "type"), Is.True);
    }

    [Test]
    public async Task ValidateValueAsync_Returns_Empty_For_Null_Value_When_Nullable()
    {
        // Arrange
        var dataType = new DataType(
            new IntegerPropertyEditor(DataValueEditorFactory),
            ConfigurationEditorJsonSerializer)
        {
            Name = "Test Integer Null Validation",
            DatabaseType = ValueStorageType.Integer,
        };
        var createResult = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);

        // Act
        var results = await PropertyEditorSchemaService.ValidateValueAsync(dataType.Key, "null");

        // Assert
        Assert.That(results, Is.Empty);
    }

    [Test]
    public async Task ValidateValueAsync_Returns_Empty_For_NonExistent_DataType()
    {
        // Act
        var results = await PropertyEditorSchemaService.ValidateValueAsync(Guid.NewGuid(), "any value");

        // Assert - No schema means validation passes by default
        Assert.That(results, Is.Empty);
    }
}
