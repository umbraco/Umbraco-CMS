// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V19_0_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MigrateTypedLabelDataTypesTests : UmbracoIntegrationTest
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer
        => GetRequiredService<IConfigurationEditorJsonSerializer>();

    [TestCase(ValueTypes.Integer, Constants.PropertyEditors.Aliases.LabelInteger, ValueStorageType.Integer)]
    [TestCase(ValueTypes.Bigint, Constants.PropertyEditors.Aliases.LabelBigInt, ValueStorageType.Nvarchar)]
    [TestCase(ValueTypes.Decimal, Constants.PropertyEditors.Aliases.LabelDecimal, ValueStorageType.Decimal)]
    [TestCase(ValueTypes.DateTime, Constants.PropertyEditors.Aliases.LabelDateTime, ValueStorageType.Date)]
    [TestCase(ValueTypes.Date, Constants.PropertyEditors.Aliases.LabelDateTime, ValueStorageType.Date)]
    [TestCase(ValueTypes.Time, Constants.PropertyEditors.Aliases.LabelTime, ValueStorageType.Date)]
    [TestCase(ValueTypes.Text, Constants.PropertyEditors.Aliases.LabelText, ValueStorageType.Ntext)]
    [TestCase(ValueTypes.Json, Constants.PropertyEditors.Aliases.LabelText, ValueStorageType.Ntext)]
    [TestCase(ValueTypes.Xml, Constants.PropertyEditors.Aliases.LabelText, ValueStorageType.Ntext)]
    public async Task Moves_Label_Onto_The_Editor_For_Its_Configured_Value_Type(
        string valueType,
        string expectedEditorAlias,
        ValueStorageType expectedStorageType)
    {
        IDataType dataType = await CreateLabelDataType(valueType);

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.Multiple(() =>
        {
            Assert.That(migrated.EditorAlias, Is.EqualTo(expectedEditorAlias));
            Assert.That(migrated.EditorUiAlias, Is.EqualTo($"Umb.PropertyEditorUi.Label.{expectedEditorAlias.Split('.').Last()}"));
            Assert.That(migrated.DatabaseType, Is.EqualTo(expectedStorageType));
            Assert.That(
                migrated.ConfigurationData.ContainsKey(Constants.PropertyEditors.ConfigurationKeys.DataValueType),
                Is.False);
        });
    }

    [TestCase(ValueTypes.String)]
    [TestCase(null)]
    public async Task Leaves_A_String_Label_On_The_Label_Editor(string? valueType)
    {
        IDataType dataType = await CreateLabelDataType(valueType);

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.Multiple(() =>
        {
            Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.Label));
            Assert.That(migrated.DatabaseType, Is.EqualTo(ValueStorageType.Nvarchar));
            Assert.That(
                migrated.ConfigurationData.ContainsKey(Constants.PropertyEditors.ConfigurationKeys.DataValueType),
                Is.False);
        });
    }

    [Test]
    public async Task Keeps_Configuration_Other_Than_The_Value_Type()
    {
        // The built-in "Label (pixels)" and "Label (bytes)" data types carry a label template alongside the value
        // type, and only the value type is redundant.
        IDataType dataType = await CreateLabelDataType(
            ValueTypes.Integer,
            additionalConfiguration: new Dictionary<string, object> { ["labelTemplate"] = "{=value}px" });

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.Multiple(() =>
        {
            Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.LabelInteger));
            Assert.That(migrated.ConfigurationData["labelTemplate"], Is.EqualTo("{=value}px"));
            Assert.That(
                migrated.ConfigurationData.ContainsKey(Constants.PropertyEditors.ConfigurationKeys.DataValueType),
                Is.False);
        });
    }

    [Test]
    public async Task Is_Idempotent()
    {
        IDataType dataType = await CreateLabelDataType(ValueTypes.Integer);

        await ExecuteMigration();
        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.Multiple(() =>
        {
            Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.LabelInteger));
            Assert.That(migrated.DatabaseType, Is.EqualTo(ValueStorageType.Integer));
        });
    }

    [Test]
    public async Task Seeds_The_Built_In_Label_Data_Types_On_The_Typed_Editors()
    {
        // A clean install seeds these already on the typed editors, so this covers DatabaseDataCreator rather than
        // the migration - see Moves_The_Built_In_Label_Data_Types_From_The_Legacy_Shape for the upgrade path. They
        // back the built-in media and member properties, so neither route may leave them behind.
        Assert.Multiple(async () =>
        {
            Assert.That(
                (await DataTypeService.GetAsync(Constants.DataTypes.Guids.LabelIntGuid))?.EditorAlias,
                Is.EqualTo(Constants.PropertyEditors.Aliases.LabelInteger));
            Assert.That(
                (await DataTypeService.GetAsync(Constants.DataTypes.Guids.LabelBigIntGuid))?.EditorAlias,
                Is.EqualTo(Constants.PropertyEditors.Aliases.LabelBigInt));
            Assert.That(
                (await DataTypeService.GetAsync(Constants.DataTypes.Guids.LabelDateTimeGuid))?.EditorAlias,
                Is.EqualTo(Constants.PropertyEditors.Aliases.LabelDateTime));
            Assert.That(
                (await DataTypeService.GetAsync(Constants.DataTypes.Guids.LabelTimeGuid))?.EditorAlias,
                Is.EqualTo(Constants.PropertyEditors.Aliases.LabelTime));
            Assert.That(
                (await DataTypeService.GetAsync(Constants.DataTypes.Guids.LabelDecimalGuid))?.EditorAlias,
                Is.EqualTo(Constants.PropertyEditors.Aliases.LabelDecimal));
            Assert.That(
                (await DataTypeService.GetAsync(Constants.DataTypes.Guids.LabelStringGuid))?.EditorAlias,
                Is.EqualTo(Constants.PropertyEditors.Aliases.Label));
        });
    }

    [Test]
    public async Task Moves_The_Built_In_Label_Data_Types_From_The_Legacy_Shape()
    {
        // What an upgraded site looks like: every built-in label sits on Umbraco.Label, with the value type in its
        // configuration. Put them back that way, so the migration is what moves them rather than the seed.
        var legacyShape = new (Guid Key, string ValueType, string ExpectedEditorAlias)[]
        {
            (Constants.DataTypes.Guids.LabelIntGuid, ValueTypes.Integer, Constants.PropertyEditors.Aliases.LabelInteger),
            (Constants.DataTypes.Guids.LabelBigIntGuid, ValueTypes.Bigint, Constants.PropertyEditors.Aliases.LabelBigInt),
            (Constants.DataTypes.Guids.LabelDateTimeGuid, ValueTypes.DateTime, Constants.PropertyEditors.Aliases.LabelDateTime),
            (Constants.DataTypes.Guids.LabelTimeGuid, ValueTypes.Time, Constants.PropertyEditors.Aliases.LabelTime),
            (Constants.DataTypes.Guids.LabelDecimalGuid, ValueTypes.Decimal, Constants.PropertyEditors.Aliases.LabelDecimal),
            (new Guid(Constants.DataTypes.Guids.LabelBytes), ValueTypes.Bigint, Constants.PropertyEditors.Aliases.LabelBigInt),
            (new Guid(Constants.DataTypes.Guids.LabelPixels), ValueTypes.Integer, Constants.PropertyEditors.Aliases.LabelInteger),
        };

        foreach ((Guid key, var valueType, _) in legacyShape)
        {
            await RevertToLegacyShape(key, valueType);
        }

        await ExecuteMigration();

        Assert.Multiple(async () =>
        {
            foreach ((Guid key, var valueType, var expectedEditorAlias) in legacyShape)
            {
                IDataType? migrated = await DataTypeService.GetAsync(key);
                Assert.That(migrated?.EditorAlias, Is.EqualTo(expectedEditorAlias), $"for {key} ({valueType})");
                Assert.That(
                    migrated?.ConfigurationData.ContainsKey(Constants.PropertyEditors.ConfigurationKeys.DataValueType),
                    Is.False,
                    $"for {key} ({valueType})");
            }
        });
    }

    private async Task RevertToLegacyShape(Guid key, string valueType)
    {
        IDataType dataType = (await DataTypeService.GetAsync(key))!;
        dataType.Editor = PropertyEditors.First(e => e.Alias == Constants.PropertyEditors.Aliases.Label);
        dataType.EditorUiAlias = "Umb.PropertyEditorUi.Label";

        var configuration = new Dictionary<string, object>(dataType.ConfigurationData)
        {
            [Constants.PropertyEditors.ConfigurationKeys.DataValueType] = valueType,
        };
        dataType.ConfigurationData = configuration;

        await DataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
    }

    private async Task<IDataType> CreateLabelDataType(
        string? valueType,
        IDictionary<string, object>? additionalConfiguration = null)
    {
        IDataEditor editor = PropertyEditors.First(e => e.Alias == Constants.PropertyEditors.Aliases.Label);
        var dataType = new DataType(editor, ConfigurationEditorJsonSerializer)
        {
            Name = $"Test Label {valueType ?? "Default"}",
            DatabaseType = valueType is null ? ValueStorageType.Nvarchar : ValueTypes.ToStorageType(valueType),
        };

        var configuration = new Dictionary<string, object>(additionalConfiguration ?? new Dictionary<string, object>());
        if (valueType is not null)
        {
            configuration[Constants.PropertyEditors.ConfigurationKeys.DataValueType] = valueType;
        }

        dataType.ConfigurationData = configuration;

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return (await DataTypeService.GetAsync(dataType.Key))!;
    }

    private Task ExecuteMigration()
        => MigrateTypedLabelDataTypes.ExecuteMigration(DataTypeService, PropertyEditors, NullLogger.Instance);
}
