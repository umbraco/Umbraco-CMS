using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Implementation of the <see cref="IUmbracoEntity"/> for internal use.
    /// </summary>
    internal class UmbracoEntity : Entity, IUmbracoEntity
    {
        private int _creatorId;
        private int _level;
        private string _name;
        private int _parentId;
        private string _path;
        private int _sortOrder;
        private bool _trashed;
        private bool _hasChildren;
        private bool _isPublished;
        private bool _isDraft;
        private bool _hasPendingChanges;
        private string _contentTypeAlias;
        private Guid _nodeObjectTypeId;

        private static readonly PropertyInfo CreatorIdSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, int>(x => x.CreatorId);
        private static readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, int>(x => x.Level);
        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.Name);
        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, int>(x => x.ParentId);
        private static readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.Path);
        private static readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, int>(x => x.SortOrder);
        private static readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.Trashed);
        private static readonly PropertyInfo HasChildrenSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.HasChildren);
        private static readonly PropertyInfo IsPublishedSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.IsPublished);
        private static readonly PropertyInfo IsDraftSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.IsDraft);
        private static readonly PropertyInfo HasPendingChangesSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.HasPendingChanges);
        private static readonly PropertyInfo ContentTypeAliasSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.ContentTypeAlias);
        private static readonly PropertyInfo ContentTypeIconSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.ContentTypeIcon);
        private static readonly PropertyInfo ContentTypeThumbnailSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.ContentTypeThumbnail);
        private static readonly PropertyInfo NodeObjectTypeIdSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, Guid>(x => x.NodeObjectTypeId);
        private string _contentTypeIcon;
        private string _contentTypeThumbnail;

        public UmbracoEntity()
        {
            AdditionalData = new Dictionary<string, object>();
        }

        public UmbracoEntity(bool trashed)
        {
            AdditionalData = new Dictionary<string, object>();
            Trashed = trashed;
        }

        // for MySql
        public UmbracoEntity(UInt64 trashed)
        {
            AdditionalData = new Dictionary<string, object>();
            Trashed = trashed == 1;
        }

        public int CreatorId
        {
            get { return _creatorId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _creatorId = value;
                    return _creatorId;
                }, _creatorId, CreatorIdSelector);  
            }
        }

        public int Level
        {
            get { return _level; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _level = value;
                    return _level;
                }, _level, LevelSelector);  
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _name = value;
                    return _name;
                }, _name, NameSelector);  
            }
        }

        public int ParentId
        {
            get { return _parentId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _parentId = value;
                    return _parentId;
                }, _parentId, ParentIdSelector);  
            }
        }

        public string Path
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

        public int SortOrder
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

        public bool Trashed
        {
            get { return _trashed; }
            private set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _trashed = value;
                    return _trashed;
                }, _trashed, TrashedSelector);  
            }
        }

        public IDictionary<string, object> AdditionalData { get; private set; }


        public bool HasChildren
        {
            get { return _hasChildren; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _hasChildren = value;
                    return _hasChildren;
                }, _hasChildren, HasChildrenSelector);  

                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["HasChildren"] = value;
            }
        }

        public bool IsPublished
        {
            get { return _isPublished; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _isPublished = value;
                    return _isPublished;
                }, _isPublished, IsPublishedSelector);

                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["IsPublished"] = value;
            }
        }

        public bool IsDraft
        {
            get { return _isDraft; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _isDraft = value;
                    return _isDraft;
                }, _isDraft, IsDraftSelector);

                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["IsDraft"] = value;
            }
        }

        public bool HasPendingChanges
        {
            get { return _hasPendingChanges; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _hasPendingChanges = value;
                    return _hasPendingChanges;
                }, _hasPendingChanges, HasPendingChangesSelector);

                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["HasPendingChanges"] = value;
            }
        }

        public string ContentTypeAlias
        {
            get { return _contentTypeAlias; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _contentTypeAlias = value;
                    return _contentTypeAlias;
                }, _contentTypeAlias, ContentTypeAliasSelector);

                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["ContentTypeAlias"] = value;
            }
        }

        public string ContentTypeIcon
        {
            get { return _contentTypeIcon; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _contentTypeIcon = value;
                    return _contentTypeIcon;
                }, _contentTypeIcon, ContentTypeIconSelector);

                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["ContentTypeIcon"] = value;
            }
        }

        public string ContentTypeThumbnail
        {
            get { return _contentTypeThumbnail; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _contentTypeThumbnail = value;
                    return _contentTypeThumbnail;
                }, _contentTypeThumbnail, ContentTypeThumbnailSelector);

                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["ContentTypeThumbnail"] = value;
            }
        }

        public Guid NodeObjectTypeId
        {
            get { return _nodeObjectTypeId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _nodeObjectTypeId = value;
                    return _nodeObjectTypeId;
                }, _nodeObjectTypeId, NodeObjectTypeIdSelector);

                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["NodeObjectTypeId"] = value;
            }
        }

        public override object DeepClone()
        {
            var clone = (UmbracoEntity) base.DeepClone();
            //turn off change tracking
            clone.DisableChangeTracking();
            //This ensures that any value in the dictionary that is deep cloneable is cloned too
            foreach (var key in clone.AdditionalData.Keys.ToArray())
            {
                var deepCloneable = clone.AdditionalData[key] as IDeepCloneable;
                if (deepCloneable != null)
                {
                    clone.AdditionalData[key] = deepCloneable.DeepClone();
                }
            }
            //this shouldn't really be needed since we're not tracking
            clone.ResetDirtyProperties(false);
            //re-enable tracking
            clone.EnableChangeTracking();
            return clone;
        }

        /// <summary>
        /// A struction that can be contained in the additional data of an UmbracoEntity representing 
        /// a user defined property
        /// </summary>
        public class EntityProperty : IDeepCloneable
        {
            public string PropertyEditorAlias { get; set; }
            public object Value { get; set; }
            public object DeepClone()
            {
                //Memberwise clone on Entity will work since it doesn't have any deep elements
                // for any sub class this will work for standard properties as well that aren't complex object's themselves.
                var clone = MemberwiseClone();
                return clone;
            }

            protected bool Equals(EntityProperty other)
            {
                return PropertyEditorAlias.Equals(other.PropertyEditorAlias) && string.Equals(Value, other.Value);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((EntityProperty) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (PropertyEditorAlias.GetHashCode() * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                }
            }
        }
    }
}