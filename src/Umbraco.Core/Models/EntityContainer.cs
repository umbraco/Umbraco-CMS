using System;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a folder for organizing entities such as content types and data types
    /// </summary>
    public sealed class EntityContainer : UmbracoEntity, IAggregateRoot
    {
        public EntityContainer()
        {            
        }

        public EntityContainer(int id, Guid uniqueId, int parentId, string name, int userId, string path)
        {
            Id = id;
            Key = uniqueId;
            ParentId = parentId;
            Name = name;
            CreatorId = userId;
            Path = path;
        }
    }
}