namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class ImportDictionaryRequestModel
{
    public required ReferenceByIdModel TemporaryFile { get; set; }

    public ReferenceByIdModel? Parent { get; set; }
}
