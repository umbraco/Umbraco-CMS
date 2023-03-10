namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class DictionaryImportModel
{
    public required Guid TemporaryFileKey { get; set; }

    public Guid? ParentKey { get; set; }
}
