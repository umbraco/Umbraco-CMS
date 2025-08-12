using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Api.Management.ViewModels.Document.Collection;

public class DocumentCollectionResponseModel : ContentCollectionResponseModelBase<DocumentValueResponseModel, DocumentVariantResponseModel>
{
    private readonly List<SignModel> _signs = [];

    public DocumentTypeCollectionReferenceResponseModel DocumentType { get; set; } = new();

    public bool IsTrashed { get; set; }

    public bool IsProtected { get; set; }

    public IEnumerable<ReferenceByIdModel> Ancestors { get; set; } = [];

    public string? Updater { get; set; }

    public IEnumerable<SignModel> Signs => _signs.AsEnumerable();

    public void AddSign(string alias) => _signs.Add(new SignModel { Alias = alias });

    public void RemoveSign(string alias) => _signs.RemoveAll(x => x.Alias == alias);
}
