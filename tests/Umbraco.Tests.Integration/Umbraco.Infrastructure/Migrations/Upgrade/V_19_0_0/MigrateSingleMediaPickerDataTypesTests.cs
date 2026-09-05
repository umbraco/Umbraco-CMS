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
internal sealed class MigrateSingleMediaPickerDataTypesTests : UmbracoIntegrationTest
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer
        => GetRequiredService<IConfigurationEditorJsonSerializer>();

    [TestCase(false)]
    [TestCase(null)]
    public async Task Moves_A_Media_Picker_Configured_To_Hold_A_Single_Item(bool? multiple)
    {
        IDataType dataType = await CreateMediaPickerDataType(multiple);

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.Multiple(() =>
        {
            Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.SingleMediaPicker));
            Assert.That(migrated.EditorUiAlias, Is.EqualTo("Umb.PropertyEditorUi.MediaPicker.Single"));
            Assert.That(migrated.ConfigurationData.ContainsKey("multiple"), Is.False);
        });
    }

    [Test]
    public async Task Leaves_A_Media_Picker_Holding_Several_Items()
    {
        IDataType dataType = await CreateMediaPickerDataType(multiple: true);

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.Multiple(() =>
        {
            Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.MediaPicker3));
            Assert.That(migrated.ConfigurationData.ContainsKey("multiple"), Is.False);
        });
    }

    [Test]
    public async Task Drops_The_Item_Count_Validation_From_A_Picker_Holding_A_Single_Item()
    {
        IDataType dataType = await CreateMediaPickerDataType(
            multiple: false,
            additionalConfiguration: new Dictionary<string, object>
            {
                ["validationLimit"] = new MediaPicker3Configuration.NumberRange { Min = 0, Max = 1 },
            });

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.That(migrated.ConfigurationData.ContainsKey("validationLimit"), Is.False);
    }

    [Test]
    public async Task Keeps_The_Item_Count_Validation_On_A_Picker_Holding_Several_Items()
    {
        IDataType dataType = await CreateMediaPickerDataType(
            multiple: true,
            additionalConfiguration: new Dictionary<string, object>
            {
                ["validationLimit"] = new MediaPicker3Configuration.NumberRange { Min = 2, Max = 5 },
            });

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        var validationLimit = migrated.ConfigurationAs<MediaPicker3Configuration>()!.ValidationLimit;
        Assert.Multiple(() =>
        {
            Assert.That(validationLimit.Min, Is.EqualTo(2));
            Assert.That(validationLimit.Max, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task Keeps_Configuration_Other_Than_The_Item_Count()
    {
        var startNodeId = Guid.NewGuid();
        IDataType dataType = await CreateMediaPickerDataType(
            multiple: false,
            additionalConfiguration: new Dictionary<string, object>
            {
                ["filter"] = "cc07b313-0843-4aa8-bbda-871c8da728c8",
                ["startNodeId"] = startNodeId,
                ["enableLocalFocalPoint"] = true,
                ["ignoreUserStartNodes"] = true,
            });

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        var configuration = migrated.ConfigurationAs<SingleMediaPickerConfiguration>()!;
        Assert.Multiple(() =>
        {
            Assert.That(configuration.Filter, Is.EqualTo("cc07b313-0843-4aa8-bbda-871c8da728c8"));
            Assert.That(configuration.StartNodeId, Is.EqualTo(startNodeId));
            Assert.That(configuration.EnableLocalFocalPoint, Is.True);
            Assert.That(configuration.IgnoreUserStartNodes, Is.True);
        });
    }

    [Test]
    public async Task Is_Idempotent()
    {
        IDataType dataType = await CreateMediaPickerDataType(multiple: false);

        await ExecuteMigration();
        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.SingleMediaPicker));
    }

    private async Task<IDataType> CreateMediaPickerDataType(
        bool? multiple,
        IDictionary<string, object>? additionalConfiguration = null)
    {
        IDataEditor editor = PropertyEditors.First(e => e.Alias == Constants.PropertyEditors.Aliases.MediaPicker3);
        var dataType = new DataType(editor, ConfigurationEditorJsonSerializer)
        {
            Name = $"Test Media Picker {multiple?.ToString() ?? "Default"}",
        };

        var configuration = new Dictionary<string, object>(additionalConfiguration ?? new Dictionary<string, object>());
        if (multiple is not null)
        {
            configuration["multiple"] = multiple.Value;
        }

        dataType.ConfigurationData = configuration;

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return (await DataTypeService.GetAsync(dataType.Key))!;
    }

    private Task ExecuteMigration()
        => MigrateSingleMediaPickerDataTypes.ExecuteMigration(DataTypeService, PropertyEditors, NullLogger.Instance);
}
