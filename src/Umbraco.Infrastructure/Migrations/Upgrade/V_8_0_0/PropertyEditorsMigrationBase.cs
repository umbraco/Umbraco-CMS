using System.Globalization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public abstract class PropertyEditorsMigrationBase : MigrationBase
{
    protected PropertyEditorsMigrationBase(IMigrationContext context)
        : base(context)
    {
    }

    internal List<DataTypeDto> GetDataTypes(string editorAlias, bool strict = true)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<DataTypeDto>()
            .From<DataTypeDto>();

        sql = strict
            ? sql.Where<DataTypeDto>(x => x.EditorAlias == editorAlias)
            : sql.Where<DataTypeDto>(x => x.EditorAlias.Contains(editorAlias));

        return Database.Fetch<DataTypeDto>(sql);
    }

    internal bool UpdatePropertyDataDto(PropertyDataDto propData, ValueListConfiguration config, bool isMultiple)
    {
        // Get the INT ids stored for this property/drop down
        int[]? ids = null;
        if (!propData.VarcharValue.IsNullOrWhiteSpace())
        {
            ids = ConvertStringValues(propData.VarcharValue);
        }
        else if (!propData.TextValue.IsNullOrWhiteSpace())
        {
            ids = ConvertStringValues(propData.TextValue);
        }
        else if (propData.IntegerValue.HasValue)
        {
            ids = new[] { propData.IntegerValue.Value };
        }

        // if there are INT ids, convert them to values based on the configuration
        if (ids == null || ids.Length <= 0)
        {
            return false;
        }

        // map ids to values
        var values = new List<string>();
        var canConvert = true;

        foreach (var id in ids)
        {
            ValueListConfiguration.ValueListItem? val = config.Items.FirstOrDefault(x => x.Id == id);
            if (val?.Value != null)
            {
                values.Add(val.Value);
                continue;
            }

            Logger.LogWarning(
                "Could not find PropertyData {PropertyDataId} value '{PropertyValue}' in the datatype configuration: {Values}.",
                propData.Id, id, string.Join(", ", config.Items.Select(x => x.Id + ":" + x.Value)));
            canConvert = false;
        }

        if (!canConvert)
        {
            return false;
        }

        propData.VarcharValue = isMultiple ? JsonConvert.SerializeObject(values) : values[0];
        propData.TextValue = null;
        propData.IntegerValue = null;
        return true;
    }

    protected int[]? ConvertStringValues(string? val)
    {
        var splitVals = val?.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);

        var intVals = splitVals?
            .Select(x =>
                int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : int.MinValue)
            .Where(x => x != int.MinValue)
            .ToArray();

        // only return if the number of values are the same (i.e. All INTs)
        if (splitVals?.Length == intVals?.Length)
        {
            return intVals;
        }

        return null;
    }

    // dummy editor for deserialization
    protected class ValueListConfigurationEditor : ConfigurationEditor<ValueListConfiguration>
    {
        public ValueListConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
            : base(ioHelper, editorConfigurationParser)
        {
        }
    }
}
