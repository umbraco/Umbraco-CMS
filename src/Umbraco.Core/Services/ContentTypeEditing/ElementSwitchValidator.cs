using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

/// <summary>
///     Implementation of <see cref="IElementSwitchValidator"/> for validating element type switching operations.
/// </summary>
/// <remarks>
///     This validator checks constraints when switching content types between document and element modes,
///     ensuring data integrity and preventing invalid configurations.
/// </remarks>
public class ElementSwitchValidator : IElementSwitchValidator
{
    private readonly IContentTypeService _contentTypeService;
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IDataTypeService _dataTypeService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementSwitchValidator"/> class.
    /// </summary>
    /// <param name="contentTypeService">The content type service for querying content type hierarchies.</param>
    /// <param name="propertyEditorCollection">The collection of property editors to check for block structure support.</param>
    /// <param name="dataTypeService">The data type service for querying data type configurations.</param>
    public ElementSwitchValidator(
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService)
    {
        _contentTypeService = contentTypeService;
        _propertyEditorCollection = propertyEditorCollection;
        _dataTypeService = dataTypeService;
    }

    /// <inheritdoc />
    public Task<bool> AncestorsAreAlignedAsync(IContentType contentType)
    {
        // this call does not return the system roots
        var ancestorIds = contentType.AncestorIds();
        if (ancestorIds.Length == 0)
        {
            // if there are no ancestors, validation passes
            return Task.FromResult(true);
        }

        // if there are any ancestors where IsElement is different from the contentType, the validation fails
        return Task.FromResult(_contentTypeService.GetMany(ancestorIds)
            .Any(ancestor => ancestor.IsElement != contentType.IsElement) is false);
    }

    /// <inheritdoc />
    public Task<bool> DescendantsAreAlignedAsync(IContentType contentType)
    {
        IEnumerable<IContentType> descendants = _contentTypeService.GetDescendants(contentType.Id, false);

        // if there are any descendants where IsElement is different from the contentType, the validation fails
        return Task.FromResult(descendants.Any(descendant => descendant.IsElement != contentType.IsElement) is false);
    }

    /// <inheritdoc />
    public async Task<bool> ElementToDocumentNotUsedInBlockStructuresAsync(IContentTypeBase contentType)
    {
        // get all propertyEditors that support block usage
        IDataEditor[] editors = _propertyEditorCollection.Where(pe => pe.SupportsConfigurableElements).ToArray();
        var blockEditorAliases = editors.Select(pe => pe.Alias).ToArray();

        // get all dataTypes that are based on those propertyEditors
        IEnumerable<IDataType> dataTypes = await _dataTypeService.GetByEditorAliasAsync(blockEditorAliases);

        // if any dataType has a configuration where this element is selected as a possible block, the validation fails.
        return dataTypes.Any(dataType =>
            editors.First(editor => editor.Alias == dataType.EditorAlias)
                .GetValueEditor(dataType.ConfigurationObject)
                .ConfiguredElementTypeKeys().Contains(contentType.Key)) is false;
    }

    /// <inheritdoc />
    public Task<bool> DocumentToElementHasNoContentAsync(IContentTypeBase contentType) =>

        // if any content for the content type exists, the validation fails.
        Task.FromResult(_contentTypeService.HasContentNodes(contentType.Id) is false);
}
