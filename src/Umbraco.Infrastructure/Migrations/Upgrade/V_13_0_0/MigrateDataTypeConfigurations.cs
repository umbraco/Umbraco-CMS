using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Migrations.PostMigrations;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;
using PropertyEditorAliases = Umbraco.Cms.Core.Constants.PropertyEditors.Aliases;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

public class MigrateDataTypeConfigurations : MigrationBase
{
    public MigrateDataTypeConfigurations(IMigrationContext context)
        : base(context)
    {
    }

    // TODO: this migration is a work in progress; it will be amended for a while, thus it MUST be able to re-run several times without failing miserably
    protected override void Migrate()
    {
        // this really should be injected, but until we can get rid of the Newtonsoft.Json based config serializer, we can't rely on
        // injection during installs, so we will have to settle for new'ing it up explicitly here (at least for now).
        IConfigurationEditorJsonSerializer serializer = new SystemTextConfigurationEditorJsonSerializer();

        Sql<ISqlContext> sql = Sql()
            .Select<DataTypeDto>()
            .From<DataTypeDto>()
            .Where<DataTypeDto>(x => x.EditorAlias.Contains("Umbraco."));

        List<DataTypeDto> dataTypeDtos = Database.Fetch<DataTypeDto>(sql);

        foreach (DataTypeDto dataTypeDto in dataTypeDtos)
        {
            Dictionary<string, object> configurationData = dataTypeDto.Configuration.IsNullOrWhiteSpace()
                ? new Dictionary<string, object>()
                : serializer.Deserialize<Dictionary<string, object>>(dataTypeDto.Configuration) ?? new Dictionary<string, object>();

            // fix config key casing - should always be camelCase, but some have been saved as PascalCase over the years
            var badlyCasedKeys = configurationData.Keys.Where(key => key.ToFirstLowerInvariant() != key).ToArray();
            var updated = badlyCasedKeys.Any();
            foreach (var incorrectKey in badlyCasedKeys)
            {
                configurationData[incorrectKey.ToFirstLowerInvariant()] = configurationData[incorrectKey];
                configurationData.Remove(incorrectKey);
            }

            // handle special cases, i.e. missing configs (list view), weirdly serialized configs (color picker), min/max for multiple text strings, etc. etc.
            updated |= dataTypeDto.EditorAlias switch
            {
                PropertyEditorAliases.MultipleTextstring => HandleMultipleTextstring(ref configurationData),
                PropertyEditorAliases.Label => HandleLabel(ref configurationData),
                PropertyEditorAliases.TextArea => HandleTextBoxAndTextArea(ref configurationData),
                PropertyEditorAliases.TextBox => HandleTextBoxAndTextArea(ref configurationData),
                // TODO: decide on value formats for applicable configs and re-format said configs here (i.e. color picker)
                // TODO: append/enrich any missing configs here (i.e. list views are likely missing one or more config values)
                _ => false
            };

            if (updated)
            {
                dataTypeDto.Configuration = serializer.Serialize(configurationData);
                Database.Update(dataTypeDto);
                RebuildCache = true;
            }
        }
    }

    // convert the stored keys "minimum" and "maximum" to the expected keys "min" and "max for multiple textstrings
    private static bool HandleMultipleTextstring(ref Dictionary<string, object> configurationData)
    {
        Dictionary<string, object> data = configurationData;

        bool ReplaceKey(string oldKey, string newKey)
        {
            if (data.ContainsKey(oldKey))
            {
                data[newKey] = data[oldKey];
                data.Remove(oldKey);
                return true;
            }

            return false;
        }

        return ReplaceKey("minimum", "min") | ReplaceKey("maximum", "max");
    }

    // enforce default "umbracoDataValueType" for label (may be empty for old data types)
    private static bool HandleLabel(ref Dictionary<string, object> configurationData)
    {
        if (configurationData.ContainsKey(Constants.PropertyEditors.ConfigurationKeys.DataValueType))
        {
            if (configurationData[Constants.PropertyEditors.ConfigurationKeys.DataValueType] is string value && value.IsNullOrWhiteSpace() == false)
            {
                return false;
            }
        }

        configurationData[Constants.PropertyEditors.ConfigurationKeys.DataValueType] = ValueTypes.String;
        return true;
    }

    // enforce integer values for text area and text box (may be saved as string values from old times)
    private static bool HandleTextBoxAndTextArea(ref Dictionary<string, object> configurationData)
    {
        Dictionary<string, object> data = configurationData;
        bool ReplaceStringWithIntValue(string key)
        {
            if (data.ContainsKey(key) && data[key] is string stringValue && int.TryParse(stringValue, out var intValue))
            {
                data[key] = intValue;
                return true;
            }

            return false;
        }

        return ReplaceStringWithIntValue("maxChars") | ReplaceStringWithIntValue("rows");
    }
}
