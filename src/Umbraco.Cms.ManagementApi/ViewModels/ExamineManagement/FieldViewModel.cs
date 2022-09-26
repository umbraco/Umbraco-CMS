namespace Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;

public class FieldViewModel
{
    public string Name { get; init; } = null!;

    public IEnumerable<string> Values { get; init; } = null!;
}
