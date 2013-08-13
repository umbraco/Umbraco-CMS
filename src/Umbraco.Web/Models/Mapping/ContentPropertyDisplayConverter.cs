using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
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
            display.Config = _applicationContext.Services.DataTypeService.GetPreValuesByDataTypeId(originalProp.PropertyType.DataTypeDefinitionId);
            if (display.PropertyEditor == null)
            {
                //if there is no property editor it means that it is a legacy data type
                // we cannot support editing with that so we'll just render the readonly value view.
                display.View = GlobalSettings.Path.EnsureEndsWith('/') +
                               "views/propertyeditors/umbraco/readonlyvalue/readonlyvalue.html";
            }
            else
            {
                display.View = display.PropertyEditor.ValueEditor.View;
            }

            return display;
        }
    }
}