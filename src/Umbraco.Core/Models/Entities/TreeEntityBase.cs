using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Entities
{
    /// <summary>
    /// Provides a base class for tree entities.
    /// </summary>
    public abstract class TreeEntityBase : EntityBase, ITreeEntity
    {
        private static PropertySelectors _selectors;
        private static PropertySelectors Selectors => _selectors ?? (_selectors = new PropertySelectors());

        private string _name;
        private int _creatorId;
        private int _parentId;
        private bool _hasParentId;
        private ITreeEntity _parent;
        private int _level;
        private string _path;
        private int _sortOrder;
        private bool _trashed;

        private class PropertySelectors
        {
            public readonly PropertyInfo Name = ExpressionHelper.GetPropertyInfo<ContentBase, string>(x => x.Name);
            public readonly PropertyInfo CreatorId = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.CreatorId);
            public readonly PropertyInfo ParentId = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.ParentId);
            public readonly PropertyInfo Level = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.Level);
            public readonly PropertyInfo Path = ExpressionHelper.GetPropertyInfo<ContentBase, string>(x => x.Path);
            public readonly PropertyInfo SortOrder = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.SortOrder);
            public readonly PropertyInfo Trashed = ExpressionHelper.GetPropertyInfo<ContentBase, bool>(x => x.Trashed);
        }

        // fixme
        // ParentId, Path, Level and Trashed all should be consistent, and all derive from parentId, really

        /// <inheritdoc />
        [DataMember]
        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, Selectors.Name);
        }

        /// <inheritdoc />
        [DataMember]
        public int CreatorId
        {
            get => _creatorId;
            set => SetPropertyValueAndDetectChanges(value, ref _creatorId, Selectors.CreatorId);
        }

        /// <inheritdoc />
        [DataMember]
        public int ParentId
        {
            get
            {
                if (_hasParentId) return _parentId;

                if (_parent == null) throw new InvalidOperationException("Content does not have a parent.");
                if (!_parent.HasIdentity) throw new InvalidOperationException("Content's parent does not have an identity.");

                _parentId = _parent.Id;
                if (_parentId == 0)
                    throw new Exception("Panic: parent has an identity but id is zero.");

                _hasParentId = true;
                _parent = null;
                return _parentId;
            }
            set
            {
                if (value == 0)
                    throw new ArgumentException("Value cannot be zero.", nameof(value));
                SetPropertyValueAndDetectChanges(value, ref _parentId, Selectors.ParentId);
                _hasParentId = true;
                _parent = null;
            }
        }

        /// <inheritdoc />
        public void SetParent(ITreeEntity parent)
        {
            _hasParentId = false;
            _parent = parent;
            OnPropertyChanged(Selectors.ParentId);
        }

        /// <inheritdoc />
        [DataMember]
        public int Level
        {
            get => _level;
            set => SetPropertyValueAndDetectChanges(value, ref _level, Selectors.Level);
        }

        /// <inheritdoc />
        [DataMember]
        public string Path
        {
            get => _path;
            set => SetPropertyValueAndDetectChanges(value, ref _path, Selectors.Path);
        }

        /// <inheritdoc />
        [DataMember]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, Selectors.SortOrder);
        }

        /// <inheritdoc />
        [DataMember]
        public bool Trashed
        {
            get => _trashed;
            set => SetPropertyValueAndDetectChanges(value, ref _trashed, Selectors.Trashed);
        }
    }
}