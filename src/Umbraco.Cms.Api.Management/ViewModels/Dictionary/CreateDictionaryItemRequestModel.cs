namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class CreateDictionaryItemRequestModel : DictionaryItemModelBase
{
    public Guid? Id { get; set; }

    public ReferenceByIdModel? Parent { get; set; }
}
