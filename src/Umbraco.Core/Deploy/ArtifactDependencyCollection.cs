using System.Collections;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Represents a collection of distinct <see cref="ArtifactDependency" />.
/// </summary>
/// <remarks>The collection cannot contain duplicates and modes are properly managed.</remarks>
public class ArtifactDependencyCollection : ICollection<ArtifactDependency>
{
    private readonly Dictionary<Udi, ArtifactDependency> _dependencies = new();

    public int Count => _dependencies.Count;

    public IEnumerator<ArtifactDependency> GetEnumerator() => _dependencies.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(ArtifactDependency item)
    {
        if (_dependencies.ContainsKey(item.Udi))
        {
            ArtifactDependency exist = _dependencies[item.Udi];
            if (item.Mode == ArtifactDependencyMode.Exist || item.Mode == exist.Mode)
            {
                return;
            }
        }

        _dependencies[item.Udi] = item;
    }

    public void Clear() => _dependencies.Clear();

    public bool Contains(ArtifactDependency item) =>
        _dependencies.ContainsKey(item.Udi) &&
        (_dependencies[item.Udi].Mode == item.Mode || _dependencies[item.Udi].Mode == ArtifactDependencyMode.Match);

    public void CopyTo(ArtifactDependency[] array, int arrayIndex) => _dependencies.Values.CopyTo(array, arrayIndex);

    public bool Remove(ArtifactDependency item) => throw new NotSupportedException();

    public bool IsReadOnly => false;
}
