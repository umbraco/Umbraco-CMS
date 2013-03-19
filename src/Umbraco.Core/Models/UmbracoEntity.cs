using System;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Implementation of the <see cref="IUmbracoEntity"/> for internal use.
    /// </summary>
    internal class UmbracoEntity : Entity, IUmbracoEntity
    {
        public UmbracoEntity()
        {
        }

        public UmbracoEntity(bool trashed)
        {
            Trashed = trashed;
        }

        public int CreatorId { get; set; }

        public int Level { get; set; }

        public string Name { get; set; }

        public int ParentId { get; set; }

        public string Path { get; set; }

        public int SortOrder { get; set; }

        public bool Trashed { get; private set; }

        public bool HasChildren { get; set; }

        public bool IsPublished { get; set; }

        public Guid NodeObjectTypeId { get; set; }
    }
}