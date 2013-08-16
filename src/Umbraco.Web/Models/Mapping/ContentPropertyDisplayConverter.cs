using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates a ContentPropertyDto from a Property
    /// </summary>
    internal class ContentPropertyDisplayConverter : ContentPropertyBasicConverter<ContentPropertyDisplay>
    {
        private readonly ApplicationContext _applicationContext;

        public ContentPropertyDisplayConverter(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        protected override ContentPropertyDisplay ConvertCore(Property originalProp)
        {
            var display = base.ConvertCore(originalProp);

            //set the display properties after mapping
            display.Alias = originalProp.Alias;
            display.Description = originalProp.PropertyType.Description;
            display.Label = originalProp.PropertyType.Name;
            var dataTypeService = (DataTypeService) _applicationContext.Services.DataTypeService;

            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(originalProp.PropertyType.DataTypeDefinitionId);
           
            
            
            if (display.PropertyEditor == null)
            {
                display.Config = PreValueCollection.AsDictionary(preVals);
                //if there is no property editor it means that it is a legacy data type
                // we cannot support editing with that so we'll just render the readonly value view.
                display.View = GlobalSettings.Path.EnsureEndsWith('/') +
                               "views/propertyeditors/readonlyvalue/readonlyvalue.html";
            }
            else
            {
                //let the property editor format the pre-values
                display.Config = display.PropertyEditor.FormatPreValues(display.PropertyEditor.DefaultPreValues, preVals);
                display.View = display.PropertyEditor.ValueEditor.View;
            }

            return display;
        }
    }
}