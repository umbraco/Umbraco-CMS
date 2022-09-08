using Newtonsoft.Json;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;

internal class DefaultPreValueMigrator : IPreValueMigrator
{
    public virtual bool CanMigrate(string editorAlias)
        => true;

    public virtual string? GetNewAlias(string editorAlias)
        => editorAlias;

    public object GetConfiguration(int dataTypeId, string editorAlias, Dictionary<string, PreValueDto> preValues)
    {
        var preValuesA = preValues.Values.ToList();
        var aliases = preValuesA.Select(x => x.Alias).Distinct().ToArray();
        if (aliases.Length == 1 && string.IsNullOrWhiteSpace(aliases[0]))
        {
            // array-based prevalues
            return new Dictionary<string, object>
            {
                ["values"] = preValuesA.OrderBy(x => x.SortOrder).Select(x => x.Value).ToArray(),
            };
        }

        // assuming we don't want to fall back to array
        if (aliases.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                $"Cannot migrate prevalues for datatype id={dataTypeId}, editor={editorAlias}: null/empty alias.");
        }

        // dictionary-base prevalues
        return GetPreValues(preValuesA).ToDictionary(x => x.Alias, GetPreValueValue);
    }

    protected virtual IEnumerable<PreValueDto> GetPreValues(IEnumerable<PreValueDto> preValues)
        => preValues;

    protected virtual object? GetPreValueValue(PreValueDto preValue) => preValue.Value?.DetectIsJson() ?? false
        ? JsonConvert.DeserializeObject(preValue.Value)
        : preValue.Value;
}
