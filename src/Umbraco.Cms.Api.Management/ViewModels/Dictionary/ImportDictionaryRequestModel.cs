namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class ImportDictionaryRequestModel
{
    public required string FileName { get; set; }

    public Guid? ParentKey { get; set; }
}
