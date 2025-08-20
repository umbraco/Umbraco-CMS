namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class EntityTreeItemResponseModel : TreeItemPresentationModel, IHasSigns
{
    private readonly List<SignModel> _signs = [];

    public Guid Id { get; set; }

    public ReferenceByIdModel? Parent { get; set; }

    public IEnumerable<SignModel> Signs => _signs.AsEnumerable();

    public void AddSign(string alias) => _signs.Add(new SignModel { Alias = alias });

    public void RemoveSign(string alias) => _signs.RemoveAll(x => x.Alias == alias);
}
