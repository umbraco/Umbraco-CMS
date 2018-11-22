using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents the content type that a <see cref="Media"/> object is based on
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class MediaType : ContentTypeCompositionBase, IMediaType
    {
        /// <summary>
        /// Constuctor for creating a MediaType with the parent's id.
        /// </summary>
        /// <remarks>Only use this for creating MediaTypes at the root (with ParentId -1).</remarks>
        /// <param name="parentId"></param>
        public MediaType(int parentId) : base(parentId)
        {
        }

        /// <summary>
        /// Constuctor for creating a MediaType with the parent as an inherited type.
        /// </summary>
        /// <remarks>Use this to ensure inheritance from parent.</remarks>
        /// <param name="parent"></param>
		public MediaType(IMediaType parent) : this(parent, null)
		{
		}

        /// <summary>
        /// Constuctor for creating a MediaType with the parent as an inherited type.
        /// </summary>
        /// <remarks>Use this to ensure inheritance from parent.</remarks>
        /// <param name="parent"></param>
        /// <param name="alias"></param>
        public MediaType(IMediaType parent, string alias)
            : base(parent, alias)
        {
        }

        /// <summary>
        /// Creates a deep clone of the current entity with its identity/alias and it's property identities reset
        /// </summary>
        /// <returns></returns>
        public IMediaType DeepCloneWithResetIdentities(string alias)
        {
            var clone = (MediaType)DeepClone();
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