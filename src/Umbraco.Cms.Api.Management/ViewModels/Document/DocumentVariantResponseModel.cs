using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentVariantResponseModel : VariantResponseModelBase, IHasFlags
{
    public DocumentVariantState State { get; set; }

    public DateTimeOffset? PublishDate { get; set; }

    public DateTimeOffset? ScheduledPublishDate { get; set; }

    public DateTimeOffset? ScheduledUnpublishDate { get; set; }

    private readonly List<FlagModel> _flags = [];

    public Guid Id { get; }

    public IEnumerable<FlagModel> Flags
    {
        get => _flags.AsEnumerable();
        set
        {
            _flags.Clear();
            _flags.AddRange(value);
        }
    }

    public void AddFlag(string alias) => _flags.Add(new FlagModel { Alias = alias });

    public void RemoveFlag(string alias) => _flags.RemoveAll(x => x.Alias == alias);
}
