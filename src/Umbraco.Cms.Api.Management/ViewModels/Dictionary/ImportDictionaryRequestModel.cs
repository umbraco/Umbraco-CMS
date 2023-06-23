namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class ImportDictionaryRequestModel
{
    public required Guid TemporaryFileId { get; set; }

    public Guid? ParentId { get; set; }
}
