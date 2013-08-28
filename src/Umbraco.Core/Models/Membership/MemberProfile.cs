using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Membership
{
    internal abstract class MemberProfile : Profile, IUmbracoEntity
    {
        private Lazy<int> _parentId;
        private int _sortOrder;
        private int _level;
        private string _path;
        private int _creatorId;
        private bool _trashed;
        private PropertyCollection _properties;

        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<MemberProfile, int>(x => ((IUmbracoEntity)x).ParentId);
        private static readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<MemberProfile, int>(x => ((IUmbracoEntity)x).SortOrder);
        private static readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<MemberProfile, int>(x => ((IUmbracoEntity)x).Level);
        private static readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<MemberProfile, string>(x => ((IUmbracoEntity)x).Path);
        private static readonly PropertyInfo CreatorIdSelector = ExpressionHelper.GetPropertyInfo<MemberProfile, int>(x => ((IUmbracoEntity)x).CreatorId);
        private static readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<MemberProfile, bool>(x => x.Trashed);
        private readonly static PropertyInfo PropertyCollectionSelector = ExpressionHelper.GetPropertyInfo<Member, PropertyCollection>(x => x.Properties);

        protected void PropertiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(PropertyCollectionSelector);
        }

        public abstract new int Id { get; set; }
        public abstract Guid Key { get; set; }
        public abstract DateTime CreateDate { get; set; }
        public abstract DateTime UpdateDate { get; set; }
        public abstract bool HasIdentity { get; protected set; }

        /// <summary>
        /// Profile of the user who created this Content
        /// </summary>
        [DataMember]
        int IUmbracoEntity.CreatorId
        {
            get
            {
                return _creatorId;
            }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _creatorId = value;
                    return _creatorId;
                }, _creatorId, CreatorIdSelector);
            }
        }

        /// <summary>
        /// Gets or sets the level of the content entity
        /// </summary>
        [DataMember]
        int IUmbracoEntity.Level
        { 
            get { return _level; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _level = value;
                    return _level;
                }, _level, LevelSelector);
            } }

        /// <summary>
        /// Gets or sets the Id of the Parent entity
        /// </summary>
        [DataMember]
        int IUmbracoEntity.ParentId
        {
            get
            {
                var val = _parentId.Value;
                if (val == 0)
                {
                    throw new InvalidOperationException("The ParentId cannot have a value of 0. Perhaps the parent object used to instantiate this object has not been persisted to the data store.");
                }
                return val;
            }
            set
            {
                _parentId = new Lazy<int>(() => value);
                OnPropertyChanged(ParentIdSelector);
            }
        }

        /// <summary>
        /// Gets or sets the path
        /// </summary>
        [DataMember]
        string IUmbracoEntity.Path
        {
            get { return _path; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _path = value;
                    return _path;
                }, _path, PathSelector);
            }
        }

        /// <summary>
        /// Gets or sets the sort order of the content entity
        /// </summary>
        [DataMember]
        int IUmbracoEntity.SortOrder
        {
            get { return _sortOrder; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _sortOrder = value;
                    return _sortOrder;
                }, _sortOrder, SortOrderSelector);
            }
        }

        /// <summary>
        /// Boolean indicating whether this Content is Trashed or not.
        /// If Content is Trashed it will be located in the Recyclebin.
        /// </summary>
        [DataMember]
        public virtual bool Trashed
        {
            get { return _trashed; }
            internal set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _trashed = value;
                    return _trashed;
                }, _trashed, TrashedSelector);
            }
        }

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
    }
}