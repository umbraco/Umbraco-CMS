namespace Umbraco.Cms.Api.Management.ViewModels.Tag;

public class TagResponseModel
{
    public Guid Id { get; set; }

    public string? Text { get; set; }

    public string? Group { get; set; }

    public int NodeCount { get; set; }
}
