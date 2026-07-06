namespace Umbraco.Cms.Search.Provider.Examine.Models.ViewModels;

public class FieldViewModel
{
    public required string Name { get; set; }

    public string? Type { get; set; }

    public required IReadOnlyCollection<string> Values { get; set; }
}
