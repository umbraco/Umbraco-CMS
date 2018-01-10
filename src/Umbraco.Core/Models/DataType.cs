using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Implements <see cref="IDataType"/>.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class DataType : EntityBase.EntityBase, IDataType
    {
        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();
        private readonly IDictionary<string, object> _additionalData;

        private int _parentId;
        private string _name;
        private int _sortOrder;
        private int _level;
        private string _path;
        private int _creatorId;
        private bool _trashed;
        private string _propertyEditorAlias;
        private DataTypeDatabaseType _databaseType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataType"/> class.
        /// </summary>
        public DataType(int parentId, string propertyEditorAlias)
        {
            _parentId = parentId;
            _propertyEditorAlias = propertyEditorAlias;

            _additionalData = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataType"/> class.
        /// </summary>
        public DataType(string propertyEditorAlias)
        {
            _parentId = -1;
            _propertyEditorAlias = propertyEditorAlias;

            _additionalData = new Dictionary<string, object>();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<DataType, string>(x => x.Name);
            public readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<DataType, int>(x => x.ParentId);
            public readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<DataType, int>(x => x.SortOrder);
            public readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<DataType, int>(x => x.Level);
            public readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<DataType, string>(x => x.Path);
            public readonly PropertyInfo UserIdSelector = ExpressionHelper.GetPropertyInfo<DataType, int>(x => x.CreatorId);
            public readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<DataType, bool>(x => x.Trashed);
            public readonly PropertyInfo PropertyEditorAliasSelector = ExpressionHelper.GetPropertyInfo<DataType, string>(x => x.EditorAlias);
            public readonly PropertyInfo DatabaseTypeSelector = ExpressionHelper.GetPropertyInfo<DataType, DataTypeDatabaseType>(x => x.DatabaseType);
        }

        /// <inheritdoc />
        [DataMember]
        public int ParentId
        {
            get => _parentId;
            set => SetPropertyValueAndDetectChanges(value, ref _parentId, Ps.Value.ParentIdSelector);
        }

        /// <inheritdoc />
        [DataMember]
        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector);
        }

        /// <inheritdoc />
        [DataMember]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, Ps.Value.SortOrderSelector);
        }

        /// <inheritdoc />
        [DataMember]
        public int Level
        {
            get => _level;
            set => SetPropertyValueAndDetectChanges(value, ref _level, Ps.Value.LevelSelector);
        }

        /// <inheritdoc />
        // fixme - setting this value should be handled by the class not the user
        [DataMember]
        public string Path
        {
            get => _path;
            set => SetPropertyValueAndDetectChanges(value, ref _path, Ps.Value.PathSelector);
        }

        /// <inheritdoc />
        [DataMember]
        public int CreatorId
        {
            get => _creatorId;
            set => SetPropertyValueAndDetectChanges(value, ref _creatorId, Ps.Value.UserIdSelector);
        }

        /// <inheritdoc />
        // fixme - data types cannot be trashed?
        [DataMember]
        public bool Trashed
        {
            get => _trashed;
            internal set
            {
                SetPropertyValueAndDetectChanges(value, ref _trashed, Ps.Value.TrashedSelector);

                // this is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                _additionalData["Trashed"] = value;
            }
        }

        // fixme - what exactly are we doing with _additionalData?
        // are we allocating 1 dictionary for *every* entity?
        // not doing it for other entities?

        /// <inheritdoc />
        [DataMember]
        public string EditorAlias
        {
            get => _propertyEditorAlias;
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _propertyEditorAlias, Ps.Value.PropertyEditorAliasSelector);

                // this is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                _additionalData["DatabaseType"] = value;
            }
        }

        /// <inheritdoc />
        [DataMember]
        public DataTypeDatabaseType DatabaseType
        {
            get => _databaseType;
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _databaseType, Ps.Value.DatabaseTypeSelector);

                // this is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                _additionalData["DatabaseType"] = value;
            }
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        IDictionary<string, object> IUmbracoEntity.AdditionalData => _additionalData;
    }
}
