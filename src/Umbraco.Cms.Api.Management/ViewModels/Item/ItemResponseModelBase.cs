namespace Umbraco.Cms.Api.Management.ViewModels.Item;

public abstract class ItemResponseModelBase : IHasSigns
{
    private readonly List<SignModel> _signs = [];

    public Guid Id { get; set; }

    public IEnumerable<SignModel> Signs
    {
        get => _signs.AsEnumerable();
        set
        {
            _signs.Clear();
            _signs.AddRange(value);
        }
    }

    public void AddSign(string alias) => _signs.Add(new SignModel { Alias = alias });

    public void RemoveSign(string alias) => _signs.RemoveAll(x => x.Alias == alias);
}
