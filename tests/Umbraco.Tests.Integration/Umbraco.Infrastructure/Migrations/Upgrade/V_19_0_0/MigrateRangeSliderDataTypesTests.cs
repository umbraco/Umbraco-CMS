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
internal sealed class MigrateRangeSliderDataTypesTests : UmbracoIntegrationTest
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer
        => GetRequiredService<IConfigurationEditorJsonSerializer>();

    [Test]
    public async Task Moves_A_Slider_Configured_To_Hold_A_Range()
    {
        IDataType dataType = await CreateSliderDataType(enableRange: true);

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.Multiple(() =>
        {
            Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.RangeSlider));
            Assert.That(migrated.EditorUiAlias, Is.EqualTo("Umb.PropertyEditorUi.RangeSlider"));
            Assert.That(migrated.ConfigurationData.ContainsKey("enableRange"), Is.False);
        });
    }

    [TestCase(false)]
    [TestCase(null)]
    public async Task Leaves_A_Slider_Holding_A_Single_Value(bool? enableRange)
    {
        IDataType dataType = await CreateSliderDataType(enableRange);

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.Multiple(() =>
        {
            Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.Slider));
            Assert.That(migrated.ConfigurationData.ContainsKey("enableRange"), Is.False);
        });
    }

    [Test]
    public async Task Keeps_Configuration_Other_Than_The_Range_Flag()
    {
        IDataType dataType = await CreateSliderDataType(
            enableRange: true,
            additionalConfiguration: new Dictionary<string, object>
            {
                ["minVal"] = 10m,
                ["maxVal"] = 90m,
                ["minimumRange"] = 5m,
                ["step"] = 0.5m,
            });

        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.Multiple(() =>
        {
            Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.RangeSlider));
            Assert.That(migrated.ConfigurationData["minVal"], Is.EqualTo(10m));
            Assert.That(migrated.ConfigurationData["maxVal"], Is.EqualTo(90m));
            Assert.That(migrated.ConfigurationData["minimumRange"], Is.EqualTo(5m));
            Assert.That(migrated.ConfigurationData["step"], Is.EqualTo(0.5m));
        });
    }

    [Test]
    public async Task Is_Idempotent()
    {
        IDataType dataType = await CreateSliderDataType(enableRange: true);

        await ExecuteMigration();
        await ExecuteMigration();

        IDataType migrated = (await DataTypeService.GetAsync(dataType.Key))!;
        Assert.That(migrated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.RangeSlider));
    }

    private async Task<IDataType> CreateSliderDataType(
        bool? enableRange,
        IDictionary<string, object>? additionalConfiguration = null)
    {
        IDataEditor editor = PropertyEditors.First(e => e.Alias == Constants.PropertyEditors.Aliases.Slider);
        var dataType = new DataType(editor, ConfigurationEditorJsonSerializer)
        {
            Name = $"Test Slider {enableRange?.ToString() ?? "Default"}",
        };

        var configuration = new Dictionary<string, object>(additionalConfiguration ?? new Dictionary<string, object>());
        if (enableRange is not null)
        {
            configuration["enableRange"] = enableRange.Value;
        }

        dataType.ConfigurationData = configuration;

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return (await DataTypeService.GetAsync(dataType.Key))!;
    }

    private Task ExecuteMigration()
        => MigrateRangeSliderDataTypes.ExecuteMigration(DataTypeService, PropertyEditors, NullLogger.Instance);
}
