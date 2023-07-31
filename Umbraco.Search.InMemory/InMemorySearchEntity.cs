using System.Runtime.Serialization;

namespace Umbraco.Search.InMemory;

public class InMemorySearchEntity : Dictionary<string, object>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="key"></param>
    public InMemorySearchEntity(string? id, Guid key)
    {
        Id = id;
        Key = key;
    }

    /// <summary>
    ///
    /// </summary>
    public InMemorySearchEntity()
    {
    }

    public string? Id { get; set; }
    public Guid Key { get; set; }
}
