namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class ImportDictionaryItemsPresentationModel
{
    public Guid Key { get; set; }

    public string? Name { get; set; }

    public Guid? ParentKey { get; set; }
}
