﻿using System;
using System.Collections.Generic;
using AutoMapper;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Configure's model mappings for Data types
    /// </summary>
    internal class DataTypeModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<PropertyEditor, PropertyEditorBasic>();

            //just maps the standard properties, does not map the value!
            config.CreateMap<PreValueField, PreValueFieldDisplay>()
                .ForMember(x => x.Value, expression => expression.Ignore());

            var systemIds = new[]
            {
                Constants.System.DefaultContentListViewDataTypeId, 
                Constants.System.DefaultMediaListViewDataTypeId, 
                Constants.System.DefaultMembersListViewDataTypeId
            };

            config.CreateMap<PropertyEditor, DataTypeBasic>()
                .ForMember(x => x.Udi, expression => expression.Ignore())
                .ForMember(x => x.HasPrevalues, expression => expression.Ignore())
                .ForMember(x => x.IsSystemDataType, expression => expression.Ignore())
                .ForMember(x => x.Id, expression => expression.Ignore())
                .ForMember(x => x.Trashed, expression => expression.Ignore())
                .ForMember(x => x.Key, expression => expression.Ignore())
                .ForMember(x => x.ParentId, expression => expression.Ignore())
                .ForMember(x => x.Path, expression => expression.Ignore())
                .ForMember(x => x.AdditionalData, expression => expression.Ignore());

            config.CreateMap<IDataTypeDefinition, DataTypeBasic>()
                .ForMember(x => x.Udi, expression => expression.MapFrom(content => Udi.Create(Constants.UdiEntityType.DataType, content.Key)))
                .ForMember(x => x.HasPrevalues, expression => expression.Ignore())
                .ForMember(x => x.Icon, expression => expression.Ignore())
                .ForMember(x => x.Alias, expression => expression.Ignore())
                .ForMember(x => x.Group, expression => expression.Ignore())
                .ForMember(x => x.IsSystemDataType, expression => expression.MapFrom(definition => systemIds.Contains(definition.Id)))
                .AfterMap((def, basic) =>
                {
                    var editor = PropertyEditorResolver.Current.GetByAlias(def.PropertyEditorAlias);
                    if (editor != null)
                    {
                        basic.Alias = editor.Alias;
                        basic.Group = editor.Group;
                        basic.Icon = editor.Icon;
                    }
                });

            config.CreateMap<IDataTypeDefinition, DataTypeDisplay>()
                .ForMember(x => x.Udi, expression => expression.MapFrom(content => Udi.Create(Constants.UdiEntityType.DataType, content.Key)))
                .ForMember(display => display.AvailableEditors, expression => expression.ResolveUsing(new AvailablePropertyEditorsResolver(UmbracoConfig.For.UmbracoSettings().Content)))
                .ForMember(display => display.PreValues, expression => expression.ResolveUsing(
                    new PreValueDisplayResolver(applicationContext.Services.DataTypeService)))
                .ForMember(display => display.SelectedEditor, expression => expression.MapFrom(
                    definition => definition.PropertyEditorAlias.IsNullOrWhiteSpace() ? null : definition.PropertyEditorAlias))
                .ForMember(x => x.HasPrevalues, expression => expression.Ignore())
                .ForMember(x => x.Notifications, expression => expression.Ignore())
                .ForMember(x => x.Icon, expression => expression.Ignore())
                .ForMember(x => x.Alias, expression => expression.Ignore())
                .ForMember(x => x.Group, expression => expression.Ignore())                
                .ForMember(x => x.IsSystemDataType, expression => expression.MapFrom(definition => systemIds.Contains(definition.Id)))
                .AfterMap((def, basic) =>
                {
                    var editor = PropertyEditorResolver.Current.GetByAlias(def.PropertyEditorAlias);
                    if (editor != null)
                    {
                        basic.Group = editor.Group;
                        basic.Icon = editor.Icon;
                    }
                });

            //gets a list of PreValueFieldDisplay objects from the data type definition
            config.CreateMap<IDataTypeDefinition, IEnumerable<PreValueFieldDisplay>>()
                  .ConvertUsing(definition =>
                      {
                          var resolver = new PreValueDisplayResolver(applicationContext.Services.DataTypeService);
                          return resolver.Convert(definition);
                      });

            config.CreateMap<DataTypeSave, IDataTypeDefinition>()
                .ConstructUsing(save => new DataTypeDefinition(save.SelectedEditor) {CreateDate = DateTime.Now})
                .IgnoreDeletableEntityCommonProperties()
                .ForMember(definition => definition.Id, expression => expression.MapFrom(save => Convert.ToInt32(save.Id)))
                //we have to ignore the Key otherwise this will reset the UniqueId field which should never change!
                // http://issues.umbraco.org/issue/U4-3911                
                .ForMember(definition => definition.Key, expression => expression.Ignore())
                .ForMember(definition => definition.Path, expression => expression.Ignore())
                .ForMember(definition => definition.PropertyEditorAlias, expression => expression.MapFrom(save => save.SelectedEditor))
                .ForMember(definition => definition.DatabaseType, expression => expression.ResolveUsing(new DatabaseTypeResolver()))
                .ForMember(x => x.ControlId, expression => expression.Ignore())
                .ForMember(x => x.CreatorId, expression => expression.Ignore())
                .ForMember(x => x.Level, expression => expression.Ignore())
                .ForMember(x => x.SortOrder, expression => expression.Ignore());

            //Converts a property editor to a new list of pre-value fields - used when creating a new data type or changing a data type with new pre-vals
            config.CreateMap<PropertyEditor, IEnumerable<PreValueFieldDisplay>>()
                .ConvertUsing(editor =>
                    {
                        //this is a new data type, so just return the field editors, there are no values yet
                        var defaultVals = editor.DefaultPreValues;
                        var fields = editor.PreValueEditor.Fields.Select(Mapper.Map<PreValueFieldDisplay>).ToArray();
                        if (defaultVals != null)
                        {                           
                            PreValueDisplayResolver.MapPreValueValuesToPreValueFields(fields, defaultVals);
                        }
                        return fields;
                    });
        }
    }
}
