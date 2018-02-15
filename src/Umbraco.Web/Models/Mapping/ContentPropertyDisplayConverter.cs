using System;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates a ContentPropertyDisplay from a Property
    /// </summary>
    internal class ContentPropertyDisplayConverter : ContentPropertyBasicConverter<ContentPropertyDisplay>
    {
        public ContentPropertyDisplayConverter(Lazy<IDataTypeService> dataTypeService)
            : base(dataTypeService)
        { }

        public override ContentPropertyDisplay Convert(Property originalProp, ContentPropertyDisplay dest, ResolutionContext context)
        {
            var display = base.Convert(originalProp, dest, context);

            var dataTypeService = DataTypeService.Value;
            var config = dataTypeService.GetDataType(originalProp.PropertyType.DataTypeId).Configuration;

            //configure the editor for display with the pre-values
            var valEditor = display.PropertyEditor.ValueEditor;
            // fixme - the value editor REQUIRES the configuration to operate
            //  at the moment, only for richtext and nested, where it's used to set HideLabel
            //  but, this is the ONLY place where it's assigned? it is also the only place where
            //  .HideLabel is used - and basically all the rest kinda never depends on config,
            //  but... it should?
            var ve = (DataValueEditor) valEditor;
            ve.Configuration = config;

            //set the display properties after mapping
            display.Alias = originalProp.Alias;
            display.Description = originalProp.PropertyType.Description;
            display.Label = originalProp.PropertyType.Name;
            display.HideLabel = valEditor.HideLabel;

            //add the validation information
            display.Validation.Mandatory = originalProp.PropertyType.Mandatory;
            display.Validation.Pattern = originalProp.PropertyType.ValidationRegExp;

            if (display.PropertyEditor == null)
            {
                //display.Config = PreValueCollection.AsDictionary(preVals);
                //if there is no property editor it means that it is a legacy data type
                // we cannot support editing with that so we'll just render the readonly value view.
                display.View = "views/propertyeditors/readonlyvalue/readonlyvalue.html";
            }
            else
            {
                //let the property editor format the pre-values
                display.Config = display.PropertyEditor.ConfigurationEditor.ToValueEditor(config);
                display.View = valEditor.View;
            }

            return display;
        }
    }
}
