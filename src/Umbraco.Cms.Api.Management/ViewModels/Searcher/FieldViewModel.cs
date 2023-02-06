namespace Umbraco.Cms.Api.Management.ViewModels.Searcher;

public class FieldViewModel
{
    public string Name { get; init; } = null!;

    public IEnumerable<string> Values { get; init; } = null!;
}
