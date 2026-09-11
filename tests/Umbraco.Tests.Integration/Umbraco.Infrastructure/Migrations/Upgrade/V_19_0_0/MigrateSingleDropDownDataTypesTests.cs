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
internal sealed class MigrateSingleDropDownDataTypesTests : UmbracoIntegrationTest
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer
        => GetRequiredService<IConfigurationEditorJsonSerializer>();

    [TestCase(false)]
    [TestCase(null)]
    public async Task Moves_A_Dropdown_Configured_To_Hold_A_Single_Value(bool? multiple)
    {
        IDataType dataType = await CreateDropDownDataType(multiple);

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.Multiple(() =>
        {
            Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.SingleDropDown));
            Assert.That(migrated.EditorUiAlias, Is.EqualTo("Umb.PropertyEditorUi.Dropdown.Single"));
            Assert.That(migrated.ConfigurationData.ContainsKey("multiple"), Is.False);
        });
    }

    [Test]
    public async Task Leaves_A_Dropdown_Holding_Several_Values()
    {
        IDataType dataType = await CreateDropDownDataType(multiple: true);

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.Multiple(() =>
        {
            Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.MultipleDropDown));
            Assert.That(migrated.ConfigurationData.ContainsKey("multiple"), Is.False);
        });
    }

    [Test]
    public async Task Keeps_The_Configured_Options()
    {
        IDataType dataType = await CreateDropDownDataType(
            multiple: false,
            additionalConfiguration: new Dictionary<string, object>
            {
                ["items"] = new List<string> { "One", "Two", "Three" },
            });

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        var configuration = migrated.ConfigurationAs<SingleDropDownConfiguration>()!;
        Assert.That(configuration.Items, Is.EqualTo(new[] { "One", "Two", "Three" }));
    }

    [Test]
    public async Task Is_Idempotent()
    {
        IDataType dataType = await CreateDropDownDataType(multiple: false);

        await ExecuteMigration();
        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.SingleDropDown));
    }

    [Test]
    public async Task Moves_The_Built_In_Single_Dropdown_Data_Type()
    {
        // A fresh schema already seeds the built-in data types on the new editor, so it has to be put back
        // into the shape an upgraded database is in before the migration has anything to do.
        IDataType dropDownSingle = (await DataTypeService.GetAsync(Constants.DataTypes.Guids.DropdownGuid))!;
        dropDownSingle.Editor = PropertyEditors.First(e => e.Alias == Constants.PropertyEditors.Aliases.MultipleDropDown);
        dropDownSingle.EditorUiAlias = "Umb.PropertyEditorUi.Dropdown";
        dropDownSingle.ConfigurationData = new Dictionary<string, object> { ["multiple"] = false };
        await DataTypeService.UpdateAsync(dropDownSingle, Constants.Security.SuperUserKey);

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(Constants.DataTypes.Guids.DropdownGuid))!;
        Assert.Multiple(() =>
        {
            Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.SingleDropDown));
            Assert.That(migrated.EditorUiAlias, Is.EqualTo("Umb.PropertyEditorUi.Dropdown.Single"));
        });
    }

    private async Task<IDataType> CreateDropDownDataType(
        bool? multiple,
        IDictionary<string, object>? additionalConfiguration = null)
    {
        IDataEditor editor = PropertyEditors.First(e => e.Alias == Constants.PropertyEditors.Aliases.MultipleDropDown);
        var dataType = new DataType(editor, ConfigurationEditorJsonSerializer)
        {
            Name = $"Test Dropdown {multiple?.ToString() ?? "Default"}",
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
        => MigrateSingleDropDownDataTypes.ExecuteMigration(DataTypeService, PropertyEditors, NullLogger.Instance);
}
