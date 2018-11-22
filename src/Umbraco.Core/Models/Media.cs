using System;
using System.Runtime.Serialization;
using Umbraco.Core.Persistence.Mappers;

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
        /// <param name="name">ame of the Media object</param>
        /// <param name="parent">Parent <see cref="IMedia"/> object</param>
        /// <param name="contentType">MediaType for the current Media object</param>
        public Media(string name, IMedia parent, IMediaType contentType)
			: this(name, parent, contentType, new PropertyCollection())
		{
		}

        /// <summary>
        /// Constructor for creating a Media object
        /// </summary>
        /// <param name="name">ame of the Media object</param>
        /// <param name="parent">Parent <see cref="IMedia"/> object</param>
        /// <param name="contentType">MediaType for the current Media object</param>
        /// <param name="properties">Collection of properties</param>
        public Media(string name, IMedia parent, IMediaType contentType, PropertyCollection properties)
			: base(name, parent, contentType, properties)
		{
			Mandate.ParameterNotNull(contentType, "contentType");
			_contentType = contentType;
		}

        /// <summary>
        /// Constructor for creating a Media object
        /// </summary>
        /// <param name="name">ame of the Media object</param>
        /// <param name="parentId">Id of the Parent IMedia</param>
        /// <param name="contentType">MediaType for the current Media object</param>
        public Media(string name, int parentId, IMediaType contentType)
            : this(name, parentId, contentType, new PropertyCollection())
        {
        }

        /// <summary>
        /// Constructor for creating a Media object
        /// </summary>
        /// <param name="name">Name of the Media object</param>
        /// <param name="parentId">Id of the Parent IMedia</param>
        /// <param name="contentType">MediaType for the current Media object</param>
        /// <param name="properties">Collection of properties</param>
        public Media(string name, int parentId, IMediaType contentType, PropertyCollection properties) 
            : base(name, parentId, contentType, properties)
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
        public override void ChangeTrashedState(bool isTrashed, int parentId = -20)
        {
            Trashed = isTrashed;
            //The Media Recycle Bin Id is -21 so we correct that here
            ParentId = parentId == -20 ? -21 : parentId;
        }
    }
}