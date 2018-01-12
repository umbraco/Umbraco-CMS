using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    // fixme - changing the name of some properties that were in additionalData => must update corresponding javascript?

    public class UmbracoContentEntity : UmbracoEntity
    {
        private static PropertySelectors _selectors;
        private static PropertySelectors Selectors => _selectors ?? (_selectors = new PropertySelectors());

        private string _contentTypeAlias;

        private class PropertySelectors
        {
            public readonly PropertyInfo ContentTypeAlias = ExpressionHelper.GetPropertyInfo<UmbracoContentEntity, string>(x => x.ContentTypeAlias);
        }

        public string ContentTypeAlias
        {
            get => _contentTypeAlias;
            set => SetPropertyValueAndDetectChanges(value, ref _contentTypeAlias, Selectors.ContentTypeAlias);
        }
    }

    public class UmbracoDocumentEntity : UmbracoContentEntity
    {
        private static PropertySelectors _selectors;
        private static PropertySelectors Selectors => _selectors ?? (_selectors = new PropertySelectors());

        private bool _published;
        private bool _edited;

        private class PropertySelectors
        {
            public readonly PropertyInfo Published = ExpressionHelper.GetPropertyInfo<UmbracoDocumentEntity, bool>(x => x.Published);
            public readonly PropertyInfo Edited = ExpressionHelper.GetPropertyInfo<UmbracoDocumentEntity, bool>(x => x.Edited);
        }

        public bool Published
        {
            get => _published;
            set => SetPropertyValueAndDetectChanges(value, ref _published, Selectors.Published);
        }

        public bool Edited
        {
            get => _edited;
            set => SetPropertyValueAndDetectChanges(value, ref _edited, Selectors.Edited);
        }
    }

    /// <summary>
    /// Implementation of the <see cref="IUmbracoEntity"/> for internal use.
    /// </summary>
    public class UmbracoEntity : EntityBase.EntityBase, IUmbracoEntity
    {
        private static PropertySelectors _selectors;
        private static PropertySelectors Selectors => _selectors ?? (_selectors = new PropertySelectors());

        private Guid _nodeObjectType;

        private int _creatorId;

        private int _level;
        private string _name;
        private int _parentId;
        private string _path;
        private int _sortOrder;
        private bool _trashed;

        private bool _hasChildren;

        // fixme - usage
        private string _contentTypeIcon;
        private string _contentTypeThumbnail;

        // fixme - are we tracking changes on something that's basically READONLY?
        private class PropertySelectors
        {
            public readonly PropertyInfo CreatorId = ExpressionHelper.GetPropertyInfo<UmbracoEntity, int>(x => x.CreatorId);
            public readonly PropertyInfo Level = ExpressionHelper.GetPropertyInfo<UmbracoEntity, int>(x => x.Level);
            public readonly PropertyInfo Name = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.Name);
            public readonly PropertyInfo ParentId = ExpressionHelper.GetPropertyInfo<UmbracoEntity, int>(x => x.ParentId);
            public readonly PropertyInfo Path = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.Path);
            public readonly PropertyInfo SortOrder = ExpressionHelper.GetPropertyInfo<UmbracoEntity, int>(x => x.SortOrder);
            public readonly PropertyInfo Trashed = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.Trashed);
            public readonly PropertyInfo HasChildren = ExpressionHelper.GetPropertyInfo<UmbracoEntity, bool>(x => x.HasChildren);
            public readonly PropertyInfo NodeObjectType = ExpressionHelper.GetPropertyInfo<UmbracoEntity, Guid>(x => x.NodeObjectType);
            public readonly PropertyInfo ContentTypeIcon = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.ContentTypeIcon);
            public readonly PropertyInfo ContentTypeThumbnail = ExpressionHelper.GetPropertyInfo<UmbracoEntity, string>(x => x.ContentTypeThumbnail);
        }

        public static readonly UmbracoEntity Root = new UmbracoEntity { Path = "-1", Name = "root", HasChildren = true };

        public UmbracoEntity()
        {
            AdditionalData = new Dictionary<string, object>();
        }

        public int CreatorId
        {
            get => _creatorId;
            set => SetPropertyValueAndDetectChanges(value, ref _creatorId, Selectors.CreatorId);
        }

        public int Level
        {
            get => _level;
            set => SetPropertyValueAndDetectChanges(value, ref _level, Selectors.Level);
        }

        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, Selectors.Name);
        }

        public int ParentId
        {
            get => _parentId;
            set => SetPropertyValueAndDetectChanges(value, ref _parentId, Selectors.ParentId);
        }

        public string Path
        {
            get => _path;
            set => SetPropertyValueAndDetectChanges(value, ref _path, Selectors.Path);
        }

        public int SortOrder
        {
            get => _sortOrder;
            set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, Selectors.SortOrder);
        }

        public bool Trashed
        {
            get => _trashed;
            set => SetPropertyValueAndDetectChanges(value, ref _trashed, Selectors.Trashed);
        }

        public bool HasChildren
        {
            get => _hasChildren;
            set => SetPropertyValueAndDetectChanges(value, ref _hasChildren, Selectors.HasChildren);
        }

        public Guid NodeObjectType
        {
            get => _nodeObjectType;
            set => SetPropertyValueAndDetectChanges(value, ref _nodeObjectType, Selectors.NodeObjectType);
        }





        public IDictionary<string, object> AdditionalData { get; }


        public string ContentTypeIcon
        {
            get => _contentTypeIcon;
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _contentTypeIcon, Selectors.ContentTypeIcon);
                AdditionalData["ContentTypeIcon"] = value; // custom and not in IUmbracoEntity
            }
        }

        public string ContentTypeThumbnail
        {
            get => _contentTypeThumbnail;
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _contentTypeThumbnail, Selectors.ContentTypeThumbnail);
                AdditionalData["ContentTypeThumbnail"] = value; // custom and not in IUmbracoEntity
            }
        }

        public override object DeepClone()
        {
            var clone = (UmbracoEntity) base.DeepClone();

            // disable change tracking
            clone.DisableChangeTracking();

            // deep clone additional data properties
            // fixme - BUT the values are... only set in EntityRepository to non-deepclonable stuff?!
            foreach (var key in clone.AdditionalData.Keys.ToArray())
            {
                if (clone.AdditionalData[key] is IDeepCloneable deepCloneable)
                    clone.AdditionalData[key] = deepCloneable.DeepClone();
            }

            // enable tracking
            clone.EnableChangeTracking();

            return clone;
        }

        // fixme
        // wtf? is clone.AdditionalData at least shallow cloned?
        // and, considering the only thing we put in EntityProperty are strings,
        // what's the point of EntityProperty ???

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
