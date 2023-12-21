using System.Collections;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Represents a collection of distinct <see cref="ArtifactDependency" />.
/// </summary>
/// <remarks>
/// The collection cannot contain duplicates and modes are properly managed.
/// </remarks>
public class ArtifactDependencyCollection : ICollection<ArtifactDependency>
{
    private readonly Dictionary<Udi, ArtifactDependency> _dependencies = new();

    /// <inheritdoc />
    public int Count => _dependencies.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public void Add(ArtifactDependency item)
    {
        if (item.Mode == ArtifactDependencyMode.Exist &&
            _dependencies.TryGetValue(item.Udi, out ArtifactDependency? existingItem) &&
            existingItem.Mode == ArtifactDependencyMode.Match)
        {
            // Don't downgrade dependency mode from Match to Exist
            return;
        }

        _dependencies[item.Udi] = item;
    }

    /// <inheritdoc />
    public void Clear() => _dependencies.Clear();

    /// <inheritdoc />
    public bool Contains(ArtifactDependency item)
        => _dependencies.TryGetValue(item.Udi, out ArtifactDependency? existingItem) &&
        // Check whether it has the same or higher dependency mode
        (existingItem.Mode == item.Mode || existingItem.Mode == ArtifactDependencyMode.Match);

    /// <inheritdoc />
    public void CopyTo(ArtifactDependency[] array, int arrayIndex) => _dependencies.Values.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(ArtifactDependency item) => _dependencies.Remove(item.Udi);

    /// <inheritdoc />
    public IEnumerator<ArtifactDependency> GetEnumerator() => _dependencies.Values.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
