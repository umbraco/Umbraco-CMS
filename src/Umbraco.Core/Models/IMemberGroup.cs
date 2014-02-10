using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a member type
    /// </summary>
    public interface IMemberGroup : IAggregateRoot
    {
        string Name { get; set; }
    }
}