namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents an entity tree item returned by the Umbraco CMS Management API.
/// </summary>
public class EntityTreeItemResponseModel : TreeItemPresentationModel, IHasFlags
{
    private readonly List<FlagModel> _flags = [];

    /// <summary>
    /// Gets or sets the unique identifier for the entity tree item.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets a reference to the parent entity, or <c>null</c> if there is no parent.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }

    /// <summary>
    /// Gets or sets the collection of flags associated with the entity tree item.
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
    /// Adds a flag with the specified alias to the entity tree item.
    /// </summary>
    /// <param name="alias">The alias of the flag to add.</param>
    public void AddFlag(string alias) => _flags.Add(new FlagModel { Alias = alias });

    /// <summary>
    /// Removes a flag with the specified alias from the entity.
    /// </summary>
    /// <param name="alias">The alias of the flag to remove.</param>
    public void RemoveFlag(string alias) => _flags.RemoveAll(x => x.Alias == alias);
}
