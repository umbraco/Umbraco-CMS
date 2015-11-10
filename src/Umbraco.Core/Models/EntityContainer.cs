using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a folder for organizing entities such as content types and data types
    /// </summary>
    public sealed class EntityContainer : UmbracoEntity, IAggregateRoot
    {
        public EntityContainer(int parentId, string name, int userId)
        {
            ParentId = parentId;
            Name = name;
            CreatorId = userId;
        }
    }
}