using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents a response model for a document variant item in the Umbraco CMS Management API.
/// A document variant typically refers to a specific language or segment version of a document.
/// </summary>
public class DocumentVariantItemResponseModel : VariantItemResponseModelBase, IHasFlags
{
    private readonly List<FlagModel> _flags = [];

    /// <summary>
    /// Gets the unique identifier of the document variant.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets or sets the collection of flags associated with the document variant.
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
    /// Adds a flag with the specified alias to the collection of flags for this document variant.
    /// </summary>
    /// <param name="alias">The alias of the flag to add.</param>
    public void AddFlag(string alias) => _flags.Add(new FlagModel { Alias = alias });

    /// <summary>Removes a flag with the specified alias from the document variant.</summary>
    /// <param name="alias">The alias of the flag to remove.</param>
    public void RemoveFlag(string alias) => _flags.RemoveAll(x => x.Alias == alias);

    public required DocumentVariantState State { get; set; }
}
