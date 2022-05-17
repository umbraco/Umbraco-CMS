using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     CUstom value editor so we can serialize with the correct date format (excluding time)
///     and includes the date validator
/// </summary>
internal class DateValueEditor : DataValueEditor
{
    public DateValueEditor(
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        DataEditorAttribute attribute)
        : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute) =>
        Validators.Add(new DateTimeValidator());

    public override object ToEditor(IProperty property, string? culture = null, string? segment = null)
    {
        Attempt<DateTime?> date = property.GetValue(culture, segment).TryConvertTo<DateTime?>();
        if (date.Success == false || date.Result == null)
        {
            return string.Empty;
        }

        // Dates will be formatted as yyyy-MM-dd
        return date.Result.Value.ToString("yyyy-MM-dd");
    }
}
