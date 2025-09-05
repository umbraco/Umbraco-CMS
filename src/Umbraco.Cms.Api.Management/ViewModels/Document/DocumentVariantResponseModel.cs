using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentVariantResponseModel : VariantResponseModelBase, IHasSigns
{
    public DocumentVariantState State { get; set; }

    public DateTimeOffset? PublishDate { get; set; }

    public DateTimeOffset? ScheduledPublishDate { get; set; }

    public DateTimeOffset? ScheduledUnpublishDate { get; set; }

    private readonly List<SignModel> _signs = [];

    public Guid Id { get; }

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
