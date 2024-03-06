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
        if (_dependencies.TryGetValue(item.Udi, out ArtifactDependency? existingItem))
        {
            // Update existing item
            if (existingItem.Mode is ArtifactDependencyMode.Exist)
            {
                // Allow updating dependency mode from Exist to Match
                existingItem.Mode = item.Mode;
            }

            if (existingItem.Ordering is false)
            {
                // Allow updating non-ordering to ordering
                existingItem.Ordering = item.Ordering;
            }

            if (string.IsNullOrEmpty(item.Checksum) is false)
            {
                // Allow updating checksum if set
                existingItem.Checksum = item.Checksum;
            }
        }
        else
        {
            // Add new item
            _dependencies[item.Udi] = item;
        }
    }

    /// <inheritdoc />
    public void Clear() => _dependencies.Clear();

    /// <inheritdoc />
    public bool Contains(ArtifactDependency item) => _dependencies.ContainsKey(item.Udi);

    /// <inheritdoc />
    public void CopyTo(ArtifactDependency[] array, int arrayIndex) => _dependencies.Values.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(ArtifactDependency item) => _dependencies.Remove(item.Udi);

    /// <inheritdoc />
    public IEnumerator<ArtifactDependency> GetEnumerator() => _dependencies.Values.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
