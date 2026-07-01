namespace Umbraco.Cms.Api.Management.ViewModels.Item;

/// <summary>
/// Serves as the base response model for items in the Umbraco CMS Management API.
/// </summary>
public abstract class ItemResponseModelBase : IHasFlags
{
    private readonly List<FlagModel> _flags = [];

    /// <summary>Gets or sets the unique identifier of the item.</summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the collection of flags associated with the item.
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

    /// <summary>Adds a flag with the specified alias to the item.</summary>
    /// <param name="alias">The alias of the flag to add.</param>
    public void AddFlag(string alias) => _flags.Add(new FlagModel { Alias = alias });

    /// <summary>Removes a flag identified by the specified alias.</summary>
    /// <param name="alias">The alias of the flag to remove.</param>
    public void RemoveFlag(string alias) => _flags.RemoveAll(x => x.Alias == alias);
}
