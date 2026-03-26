using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

/// <summary>
/// Base class for publishable variant item response models, providing flag support and publish state.
/// </summary>
public abstract class PublishableVariantItemResponseModelBase : VariantItemResponseModelBase, IHasFlags
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
    public required DocumentVariantState State { get; set; }
}
