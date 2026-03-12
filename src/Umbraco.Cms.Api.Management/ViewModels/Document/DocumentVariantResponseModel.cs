using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the response model returned by the API for a specific variant of a document, containing variant-specific data.
/// </summary>
public class DocumentVariantResponseModel : PublishableVariantResponseModelBase, IHasFlags
{
    private readonly List<FlagModel> _flags = [];

    /// <summary>
    /// Gets the unique identifier of the document variant.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets or sets the collection of flags that provide additional information or status about the document variant.
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

    /// <summary>
    /// Adds a flag with the specified alias to the current document variant.
    /// </summary>
    /// <param name="alias">The alias of the flag to add to the document variant.</param>
    public void AddFlag(string alias) => _flags.Add(new FlagModel { Alias = alias });

    /// <summary>
    /// Removes a flag identified by the specified alias.
    /// </summary>
    /// <param name="alias">The alias of the flag to remove.</param>
    public void RemoveFlag(string alias) => _flags.RemoveAll(x => x.Alias == alias);
}
