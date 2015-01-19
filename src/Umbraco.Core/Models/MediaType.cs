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
        /// Method to call when Entity is being saved
        /// </summary>
        /// <remarks>Created date is set and a Unique key is assigned</remarks>
        internal override void AddingEntity()
        {
            base.AddingEntity();

            if (Key == Guid.Empty)
                Key = Guid.NewGuid();
        }

        /// <summary>
        /// Method to call when Entity is being updated
        /// </summary>
        /// <remarks>Modified Date is set and a new Version guid is set</remarks>
        internal override void UpdatingEntity()
        {
            base.UpdatingEntity();
        }
    }
}