using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// CUstom value editor so we can serialize with the correct date format (excluding time)
    /// and includes the date validator
    /// </summary>
    internal class DateValueEditor : DataValueEditor
    {
        public DateValueEditor(
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            DataEditorAttribute attribute)
            : base(dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer, attribute)
        {
            Validators.Add(new DateTimeValidator());
        }

        public override object ToEditor(IProperty property, string culture= null, string segment = null)
        {
            var date = property.GetValue(culture, segment).TryConvertTo<DateTime?>();
            if (date.Success == false || date.Result == null)
            {
                return String.Empty;
            }
            //Dates will be formatted as yyyy-MM-dd
            return date.Result.Value.ToString("yyyy-MM-dd");
        }
    }
}
