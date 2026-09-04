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
internal sealed class MigrateSingleUrlPickerDataTypesTests : UmbracoIntegrationTest
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer
        => GetRequiredService<IConfigurationEditorJsonSerializer>();

    [Test]
    public async Task Moves_A_Url_Picker_Configured_To_Hold_One_Link()
    {
        IDataType dataType = await CreateUrlPickerDataType(maxNumber: 1);

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.Multiple(() =>
        {
            Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.SingleUrlPicker));
            Assert.That(migrated.EditorUiAlias, Is.EqualTo("Umb.PropertyEditorUi.SingleUrlPicker"));
            Assert.That(migrated.ConfigurationData.ContainsKey("maxNumber"), Is.False);
            Assert.That(migrated.ConfigurationData.ContainsKey("minNumber"), Is.False);
        });
    }

    [TestCase(0)]
    [TestCase(2)]
    [TestCase(null)]
    public async Task Leaves_A_Url_Picker_Holding_Several_Links(int? maxNumber)
    {
        IDataType dataType = await CreateUrlPickerDataType(maxNumber);

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.MultiUrlPicker));
    }

    [Test]
    public async Task Keeps_The_Link_Count_Validation_On_A_Picker_Holding_Several_Links()
    {
        IDataType dataType = await CreateUrlPickerDataType(
            maxNumber: 5,
            additionalConfiguration: new Dictionary<string, object> { ["minNumber"] = 2 });

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        var configuration = migrated.ConfigurationAs<MultiUrlPickerConfiguration>()!;
        Assert.Multiple(() =>
        {
            Assert.That(configuration.MinNumber, Is.EqualTo(2));
            Assert.That(configuration.MaxNumber, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task Keeps_Configuration_Other_Than_The_Link_Count()
    {
        IDataType dataType = await CreateUrlPickerDataType(
            maxNumber: 1,
            additionalConfiguration: new Dictionary<string, object> { ["ignoreUserStartNodes"] = true });

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        var configuration = migrated.ConfigurationAs<SingleUrlPickerConfiguration>()!;
        Assert.That(configuration.IgnoreUserStartNodes, Is.True);
    }

    [Test]
    public async Task Is_Idempotent()
    {
        IDataType dataType = await CreateUrlPickerDataType(maxNumber: 1);

        await ExecuteMigration();
        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.SingleUrlPicker));
    }

    private async Task<IDataType> CreateUrlPickerDataType(
        int? maxNumber,
        IDictionary<string, object>? additionalConfiguration = null)
    {
        IDataEditor editor = PropertyEditors.First(e => e.Alias == Constants.PropertyEditors.Aliases.MultiUrlPicker);
        var dataType = new DataType(editor, ConfigurationEditorJsonSerializer)
        {
            Name = $"Test URL Picker {maxNumber?.ToString() ?? "Default"}",
        };

        var configuration = new Dictionary<string, object>(additionalConfiguration ?? new Dictionary<string, object>());
        if (maxNumber is not null)
        {
            configuration["maxNumber"] = maxNumber.Value;
        }

        dataType.ConfigurationData = configuration;

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return (await DataTypeService.GetAsync(dataType.Key))!;
    }

    private Task ExecuteMigration()
        => MigrateSingleUrlPickerDataTypes.ExecuteMigration(DataTypeService, PropertyEditors, NullLogger.Instance);
}
