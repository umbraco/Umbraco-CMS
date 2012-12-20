using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Media object
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Media : ContentBase, IMedia
    {
        private IMediaType _contentType;

        /// <summary>
        /// Constructor for creating a Media object
        /// </summary>
        /// <param name="parentId"> </param>
        /// <param name="contentType">MediaType for the current Media object</param>
        public Media(int parentId, IMediaType contentType)
            : this(parentId, contentType, new PropertyCollection())
        {
        }

		public Media(IMedia parent, IMediaType contentType)
			: this(parent, contentType, new PropertyCollection())
		{
		}

		public Media(IMedia parent, IMediaType contentType, PropertyCollection properties)
			: base(parent, contentType, properties)
		{
			Mandate.ParameterNotNull(contentType, "contentType");
			_contentType = contentType;
		}

        /// <summary>
        /// Constructor for creating a Media object
        /// </summary>
        /// <param name="parentId"> </param>
        /// <param name="contentType">MediaType for the current Media object</param>
        /// <param name="properties">Collection of properties</param>
        public Media(int parentId, IMediaType contentType, PropertyCollection properties) : base(parentId, contentType, properties)
        {
			Mandate.ParameterNotNull(contentType, "contentType");
            _contentType = contentType;
        }

        /// <summary>
        /// Gets the ContentType used by this Media object
        /// </summary>
        [IgnoreDataMember]
        public IMediaType ContentType
        {
            get { return _contentType; }
        }

        /// <summary>
        /// Changes the <see cref="IMediaType"/> for the current Media object
        /// </summary>
        /// <param name="contentType">New MediaType for this Media</param>
        /// <remarks>Leaves PropertyTypes intact after change</remarks>
        public void ChangeContentType(IMediaType contentType)
        {
            ContentTypeId = contentType.Id;
            _contentType = contentType;
            ContentTypeBase = contentType;
            Properties.EnsurePropertyTypes(PropertyTypes);
            Properties.CollectionChanged += PropertiesChanged;
        }

        /// <summary>
        /// Changes the <see cref="IMediaType"/> for the current Media object and removes PropertyTypes,
        /// which are not part of the new MediaType.
        /// </summary>
        /// <param name="contentType">New MediaType for this Media</param>
        /// <param name="clearProperties">Boolean indicating whether to clear PropertyTypes upon change</param>
        public void ChangeContentType(IMediaType contentType, bool clearProperties)
        {
            if (clearProperties)
            {
                ContentTypeId = contentType.Id;
                _contentType = contentType;
                ContentTypeBase = contentType;
                Properties.EnsureCleanPropertyTypes(PropertyTypes);
                Properties.CollectionChanged += PropertiesChanged;
                return;
            }

            ChangeContentType(contentType);
        }

        /// <summary>
        /// Changes the Trashed state of the content object
        /// </summary>
        /// <param name="isTrashed">Boolean indicating whether content is trashed (true) or not trashed (false)</param>
        /// <param name="parentId"> </param>
        internal void ChangeTrashedState(bool isTrashed, int parentId = -1)
        {
            Trashed = isTrashed;

            //If Content is trashed the parent id should be set to that of the RecycleBin
            if (isTrashed)
            {
                ParentId = -20;
            }
            else//otherwise set the parent id to the optional parameter, -1 being the fallback
            {
                ParentId = parentId;
            }
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
    }
}