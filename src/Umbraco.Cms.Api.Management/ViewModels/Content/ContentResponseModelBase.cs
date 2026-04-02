using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

/// <summary>
/// Serves as the base class for content response models in the Umbraco CMS API, using generic parameters to specify the value and variant response model types.
/// </summary>
public abstract class ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    : ContentModelBase<TValueResponseModelBase, TVariantResponseModel>, IHasFlags
    where TValueResponseModelBase : ValueModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    private readonly List<FlagModel> _flags = [];

    /// <summary>
    /// Gets or sets the unique identifier of the content.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the collection of flags associated with the content response.
    /// </summary>
    public IEnumerable<FlagModel> Flags
    {
        get => _flags.AsEnumerable();
        set
        {
            _flags.Clear();
            _flags.AddRange(value);
        }
    }

    /// <summary>Adds a flag with the specified alias to the content response model.</summary>
    /// <param name="alias">The alias of the flag to add.</param>
    public void AddFlag(string alias) => _flags.Add(new FlagModel { Alias = alias });

    /// <summary>
    /// Removes a flag with the specified alias from the content response model.
    /// </summary>
    /// <param name="alias">The alias of the flag to remove.</param>
    public void RemoveFlag(string alias) => _flags.RemoveAll(x => x.Alias == alias);
}
