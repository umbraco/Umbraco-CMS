namespace Umbraco.Cms.ManagementApi.ViewModels.JsonPatch;

public class JsonPatchViewModel
{
    public string Op { get; set; } = null!;

    public string Path { get; set; } = null!;

    public object Value { get; set; } = null!;
}
