using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;

internal class ValueListPreValueMigrator : IPreValueMigrator
{
    private readonly string[] _editors =
    {
        "Umbraco.RadioButtonList", "Umbraco.CheckBoxList", "Umbraco.DropDown", "Umbraco.DropdownlistPublishingKeys",
        "Umbraco.DropDownMultiple", "Umbraco.DropdownlistMultiplePublishKeys",
    };

    public bool CanMigrate(string editorAlias)
        => _editors.Contains(editorAlias);

    public virtual string? GetNewAlias(string editorAlias)
        => null;

    public object GetConfiguration(int dataTypeId, string editorAlias, Dictionary<string, PreValueDto> preValues)
    {
        var config = new ValueListConfiguration();
        foreach (PreValueDto preValue in preValues.Values)
        {
            config.Items.Add(new ValueListConfiguration.ValueListItem { Id = preValue.Id, Value = preValue.Value });
        }

        return config;
    }
}
