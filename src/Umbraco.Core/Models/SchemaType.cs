using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a schema type
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class SchemaType : ContentTypeCompositionBase, ISchemaType
    {
        /// <summary>
        /// Constuctor for creating a SchemaType with the parent's id.
        /// </summary>
        /// <remarks>Only use this for creating SchemaTypes at the root (with ParentId -1).</remarks>
        /// <param name="parentId"></param>
        public SchemaType(int parentId) : base(parentId)
        {
        }

        /// <summary>
        /// Constuctor for creating a SchemaType with the parent as an inherited type.
        /// </summary>
        /// <remarks>Use this to ensure inheritance from parent.</remarks>
        /// <param name="parent"></param>
		public SchemaType(ISchemaType parent) : this(parent, null)
		{
		}

        /// <summary>
        /// Constuctor for creating a SchemaType with the parent as an inherited type.
        /// </summary>
        /// <remarks>Use this to ensure inheritance from parent.</remarks>
        /// <param name="parent"></param>
        /// <param name="alias"></param>
        public SchemaType(ISchemaType parent, string alias)
            : base(parent, alias)
        {
        }

        /// <summary>
        /// Creates a deep clone of the current entity with its identity/alias and it's property identities reset
        /// </summary>
        /// <returns></returns>
        public ISchemaType DeepCloneWithResetIdentities(string alias)
        {
            var clone = (SchemaType)DeepClone();
            clone.Alias = alias;
            clone.Key = Guid.Empty;
            foreach (var propertyGroup in clone.PropertyGroups)
            {
                propertyGroup.ResetIdentity();
                propertyGroup.ResetDirtyProperties(false);
            }
            foreach (var propertyType in clone.PropertyTypes)
            {
                propertyType.ResetIdentity();
                propertyType.ResetDirtyProperties(false);
            }

            clone.ResetIdentity();
            clone.ResetDirtyProperties(false);
            return clone;
        }
    }
}