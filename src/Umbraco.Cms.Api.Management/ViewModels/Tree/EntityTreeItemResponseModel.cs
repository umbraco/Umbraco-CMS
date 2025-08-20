namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class EntityTreeItemResponseModel : TreeItemPresentationModel, IHasSigns
{
    private List<SignModel> _signs = [];

    public Guid Id { get; set; }

    public ReferenceByIdModel? Parent { get; set; }

    public IEnumerable<SignModel> Signs
    {
        get => _signs.AsEnumerable();
        set => _signs = value.ToList();
    }

    public void AddSign(string alias) => _signs.Add(new SignModel { Alias = alias });

    public void RemoveSign(string alias) => _signs.RemoveAll(x => x.Alias == alias);
}
