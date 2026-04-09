using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

/// <summary>
/// Base class for publishable variant response models, providing flag support, publish state, and scheduling information.
/// </summary>
public abstract class PublishableVariantResponseModelBase : VariantResponseModelBase, IHasFlags
{
    private readonly List<FlagModel> _flags = [];

    /// <inheritdoc />
    public Guid Id { get; }

    /// <inheritdoc />
    public IEnumerable<FlagModel> Flags
    {
        get => _flags.AsEnumerable();
        set
        {
            _flags.Clear();
            _flags.AddRange(value);
        }
    }

    /// <inheritdoc />
    public void AddFlag(string alias) => _flags.Add(new FlagModel { Alias = alias });

    /// <inheritdoc />
    public void RemoveFlag(string alias) => _flags.RemoveAll(x => x.Alias == alias);

    /// <summary>
    /// Gets or sets the publish state of the variant.
    /// </summary>
    public DocumentVariantState State { get; set; }

    /// <summary>
    /// Gets or sets the date the variant was published.
    /// </summary>
    public DateTimeOffset? PublishDate { get; set; }

    /// <summary>
    /// Gets or sets the scheduled publish date for the variant.
    /// </summary>
    public DateTimeOffset? ScheduledPublishDate { get; set; }

    /// <summary>
    /// Gets or sets the scheduled unpublish date for the variant.
    /// </summary>
    public DateTimeOffset? ScheduledUnpublishDate { get; set; }
}
