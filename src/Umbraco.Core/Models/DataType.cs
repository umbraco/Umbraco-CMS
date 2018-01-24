using System;
using System.Collections.Generic;
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
        private ValueStorageType _databaseType;
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
            public readonly PropertyInfo DatabaseType = ExpressionHelper.GetPropertyInfo<DataType, ValueStorageType>(x => x.DatabaseType);
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
        public ValueStorageType DatabaseType
        {
            get => _databaseType;
            set => SetPropertyValueAndDetectChanges(value, ref _databaseType, Selectors.DatabaseType);
        }

        /// <inheritdoc />
        [DataMember]
        public object Configuration // fixme is this OK if null? should!
        {
            get
            {
                if (_hasConfiguration) return _configuration;

                _configuration = _editor.ConfigurationEditor.ParseConfiguration(_configurationJson);
                _hasConfiguration = true;
                _configurationJson = null;
                return _configuration;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(); // or is this ok?

                // fixme - do it HERE
                // fixme - BUT then we have a problem, what if it's changed?
                // can't we treat configurations as plain immutable objects?!
                // also it means that we just cannot work with dictionaries?
                if (value is IConfigureValueType valueTypeConfiguration)
                    DatabaseType = ValueTypes.ToStorageType(valueTypeConfiguration.ValueType);
                if (value is IDictionary<string, object> dictionaryConfiguration
                    && dictionaryConfiguration.TryGetValue("", out var valueTypeObject)
                    && valueTypeObject is string valueTypeString)
                    DatabaseType = ValueTypes.ToStorageType(valueTypeString);

                // fixme detect changes? if it's the same object? need a special comparer!
                SetPropertyValueAndDetectChanges(value, ref _configuration, Selectors.Configuration);
                _hasConfiguration = true;
                _configurationJson = null;
            }
        }

        /// <inheritdoc /> // fixme on interface!?
        public void SetConfiguration(string configurationJson, PropertyEditor editor)
        {
            // fixme this is lazy, BUT then WHEN are we figuring out the valueType?
            _hasConfiguration = false;
            _configuration = null;
            _configurationJson = configurationJson;
            _editor = editor;
            OnPropertyChanged(Selectors.Configuration);
        }
    }
}
