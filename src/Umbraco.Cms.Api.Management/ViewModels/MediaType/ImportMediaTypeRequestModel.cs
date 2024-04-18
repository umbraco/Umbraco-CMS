namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

public class ImportMediaTypeRequestModel
{
    public required ReferenceByIdModel File { get; set; }

    public bool OverWriteExisting { get; set; } = false;
}
