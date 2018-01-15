using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Implements <see cref="IDataType"/>.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class DataType : TreeEntityBase, IDataType
    {
        private static PropertySelectors _selectors;

        private string _editorAlias;
        private DataTypeDatabaseType _databaseType;
        private object _configuration;
        private bool _hasConfiguration;
        private string _configurationJson;
        private PropertyEditor _editor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataType"/> class.
        /// </summary>
        public DataType(int parentId, string propertyEditorAlias)
        {
            ParentId = parentId;
            _editorAlias = propertyEditorAlias;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataType"/> class.
        /// </summary>
        public DataType(string propertyEditorAlias)
        {
            ParentId = -1;
            _editorAlias = propertyEditorAlias;
        }

        private static PropertySelectors Selectors => _selectors ?? (_selectors = new PropertySelectors());

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo EditorAlias = ExpressionHelper.GetPropertyInfo<DataType, string>(x => x.EditorAlias);
            public readonly PropertyInfo DatabaseType = ExpressionHelper.GetPropertyInfo<DataType, DataTypeDatabaseType>(x => x.DatabaseType);
            public readonly PropertyInfo Configuration = ExpressionHelper.GetPropertyInfo<DataType, object>(x => x.Configuration);
        }

        /// <inheritdoc />
        [DataMember]
        public string EditorAlias
        {
            get => _editorAlias;
            set => SetPropertyValueAndDetectChanges(value, ref _editorAlias, Selectors.EditorAlias);
        }

        /// <inheritdoc />
        [DataMember]
        public DataTypeDatabaseType DatabaseType
        {
            get => _databaseType;
            set => SetPropertyValueAndDetectChanges(value, ref _databaseType, Selectors.DatabaseType);
        }

        /// <inheritdoc />
        [DataMember]
        public object Configuration
        {
            get
            {
                if (_hasConfiguration) return _configuration;

                _configuration = _editor.DeserializeConfiguration(_configurationJson);
                _hasConfiguration = true;
                _configurationJson = null;
                return _configuration;
            }
            set
            {
                // fixme detect changes? if it's the same object?
                SetPropertyValueAndDetectChanges(value, ref _configuration, Selectors.Configuration);
                _hasConfiguration = true;
                _configurationJson = null;
            }
        }

        /// <inheritdoc /> // fixme on interface!?
        public void SetConfiguration(string configurationJson, PropertyEditor editor)
        {
            _hasConfiguration = false;
            _configuration = null;
            _configurationJson = configurationJson;
            _editor = editor;
            OnPropertyChanged(Selectors.Configuration);
        }
    }
}
