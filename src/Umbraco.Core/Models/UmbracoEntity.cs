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
    public class UmbracoEntity : Entity, IUmbracoEntity
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
        
        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo CreatorIdSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, int>(x => x.CreatorId);
            public readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, int>(x => x.Level);
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.Name);
            public readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, int>(x => x.ParentId);
            public readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.Path);
            public readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, int>(x => x.SortOrder);
            public readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.Trashed);
            public readonly PropertyInfo HasChildrenSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.HasChildren);
            public readonly PropertyInfo IsPublishedSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.IsPublished);
            public readonly PropertyInfo IsDraftSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.IsDraft);
            public readonly PropertyInfo HasPendingChangesSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.HasPendingChanges);
            public readonly PropertyInfo ContentTypeAliasSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.ContentTypeAlias);
            public readonly PropertyInfo ContentTypeIconSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.ContentTypeIcon);
            public readonly PropertyInfo ContentTypeThumbnailSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.ContentTypeThumbnail);
            public readonly PropertyInfo NodeObjectTypeIdSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, Guid>(x => x.NodeObjectTypeId);
        }

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
            set { SetPropertyValueAndDetectChanges(value, ref _creatorId, Ps.Value.CreatorIdSelector); }
        }

        public int Level
        {
            get { return _level; }
            set { SetPropertyValueAndDetectChanges(value, ref _level, Ps.Value.LevelSelector); }
        }

        public string Name
        {
            get { return _name; }
            set { SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }
        }

        public int ParentId
        {
            get { return _parentId; }
            set { SetPropertyValueAndDetectChanges(value, ref _parentId, Ps.Value.ParentIdSelector); }
        }

        public string Path
        {
            get { return _path; }
            set { SetPropertyValueAndDetectChanges(value, ref _path, Ps.Value.PathSelector); }
        }

        public int SortOrder
        {
            get { return _sortOrder; }
            set { SetPropertyValueAndDetectChanges(value, ref _sortOrder, Ps.Value.SortOrderSelector); }
        }

        public bool Trashed
        {
            get { return _trashed; }
            private set { SetPropertyValueAndDetectChanges(value, ref _trashed, Ps.Value.TrashedSelector); }
        }

        public IDictionary<string, object> AdditionalData { get; private set; }


        public bool HasChildren
        {
            get { return _hasChildren; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _hasChildren, Ps.Value.HasChildrenSelector);
                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["HasChildren"] = value;
            }
        }

        public bool IsPublished
        {
            get { return _isPublished; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _isPublished, Ps.Value.IsPublishedSelector);
                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["IsPublished"] = value;
            }
        }

        public bool IsDraft
        {
            get { return _isDraft; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _isDraft, Ps.Value.IsDraftSelector);
                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["IsDraft"] = value;
            }
        }

        public bool HasPendingChanges
        {
            get { return _hasPendingChanges; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _hasPendingChanges, Ps.Value.HasPendingChangesSelector);                
                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["HasPendingChanges"] = value;
            }
        }

        public string ContentTypeAlias
        {
            get { return _contentTypeAlias; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _contentTypeAlias, Ps.Value.ContentTypeAliasSelector);                
                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["ContentTypeAlias"] = value;
            }
        }

        public string ContentTypeIcon
        {
            get { return _contentTypeIcon; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _contentTypeIcon, Ps.Value.ContentTypeIconSelector);                
                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["ContentTypeIcon"] = value;
            }
        }

        public string ContentTypeThumbnail
        {
            get { return _contentTypeThumbnail; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _contentTypeThumbnail, Ps.Value.ContentTypeThumbnailSelector);                
                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                AdditionalData["ContentTypeThumbnail"] = value;
            }
        }

        public Guid NodeObjectTypeId
        {
            get { return _nodeObjectTypeId; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _nodeObjectTypeId, Ps.Value.NodeObjectTypeIdSelector);
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