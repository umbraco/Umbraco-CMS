using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    : ContentModelBase<TValueResponseModelBase, TVariantResponseModel>, IHasSigns
    where TValueResponseModelBase : ValueModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    private List<SignModel> _signs = [];

    public Guid Id { get; set; }

    public IEnumerable<SignModel> Signs
    {
        get => _signs.AsEnumerable();
        set => _signs = value.ToList();
    }

    public void AddSign(string alias) => _signs.Add(new SignModel { Alias = alias });

    public void RemoveSign(string alias) => _signs.RemoveAll(x => x.Alias == alias);
}
