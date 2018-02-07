using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
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

        private PropertyEditor _editor;
        private ValueStorageType _databaseType;
        private object _configuration;
        private bool _hasConfiguration;
        private string _configurationJson;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataType"/> class.
        /// </summary>
        public DataType(PropertyEditor editor, int parentId = -1)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            ParentId = parentId;
        }

        private static PropertySelectors Selectors => _selectors ?? (_selectors = new PropertySelectors());

        private class PropertySelectors
        {
            public readonly PropertyInfo Editor = ExpressionHelper.GetPropertyInfo<DataType, PropertyEditor>(x => x.Editor);
            public readonly PropertyInfo DatabaseType = ExpressionHelper.GetPropertyInfo<DataType, ValueStorageType>(x => x.DatabaseType);
            public readonly PropertyInfo Configuration = ExpressionHelper.GetPropertyInfo<DataType, object>(x => x.Configuration);
        }

        /// <inheritdoc />
        [IgnoreDataMember]
        public PropertyEditor Editor
        {
            get => _editor;
            set
            {
                // ignore if no change
                if (_editor.Alias == value.Alias) return;
                OnPropertyChanged(Selectors.Editor);

                // try to map the existing configuration to the new configuration
                // simulate saving to db and reloading (ie go via json)
                var configuration = Configuration;
                var json = JsonConvert.SerializeObject(configuration);
                _editor = value;
                Configuration = _editor.ConfigurationEditor.FromDatabase(json);
            }
        }

        /// <inheritdoc />
        [DataMember]
        public string EditorAlias => _editor.Alias;

        /// <inheritdoc />
        [DataMember]
        public ValueStorageType DatabaseType
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
                // if we know we have a configuration (which may be null), return it
                // if we don't have an editor, then we have no configuration, return null
                // else, use the editor to get the configuration object

                if (_hasConfiguration) return _configuration;

                _configuration = _editor.ConfigurationEditor.FromDatabase(_configurationJson);
                _hasConfiguration = true;
                _configurationJson = null;

                return _configuration;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                // we don't support re-assigning the same object
                // configurations are kinda non-mutable, mainly because detecting changes would be a pain
                if (_configuration == value) // reference comparison
                    throw new ArgumentException("Configurations are kinda non-mutable. Do not reassign the same object.", nameof(value));

                // validate configuration type
                if (!_editor.ConfigurationEditor.IsConfiguration(value))
                    throw new ArgumentException($"Value of type {value.GetType().Name} cannot be a configuration for editor {_editor.Alias}, expecting.", nameof(value));

                // extract database type from configuration object, if appropriate
                if (value is IConfigureValueType valueTypeConfiguration)
                    DatabaseType = ValueTypes.ToStorageType(valueTypeConfiguration.ValueType);

                // extract database type from dictionary, if appropriate
                if (value is IDictionary<string, object> dictionaryConfiguration
                    && dictionaryConfiguration.TryGetValue(Constants.PropertyEditors.ConfigurationKeys.DataValueType, out var valueTypeObject)
                    && valueTypeObject is string valueTypeString
                    && ValueTypes.IsValue(valueTypeString))
                    DatabaseType = ValueTypes.ToStorageType(valueTypeString);

                _configuration = value;
                _hasConfiguration = true;
                _configurationJson = null;

                // it's always a change
                OnPropertyChanged(Selectors.Configuration);
            }
        }

        public abstract class EditorConfiguration
        {
            public abstract bool Equals(EditorConfiguration other);
        }

        /// <summary>
        /// Lazily set the configuration as a serialized json string.
        /// </summary>
        /// <remarks>
        /// <para>Will be de-serialized on-demand.</para>
        /// <para>This method is meant to be used when building entities from database, exclusively.
        /// It does NOT register a property change to dirty. It ignores the fact that the configuration
        /// may contain the database type, because the datatype DTO should also contain that database
        /// type, and they should be the same.</para>
        /// <para>Think before using!</para>
        /// </remarks>
        internal void SetConfiguration(string configurationJson)
        {
            _hasConfiguration = false;
            _configuration = null;
            _configurationJson = configurationJson;
        }
    }
}
