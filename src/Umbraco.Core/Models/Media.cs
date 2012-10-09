using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.IO;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Media object
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Media : Entity, IMedia
    {
        private int _parentId;
        private string _name;
        private int _sortOrder;
        private int _level;
        private string _path;
        private int _userId;
        private bool _trashed;
        private int _contentTypeId;
        private PropertyCollection _properties;
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

        /// <summary>
        /// Constructor for creating a Media object
        /// </summary>
        /// <param name="parentId"> </param>
        /// <param name="contentType">MediaType for the current Media object</param>
        /// <param name="properties">Collection of properties</param>
        public Media(int parentId, IMediaType contentType, PropertyCollection properties)
        {
            _parentId = parentId;
            _contentTypeId = int.Parse(contentType.Id.ToString(CultureInfo.InvariantCulture));
            _contentType = contentType;
            _properties = properties;
            _properties.EnsurePropertyTypes(PropertyTypes);
        }

        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<Media, string>(x => x.Name);
        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<Media, int>(x => x.ParentId);
        private static readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<Media, int>(x => x.SortOrder);
        private static readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<Media, int>(x => x.Level);
        private static readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<Media, string>(x => x.Path);
        private static readonly PropertyInfo UserIdSelector = ExpressionHelper.GetPropertyInfo<Media, int>(x => x.UserId);
        private static readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<Media, bool>(x => x.Trashed);
        private static readonly PropertyInfo DefaultContentTypeIdSelector = ExpressionHelper.GetPropertyInfo<Media, int>(x => x.ContentTypeId);
        private readonly static PropertyInfo PropertyCollectionSelector = ExpressionHelper.GetPropertyInfo<Media, PropertyCollection>(x => x.Properties);
        void PropertiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(PropertyCollectionSelector);
        }

        /// <summary>
        /// Gets or Sets the Id of the Parent for the Media
        /// </summary>
        [DataMember]
        public int ParentId
        {
            get { return _parentId; }
            set
            {
                _parentId = value;
                OnPropertyChanged(ParentIdSelector);
            }
        }

        /// <summary>
        /// Gets or Sets the Name of the Media
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(NameSelector);
            }
        }

        /// <summary>
        /// Gets the Url name of the Media item
        /// </summary>
        [IgnoreDataMember]
        public string UrlName
        {
            //TODO: Should return the relative path to the media - if it should be implemented at all
            get
            {
                string url = string.Concat(SystemDirectories.Media.Replace("~", ""), "/", _name);
                return url;
            }
        }

        /// <summary>
        /// Gets or Sets the Sort Order of the Media
        /// </summary>
        [DataMember]
        public int SortOrder
        {
            get { return _sortOrder; }
            set
            {
                _sortOrder = value;
                OnPropertyChanged(SortOrderSelector);
            }
        }

        /// <summary>
        /// Gets or Sets the Level of the Media
        /// </summary>
        [DataMember]
        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                OnPropertyChanged(LevelSelector);
            }
        }

        /// <summary>
        /// Gets or Sets the Path of the Media
        /// </summary>
        [DataMember]
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged(PathSelector);
            }
        }

        /// <summary>
        /// Id of the user who created the Media
        /// </summary>
        [DataMember]
        public int UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                OnPropertyChanged(UserIdSelector);
            }
        }

        /// <summary>
        /// Boolean indicating whether this Media is Trashed or not.
        /// If Media is Trashed it will be located in the Recyclebin.
        /// </summary>
        [DataMember]
        public bool Trashed
        {
            get { return _trashed; }
            internal set
            {
                _trashed = value;
                OnPropertyChanged(TrashedSelector);
            }
        }

        /// <summary>
        /// Integer Id of the default MediaType
        /// </summary>
        [DataMember]
        public int ContentTypeId
        {
            get { return _contentTypeId; }
            protected set
            {
                _contentTypeId = value;
                OnPropertyChanged(DefaultContentTypeIdSelector);
            }
        }

        /// <summary>
        /// List of properties, which make up all the data available for this Media object
        /// </summary>
        [DataMember]
        public PropertyCollection Properties
        {
            get { return _properties; }
            set
            {
                _properties = value;
                _properties.CollectionChanged += PropertiesChanged;
            }
        }

        /// <summary>
        /// List of PropertyGroups available on this Media object
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyGroup> PropertyGroups
        {
            get { return _contentType.PropertyGroups; }
        }

        /// <summary>
        /// List of PropertyTypes available on this Media object
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyType> PropertyTypes
        {
            get { return _contentType.PropertyTypes; }
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
        /// Indicates whether the Media object has a property with the supplied alias
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns>True if Property with given alias exists, otherwise False</returns>
        public bool HasProperty(string propertyTypeAlias)
        {
            return Properties.Contains(propertyTypeAlias);
        }

        /// <summary>
        /// Gets the value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns><see cref="Property"/> Value as an <see cref="object"/></returns>
        public object GetValue(string propertyTypeAlias)
        {
            return Properties[propertyTypeAlias].Value;
        }

        /// <summary>
        /// Gets the value of a Property
        /// </summary>
        /// <typeparam name="TPassType">Type of the value to return</typeparam>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns><see cref="Property"/> Value as a <see cref="TPassType"/></returns>
        public TPassType GetValue<TPassType>(string propertyTypeAlias)
        {
            return (TPassType)Properties[propertyTypeAlias].Value;
        }

        /// <summary>
        /// Sets the value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public void SetValue(string propertyTypeAlias, object value)
        {
            if (Properties.Contains(propertyTypeAlias))
            {
                Properties[propertyTypeAlias].Value = value;
                return;
            }

            var propertyType = PropertyTypes.FirstOrDefault(x => x.Alias == propertyTypeAlias);
            if (propertyType == null)
            {
                throw new Exception(string.Format("No PropertyType exists with the supplied alias: {0}", propertyTypeAlias));
            }
            Properties.Add(propertyType.CreatePropertyFromValue(value));
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
            _properties.EnsurePropertyTypes(PropertyTypes);
            _properties.CollectionChanged += PropertiesChanged;
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
                _properties.EnsureCleanPropertyTypes(PropertyTypes);
                _properties.CollectionChanged += PropertiesChanged;
                return;
            }

            ChangeContentType(contentType);
        }
    }
}