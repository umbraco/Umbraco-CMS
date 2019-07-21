using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class NestedContentPropertyEditorsMigration : PropertyEditorsMigrationBase
    {
        private Dictionary<string, int> _elementTypeIds;
        private ConfigurationEditor _configEditor;

        public NestedContentPropertyEditorsMigration(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            _elementTypeIds = Database.Fetch<ContentTypeDto>(Sql()
                .Select<ContentTypeDto>(x => x.NodeId, x => x.Alias)
                .From<ContentTypeDto>()
                .InnerJoin<NodeDto>().On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(node => node.NodeObjectType == Constants.ObjectTypes.DocumentType))
                .ToDictionary(ct => ct.Alias, ct => ct.NodeId);

            _configEditor = new DropDownFlexibleConfigurationEditor();

            var dataTypes = GetDataTypes(Constants.PropertyEditors.Aliases.NestedContent);            

            var refreshCache = false;
            //ConfigurationEditor configurationEditor = null;

            foreach (var dataType in dataTypes)
            {
                // get property data dtos
                var propertyDataDtos = Database.Fetch<PropertyDataDto>(Sql()
                    .Select<PropertyDataDto>()
                    .From<PropertyDataDto>()
                    .InnerJoin<PropertyTypeDto>().On<PropertyTypeDto, PropertyDataDto>((pt, pd) => pt.Id == pd.PropertyTypeId)
                    .InnerJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>((dt, pt) => dt.NodeId == pt.DataTypeId)
                    .Where<PropertyTypeDto>(x => x.DataTypeId == dataType.NodeId));

                // update dtos
                var updatedDtos = propertyDataDtos.Where(x => UpdateNestedPropertyDataDto(x)).ToArray();

                // persist changes
                foreach (var propertyDataDto in updatedDtos)
                {
                    Database.Update(propertyDataDto);
                    refreshCache = true;
                }
            }

            // if some data types have been updated directly in the database (editing DataTypeDto and/or PropertyDataDto),
            // bypassing the services, then we need to rebuild the cache entirely, including the umbracoContentNu table
            if (refreshCache)
                Context.AddPostMigration<RebuildPublishedSnapshot>();
        }

        private bool UpdateNestedPropertyDataDto(PropertyDataDto pd)
        {
            if (String.IsNullOrWhiteSpace(pd.TextValue))
                return false;

            bool changed = false;

            var objects = JsonConvert.DeserializeObject<List<JObject>>(pd.TextValue);
            foreach(var sourceObject in objects)
            {
                var elementTypeAlias = sourceObject["ncContentTypeAlias"]?.ToObject<string>();
                if (string.IsNullOrEmpty(elementTypeAlias))
                    continue;

                var propertyValues = sourceObject.ToObject<Dictionary<string, string>>();

                var elementTypeId = _elementTypeIds[elementTypeAlias];

                var propertyTypes = Database.Fetch<PropertyTypeDto>(Sql()
                        .Select<PropertyTypeDto>(r => r.Select(x => x.DataTypeDto))
                        .From<PropertyTypeDto>()
                        .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>((pt, dt) => pt.DataTypeId == dt.NodeId)
                        .Where<PropertyTypeDto>(pt => pt.ContentTypeId == elementTypeId)
                        );

                foreach(var pt in propertyTypes.Where(pt => propertyValues.ContainsKey(pt.Alias)))
                {
                    DropDownFlexibleConfiguration config;
                    switch(pt.DataTypeDto.EditorAlias)
                    {
                        case Constants.PropertyEditors.Aliases.RadioButtonList:
                            config = (DropDownFlexibleConfiguration)_configEditor.FromDatabase(pt.DataTypeDto.Configuration);
                            config.Multiple = false;
                            changed = UpdateValueList(sourceObject, propertyValues, pt, config);
                            break;
                        case Constants.PropertyEditors.Aliases.CheckBoxList:
                            config = (DropDownFlexibleConfiguration)_configEditor.FromDatabase(pt.DataTypeDto.Configuration);
                            config.Multiple = true;
                            changed = UpdateValueList(sourceObject, propertyValues, pt, config);
                            break;
                        case Constants.PropertyEditors.Aliases.DropDownListFlexible:
                            config = (DropDownFlexibleConfiguration)_configEditor.FromDatabase(pt.DataTypeDto.Configuration);
                            changed = UpdateValueList(sourceObject, propertyValues, pt, config);
                            break;
                    }
                }
            }

            if (changed)
                pd.TextValue = JsonConvert.SerializeObject(objects);

            return changed;
        }

        private bool UpdateValueList(JObject sourceObject, Dictionary<string, string> propertyValues, PropertyTypeDto pt, DropDownFlexibleConfiguration config)
        {
            var propData = new PropertyDataDto { VarcharValue = propertyValues[pt.Alias] };
            
            if (UpdatePropertyDataDto(propData, config, isMultiple: true))
            {
                sourceObject[pt.Alias] = propData.VarcharValue;
                return true;
            }

            return false;
        }


        // dummy editor for deserialization
        protected class DropDownFlexibleConfigurationEditor : ConfigurationEditor<DropDownFlexibleConfiguration>
        { }
    }
}
