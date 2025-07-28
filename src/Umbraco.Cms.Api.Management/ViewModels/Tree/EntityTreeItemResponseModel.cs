namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class EntityTreeItemResponseModel : TreeItemPresentationModel
{
    private readonly List<SignModel> _signs = [];

    public Guid Id { get; set; }

    public ReferenceByIdModel? Parent { get; set; }

    public IEnumerable<SignModel> Signs => _signs.AsEnumerable();

    public void AddSign(string provider, string alias) => _signs.Add(new SignModel { Provider = provider, Alias = alias });

    public void RemoveSign(string provider, string alias) => _signs.RemoveAll(x => x.Provider == provider && x.Alias == alias);
}
