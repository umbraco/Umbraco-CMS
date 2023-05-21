namespace Umbraco.Cms.Core.Macros;

public class MacroPropertyModel
{
    public MacroPropertyModel() => Key = string.Empty;

    public MacroPropertyModel(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public MacroPropertyModel(string key, string value, string type)
    {
        Key = key;
        Value = value;
        Type = type;
    }

    public string Key { get; set; }

    public string? Value { get; set; }

    public string? Type { get; set; }
}
