using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public class ElementSwitchValidator : IElementSwitchValidator
{
    private readonly IContentTypeService _contentTypeService;
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IDataTypeService _dataTypeService;

    public ElementSwitchValidator(
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService)
    {
        _contentTypeService = contentTypeService;
        _propertyEditorCollection = propertyEditorCollection;
        _dataTypeService = dataTypeService;
    }

    public async Task<bool> AncestorsAreAlignedAsync(IContentType contentType)
    {
        var ancestorIds = contentType.AncestorIds();
        if (ancestorIds.Length == 0)
        {
            return false;
        }

        return await Task.FromResult(_contentTypeService.GetAll(ancestorIds)
            .Any(ancestor => ancestor.IsElement != contentType.IsElement) == false);
    }

    public async Task<bool> DescendantsAreAlignedAsync(IContentType contentType)
    {
        IEnumerable<IContentType> descendants = _contentTypeService.GetDescendants(contentType.Id, false);

        return await Task.FromResult(descendants.Any(descendant => descendant.IsElement != contentType.IsElement) == false);
    }

    public async Task<bool> ElementToDocumentNotUsedInBlockStructuresAsync(IContentTypeBase contentType)
    {
        // get all propertyEditors that support block usage
        IDataEditor[] editors = _propertyEditorCollection.Where(pe => pe.SupportsConfigurableElements).ToArray();
        var blockEditorAliases = editors.Select(pe => pe.Alias).ToArray();

        // get all dataTypes that are based on those propertyEditors
        IEnumerable<IDataType> dataTypes = await _dataTypeService.GetByEditorAliasAsync(blockEditorAliases);

        // check whether any of the configurations on those dataTypes have this element selected as a possible block
        return dataTypes.Any(dataType =>
            editors.First(editor => editor.Alias == dataType.EditorAlias)
                .GetValueEditor(dataType.ConfigurationObject)
                .ConfiguredElementTypeKeys().Contains(contentType.Key) == false);
    }

    public async Task<bool> DocumentToElementHasNoContentAsync(IContentTypeBase contentType) =>
        await Task.FromResult(_contentTypeService.HasContentNodes(contentType.Id) == false);
}
