using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class DataTypeMapDefinition : IMapDefinition
    {
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly ILogger _logger;

        public DataTypeMapDefinition(PropertyEditorCollection propertyEditors, ILogger logger)
        {
            _propertyEditors = propertyEditors;
            _logger = logger;
        }

        private static readonly int[] SystemIds =
        {
            Constants.DataTypes.DefaultContentListView,
            Constants.DataTypes.DefaultMediaListView,
            Constants.DataTypes.DefaultMembersListView
        };

        public void DefineMaps(UmbracoMapper mapper)
        {
            mapper.Define<IDataEditor, PropertyEditorBasic>((source, context) => new PropertyEditorBasic(), Map);
            mapper.Define<ConfigurationField, DataTypeConfigurationFieldDisplay>((source, context) => new DataTypeConfigurationFieldDisplay(), Map);
            mapper.Define<IDataEditor, DataTypeBasic>((source, context) => new DataTypeBasic(), Map);
            mapper.Define<IDataType, DataTypeBasic>((source, context) => new DataTypeBasic(), Map);
            mapper.Define<IDataType, DataTypeDisplay>((source, context) => new DataTypeDisplay(), Map);
            mapper.Define<IDataType, IEnumerable<DataTypeConfigurationFieldDisplay>>(MapPreValues);
            mapper.Define<DataTypeSave, IDataType>((source, context) => new DataType(_propertyEditors[source.EditorAlias]) { CreateDate = DateTime.Now },Map);
            mapper.Define<IDataEditor, IEnumerable<DataTypeConfigurationFieldDisplay>>(MapPreValues);
        }

        // Umbraco.Code.MapAll
        private static void Map(IDataEditor source, PropertyEditorBasic target, MapperContext context)
        {
            target.Alias = source.Alias;
            target.Icon = source.Icon;
            target.Name = source.Name;
        }

        // Umbraco.Code.MapAll -Value
        private static void Map(ConfigurationField source, DataTypeConfigurationFieldDisplay target, MapperContext context)
        {
            target.Config = source.Config;
            target.Description = source.Description;
            target.HideLabel = source.HideLabel;
            target.Key = source.Key;
            target.Name = source.Name;
            target.View = source.View;
        }

        // Umbraco.Code.MapAll -Udi -HasPrevalues -IsSystemDataType -Id -Trashed -Key
        // Umbraco.Code.MapAll -ParentId -Path
        private static void Map(IDataEditor source, DataTypeBasic target, MapperContext context)
        {
            target.Alias = source.Alias;
            target.Group = source.Group;
            target.Icon = source.Icon;
            target.Name = source.Name;
        }

        // Umbraco.Code.MapAll -HasPrevalues
        private void Map(IDataType source, DataTypeBasic target, MapperContext context)
        {
            target.Id = source.Id;
            target.IsSystemDataType = SystemIds.Contains(source.Id);
            target.Key = source.Key;
            target.Name = source.Name;
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.Trashed = source.Trashed;
            target.Udi = Udi.Create(Constants.UdiEntityType.DataType, source.Key);

            if (!_propertyEditors.TryGet(source.EditorAlias, out var editor))
                return;

            target.Alias = editor.Alias;
            target.Group = editor.Group;
            target.Icon = editor.Icon;
        }

        // Umbraco.Code.MapAll -HasPrevalues
        private void Map(IDataType source, DataTypeDisplay target, MapperContext context)
        {
            target.AvailableEditors = MapAvailableEditors(source, context);
            target.Id = source.Id;
            target.IsSystemDataType = SystemIds.Contains(source.Id);
            target.Key = source.Key;
            target.Name = source.Name;
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.PreValues = MapPreValues(source, context);
            target.SelectedEditor = source.EditorAlias.IsNullOrWhiteSpace() ? null : source.EditorAlias;
            target.Trashed = source.Trashed;
            target.Udi = Udi.Create(Constants.UdiEntityType.DataType, source.Key);

            if (!_propertyEditors.TryGet(source.EditorAlias, out var editor))
                return;

            target.Alias = editor.Alias;
            target.Group = editor.Group;
            target.Icon = editor.Icon;
        }

        // Umbraco.Code.MapAll -CreateDate -DeleteDate -UpdateDate
        // Umbraco.Code.MapAll -Key -Path -CreatorId -Level -SortOrder -Configuration
        private void Map(DataTypeSave source, IDataType target, MapperContext context)
        {
            target.DatabaseType = MapDatabaseType(source);
            target.Editor = _propertyEditors[source.EditorAlias];
            target.Id = Convert.ToInt32(source.Id);
            target.Name = source.Name;
            target.ParentId = source.ParentId;
        }

        private IEnumerable<PropertyEditorBasic> MapAvailableEditors(IDataType source, MapperContext context)
        {
            var contentSection = Current.Configs.Settings().Content;
            var properties = _propertyEditors
                .Where(x => !x.IsDeprecated || contentSection.ShowDeprecatedPropertyEditors || source.EditorAlias == x.Alias)
                .OrderBy(x => x.Name);
            return context.MapEnumerable<IDataEditor, PropertyEditorBasic>(properties);
        }

        private IEnumerable<DataTypeConfigurationFieldDisplay> MapPreValues(IDataType dataType, MapperContext context)
        {
            // in v7 it was apparently fine to have an empty .EditorAlias here, in which case we would map onto
            // an empty fields list, which made no sense since there would be nothing to map to - and besides,
            // a datatype without an editor alias is a serious issue - v8 wants an editor here

            if (string.IsNullOrWhiteSpace(dataType.EditorAlias) || !Current.PropertyEditors.TryGet(dataType.EditorAlias, out var editor))
                throw new InvalidOperationException($"Could not find a property editor with alias \"{dataType.EditorAlias}\".");

            var configurationEditor = editor.GetConfigurationEditor();
            var fields = context.MapEnumerable<ConfigurationField,DataTypeConfigurationFieldDisplay>(configurationEditor.Fields);
            var configurationDictionary = configurationEditor.ToConfigurationEditor(dataType.Configuration);

            MapConfigurationFields(dataType, fields, configurationDictionary);

            return fields;
        }
        
        private void MapConfigurationFields(IDataType dataType, List<DataTypeConfigurationFieldDisplay> fields, IDictionary<string, object> configuration)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            // now we need to wire up the pre-values values with the actual fields defined
            foreach (var field in fields.ToList())
            {
                //filter out the not-supported pre-values for built-in data types
                if (dataType != null && dataType.IsBuildInDataType() && field.Key.InvariantEquals(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes))
                {
                    fields.Remove(field);
                    continue;
                }

                if (configuration.TryGetValue(field.Key, out var value))
                {
                    field.Value = value;
                }
                else
                {
                    // weird - just leave the field without a value - but warn
                    _logger.Warn<DataTypeMapDefinition, string>("Could not find a value for configuration field '{ConfigField}'", field.Key);
                }
            }
        }

        private ValueStorageType MapDatabaseType(DataTypeSave source)
        {
            if (!_propertyEditors.TryGet(source.EditorAlias, out var editor))
                throw new InvalidOperationException($"Could not find property editor \"{source.EditorAlias}\".");

            // TODO: what about source.PropertyEditor? can we get the configuration here? 'cos it may change the storage type?!
            var valueType = editor.GetValueEditor().ValueType;
            return ValueTypes.ToStorageType(valueType);
        }

        private IEnumerable<DataTypeConfigurationFieldDisplay> MapPreValues(IDataEditor source, MapperContext context)
        {
            // this is a new data type, initialize default configuration
            // get the configuration editor,
            // get the configuration fields and map to UI,
            // get the configuration default values and map to UI

            var configurationEditor = source.GetConfigurationEditor();

            var fields = context.MapEnumerable<ConfigurationField, DataTypeConfigurationFieldDisplay>(configurationEditor.Fields);

            var defaultConfiguration = configurationEditor.DefaultConfiguration;
            if (defaultConfiguration != null)
                MapConfigurationFields(null, fields, defaultConfiguration);

            return fields;
        }
    }
}
