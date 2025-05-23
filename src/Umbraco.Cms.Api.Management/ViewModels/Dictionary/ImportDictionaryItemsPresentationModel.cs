namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class ImportDictionaryItemsPresentationModel
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public ReferenceByIdModel? Parent { get; set; }
}
