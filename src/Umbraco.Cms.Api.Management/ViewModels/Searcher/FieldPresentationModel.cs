namespace Umbraco.Cms.Api.Management.ViewModels.Searcher;

public class FieldPresentationModel
{
    public string Name { get; init; } = null!;

    public IEnumerable<string> Values { get; init; } = null!;
}
