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
        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private int _creatorId;
        private int _level;
        private string _name;
        private int _parentId;
        private string _path;
        private int _sortOrder;
        private bool _trashed;
        private bool _hasChildren;
        private bool _published;
        private bool _edited;
        private string _contentTypeAlias;
        private Guid _nodeObjectTypeId;

        // ReSharper disable once ClassNeverInstantiated.Local
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
            public readonly PropertyInfo PublishedSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.Published);
            public readonly PropertyInfo EditedSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.Edited);
            public readonly PropertyInfo ContentTypeAliasSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.ContentTypeAlias);
            public readonly PropertyInfo ContentTypeIconSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.ContentTypeIcon);
            public readonly PropertyInfo ContentTypeThumbnailSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.ContentTypeThumbnail);
            public readonly PropertyInfo NodeObjectTypeIdSelector = ExpressionHelper.GetPropertyInfo<UmbracoEntity, Guid>(x => x.NodeObjectTypeId);
        }

        private string _contentTypeIcon;
        private string _contentTypeThumbnail;

        public static readonly UmbracoEntity Root = new UmbracoEntity(false) { Path = "-1", Name = "root", HasChildren = true };

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
            get => _creatorId;
            set => SetPropertyValueAndDetectChanges(value, ref _creatorId, Ps.Value.CreatorIdSelector);
        }

        public int Level
        {
            get => _level;
            set => SetPropertyValueAndDetectChanges(value, ref _level, Ps.Value.LevelSelector);
        }

        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector);
        }

        public int ParentId
        {
            get => _parentId;
            set => SetPropertyValueAndDetectChanges(value, ref _parentId, Ps.Value.ParentIdSelector);
        }

        public string Path
        {
            get => _path;
            set => SetPropertyValueAndDetectChanges(value, ref _path, Ps.Value.PathSelector);
        }

        public int SortOrder
        {
            get => _sortOrder;
            set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, Ps.Value.SortOrderSelector);
        }

        public bool Trashed
        {
            get => _trashed;
            private set => SetPropertyValueAndDetectChanges(value, ref _trashed, Ps.Value.TrashedSelector);
        }

        public IDictionary<string, object> AdditionalData { get; }


        public bool HasChildren
        {
            get => _hasChildren;
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _hasChildren, Ps.Value.HasChildrenSelector);
                AdditionalData["HasChildren"] = value; // custom and not in IUmbracoEntity
            }
        }

        public bool Published
        {
            get => _published;
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _published, Ps.Value.PublishedSelector);
                AdditionalData["IsPublished"] = value; // custom and not in IUmbracoEntity
            }
        }

        public bool Edited
        {
            get => _edited;
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _edited, Ps.Value.EditedSelector);
                AdditionalData["IsEdited"] = value; // custom and not in IUmbracoEntity
            }
        }

        public string ContentTypeAlias
        {
            get => _contentTypeAlias;
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _contentTypeAlias, Ps.Value.ContentTypeAliasSelector);
                AdditionalData["ContentTypeAlias"] = value; // custom and not in IUmbracoEntity
            }
        }

        public string ContentTypeIcon
        {
            get => _contentTypeIcon;
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _contentTypeIcon, Ps.Value.ContentTypeIconSelector);
                AdditionalData["ContentTypeIcon"] = value; // custom and not in IUmbracoEntity
            }
        }

        public string ContentTypeThumbnail
        {
            get => _contentTypeThumbnail;
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _contentTypeThumbnail, Ps.Value.ContentTypeThumbnailSelector);
                AdditionalData["ContentTypeThumbnail"] = value; // custom and not in IUmbracoEntity
            }
        }

        public Guid NodeObjectTypeId
        {
            get => _nodeObjectTypeId;
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _nodeObjectTypeId, Ps.Value.NodeObjectTypeIdSelector);
                AdditionalData["NodeObjectTypeId"] = value; // custom and not in IUmbracoEntity
            }
        }

        public override object DeepClone()
        {
            var clone = (UmbracoEntity) base.DeepClone();

            // turn off change tracking
            clone.DisableChangeTracking();

            // ensure that any value in the dictionary that is deep cloneable is cloned too
            foreach (var key in clone.AdditionalData.Keys.ToArray())
            {
                if (clone.AdditionalData[key] is IDeepCloneable deepCloneable)
                    clone.AdditionalData[key] = deepCloneable.DeepClone();
            }

            // re-enable tracking
            clone.ResetDirtyProperties(false); // why? were not tracking
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
                return PropertyEditorAlias.Equals(other.PropertyEditorAlias) && Equals(Value, other.Value);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
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
