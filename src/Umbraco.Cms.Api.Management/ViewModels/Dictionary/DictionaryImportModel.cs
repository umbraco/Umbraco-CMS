namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class DictionaryImportModel
{
    public required string FileName { get; set; }

    public Guid? ParentKey { get; set; }
}
