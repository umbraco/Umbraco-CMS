using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors.ParameterEditors;

internal abstract class MultiplePickerParamateterValueEditorBase : DataValueEditor, IDataValueReference
{
    private readonly IEntityService _entityService;

    public MultiplePickerParamateterValueEditorBase(
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        DataEditorAttribute attribute,
        IEntityService entityService)
        : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute) =>
        _entityService = entityService;

    public abstract string UdiEntityType { get; }

    public abstract UmbracoObjectTypes UmbracoObjectType { get; }

    public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
    {
        var asString = value is string str ? str : value?.ToString();

        if (string.IsNullOrEmpty(asString))
        {
            yield break;
        }

        foreach (var udiStr in asString.Split(','))
        {
            if (UdiParser.TryParse(udiStr, out Udi? udi))
            {
                yield return new UmbracoEntityReference(udi);
            }

            // this is needed to support the legacy case when the multiple media picker parameter editor stores ints not udis
            if (int.TryParse(udiStr, out var id))
            {
                Attempt<Guid> guidAttempt = _entityService.GetKey(id, UmbracoObjectType);
                Guid guid = guidAttempt.Success ? guidAttempt.Result : Guid.Empty;

                if (guid != Guid.Empty)
                {
                    yield return new UmbracoEntityReference(new GuidUdi(Constants.UdiEntityType.Media, guid));
                }
            }
        }
    }
}
