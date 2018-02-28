﻿using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates a ContentPropertyDisplay from a Property
    /// </summary>
    internal class ContentPropertyDisplayConverter : ContentPropertyBasicConverter<ContentPropertyDisplay>
    {
        private readonly ILocalizedTextService _textService;

        public ContentPropertyDisplayConverter(IDataTypeService dataTypeService, ILocalizedTextService textService)
            : base(dataTypeService)
        {
            _textService = textService;
        }

        protected override ContentPropertyDisplay ConvertCore(Property originalProp)
        {
            var display = base.ConvertCore(originalProp);

            var dataTypeService = DataTypeService;
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(originalProp.PropertyType.DataTypeDefinitionId);

            //configure the editor for display with the pre-values
            var valEditor = display.PropertyEditor.ValueEditor;
            valEditor.ConfigureForDisplay(preVals);

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
                display.Config = display.PropertyEditor.PreValueEditor.ConvertDbToEditor(display.PropertyEditor.DefaultPreValues, preVals);
                display.View = valEditor.View;
            }

            //Translate
            display.Label = _textService.UmbracoDictionaryTranslate(display.Label);
            display.Description = _textService.UmbracoDictionaryTranslate(display.Description);

            return display;
        }
    }
}
