using System;
using System.Collections.Generic;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
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
            var lazyDataTypeService = new Lazy<IDataTypeService>(() => applicationContext.Services.DataTypeService);

            config.CreateMap<PropertyEditor, PropertyEditorBasic>();

            //just maps the standard properties, does not map the value!
            config.CreateMap<PreValueField, PreValueFieldDisplay>();

            config.CreateMap<IDataTypeDefinition, DataTypeDisplay>()
                  .ForMember(display => display.AvailableEditors, expression => expression.ResolveUsing<AvailablePropertyEditorsResolver>())
                  .ForMember(display => display.PreValues, expression => expression.ResolveUsing(
                      new PreValueDisplayResolver(lazyDataTypeService)))
                  .ForMember(display => display.SelectedEditor, expression => expression.MapFrom(
                      definition => definition.PropertyEditorAlias.IsNullOrWhiteSpace() ? null : definition.PropertyEditorAlias));

            //gets a list of PreValueFieldDisplay objects from the data type definition
            config.CreateMap<IDataTypeDefinition, IEnumerable<PreValueFieldDisplay>>()
                  .ConvertUsing(definition =>
                      {
                          var resolver = new PreValueDisplayResolver(lazyDataTypeService);
                          return resolver.Convert(definition);
                      });

            config.CreateMap<DataTypeSave, IDataTypeDefinition>()
                  .ConstructUsing(save => new DataTypeDefinition(-1, save.SelectedEditor) {CreateDate = DateTime.Now})
                //we have to ignore the Key otherwise this will reset the UniqueId field which should never change!
                // http://issues.umbraco.org/issue/U4-3911
                  .ForMember(definition => definition.Key, expression => expression.Ignore())
                  .ForMember(definition => definition.Path, expression => expression.Ignore())
                  .ForMember(definition => definition.PropertyEditorAlias, expression => expression.MapFrom(save => save.SelectedEditor))
                  .ForMember(definition => definition.ParentId, expression => expression.MapFrom(save => -1))
                  .ForMember(definition => definition.DatabaseType, expression => expression.ResolveUsing<DatabaseTypeResolver>());

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