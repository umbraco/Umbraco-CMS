using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Implements <see cref="IDataType"/>.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class DataType : TreeEntityBase, IDataType
    {
        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();
        private IDictionary<string, object> _additionalData;

        private string _propertyEditorAlias;
        private DataTypeDatabaseType _databaseType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataType"/> class.
        /// </summary>
        public DataType(int parentId, string propertyEditorAlias)
        {
            ParentId = parentId;
            _propertyEditorAlias = propertyEditorAlias;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataType"/> class.
        /// </summary>
        public DataType(string propertyEditorAlias)
        {
            ParentId = -1;
            _propertyEditorAlias = propertyEditorAlias;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo PropertyEditorAliasSelector = ExpressionHelper.GetPropertyInfo<DataType, string>(x => x.EditorAlias);
            public readonly PropertyInfo DatabaseTypeSelector = ExpressionHelper.GetPropertyInfo<DataType, DataTypeDatabaseType>(x => x.DatabaseType);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DataMember]
        [DoNotClone]
        IDictionary<string, object> IUmbracoEntity.AdditionalData => _additionalData ?? (_additionalData = new Dictionary<string, object>());

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        [IgnoreDataMember]
        bool IUmbracoEntity.HasAdditionalData => _additionalData != null;

        /// <inheritdoc />
        [DataMember]
        public string EditorAlias
        {
            get => _propertyEditorAlias;
            set => SetPropertyValueAndDetectChanges(value, ref _propertyEditorAlias, Ps.Value.PropertyEditorAliasSelector);
        }

        /// <inheritdoc />
        [DataMember]
        public DataTypeDatabaseType DatabaseType
        {
            get => _databaseType;
            set => SetPropertyValueAndDetectChanges(value, ref _databaseType, Ps.Value.DatabaseTypeSelector);
        }

        // fixme - implement that one !!
        [DataMember]
        public object Configuration { get; set; }
    }
}
