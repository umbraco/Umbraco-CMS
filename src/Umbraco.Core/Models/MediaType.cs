using System;
using System.Runtime.Serialization;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents the content type that a <see cref="Media"/> object is based on
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class MediaType : ContentTypeCompositionBase, IMediaType
    {
        public MediaType(int parentId) : base(parentId)
        {
        }

		public MediaType(IMediaType parent) : base(parent)
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