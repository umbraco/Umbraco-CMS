namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class DictionaryImportModel
{
    public string FileName { get; set; } = string.Empty;

    public Guid? ParentKey { get; set; }
}
