namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class ImportDictionaryRequestModel
{
    public required Guid TemporaryFileKey { get; set; }

    public Guid? ParentKey { get; set; }
}
