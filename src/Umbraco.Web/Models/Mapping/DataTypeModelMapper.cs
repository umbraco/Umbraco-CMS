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

            config.CreateMap<PropertyEditor, PropertyEditorBasic>()
                  .ForMember(basic => basic.EditorId, expression => expression.MapFrom(editor => editor.Id));

            //just maps the standard properties, does not map the value!
            config.CreateMap<PreValueField, PreValueFieldDisplay>();

            config.CreateMap<IDataTypeDefinition, DataTypeDisplay>()
                  .ForMember(display => display.AvailableEditors, expression => expression.ResolveUsing<AvailablePropertyEditorsResolver>())
                  .ForMember(display => display.PreValues, expression => expression.ResolveUsing(
                      new PreValueDisplayResolver(lazyDataTypeService)))
                  .ForMember(display => display.SelectedEditor, expression => expression.MapFrom(
                      definition => definition.ControlId == Guid.Empty ? null : (Guid?) definition.ControlId));

            //gets a list of PreValueFieldDisplay objects from the data type definition
            config.CreateMap<IDataTypeDefinition, IEnumerable<PreValueFieldDisplay>>()
                  .ConvertUsing(definition =>
                      {
                          var resolver = new PreValueDisplayResolver(lazyDataTypeService);
                          return resolver.Convert(definition);
                      });
                

            config.CreateMap<DataTypeSave, IDataTypeDefinition>()
                  .ConstructUsing(save => new DataTypeDefinition(-1, save.SelectedEditor) {CreateDate = DateTime.Now})
                  .ForMember(definition => definition.ControlId, expression => expression.MapFrom(save => save.SelectedEditor))
                  .ForMember(definition => definition.ParentId, expression => expression.MapFrom(save => -1))
                  .ForMember(definition => definition.DatabaseType, expression => expression.ResolveUsing<DatabaseTypeResolver>());
        }
    }
}