using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Templates.PartialViews;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

// Unfortunately this has to be public to be injected into a controller
public sealed class BlockGridSampleHelper
{
    private const string ContainerName = "Umbraco Block Grid Demo";

    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IPartialViewPopulator _partialViewPopulator;

    public BlockGridSampleHelper(
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IPartialViewPopulator partialViewPopulator)
    {
        _contentTypeService = contentTypeService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _partialViewPopulator = partialViewPopulator;
        _dataTypeService = dataTypeService;
    }

    /// <summary>
    /// Creates block grid elements for sample purposes:
    /// - a "Headline" block with a text box
    /// - an "Image" block with a single value media picker
    /// - a "Rich Text" block with an RTE
    /// - an empty "Two Column Layout" (will be used for layouting nested blocks)
    /// </summary>
    /// <param name="createElement">The function that will perform the actual element creation</param>
    /// <param name="errorMessage">If an error occurs, this message will describe that error</param>
    /// <returns>A mapping table between element aliases and the created element UDIs, or null if an error occurs</returns>
    internal Dictionary<string, Udi>? CreateSampleElements(Func<DocumentTypeSave, ActionResult<IContentType?>> createElement, out string errorMessage)
    {
        errorMessage = string.Empty;

        EntityContainer? container = GetOrCreateContainer();
        if (container == null)
        {
            errorMessage = $"Unable to get or create content type container: {ContainerName}";
            return null;
        }

        FindDataTypes(out IDataType? textBox, out IDataType? tinyMce, out IDataType? mediaPicker);
        if (textBox == null || tinyMce == null || mediaPicker == null)
        {
            errorMessage = $"Could not find required data types - must have a text box, a rich text editor and a media picker (configured in single picker mode)";
            return null;
        }

        // get any already created elements
        IContentType[] existingContentTypes = _contentTypeService.GetChildren(container.Id).ToArray();

        // this is the return value (will be populated below)
        var elementUdisByAlias = new Dictionary<string, Udi>();

        // these describe the block grid elements we want to create
        var elementDescriptors = new[]
        {
            new
            {
                Alias = "umbBlockGridDemoHeadlineBlock",
                Icon = "icon-font color-black",
                Name = "Headline",
                Property = new { Alias = "headline", Label = "Headline", EditorId = textBox.Id }
            },
            new
            {
                Alias = "umbBlockGridDemoImageBlock",
                Icon = "icon-umb-media color-black",
                Name = "Image",
                Property = new { Alias = "image", Label = "Image", EditorId = mediaPicker.Id }
            },
            new
            {
                Alias = "umbBlockGridDemoRichTextBlock",
                Icon = "icon-script color-black",
                Name = "Rich Text",
                Property = new { Alias = "richText", Label = "Text", EditorId = tinyMce.Id }
            },
            new
            {
                Alias = "umbBlockGridDemoTwoColumnLayoutBlock",
                Icon = "icon-book-alt color-black",
                Name = "Two Column Layout",
                Property = new { Alias = string.Empty, Label = string.Empty, EditorId = -1 }
            }
        };

        foreach (var elementDescriptor  in elementDescriptors)
        {
            IContentType? contentType = existingContentTypes.FirstOrDefault(c => c.Alias == elementDescriptor.Alias);
            if (contentType != null)
            {
                elementUdisByAlias[elementDescriptor.Alias] = contentType.GetUdi();
                continue;
            }

            var documentTypeSave = new DocumentTypeSave
            {
                Alias = elementDescriptor.Alias,
                Icon = elementDescriptor.Icon,
                Name = elementDescriptor.Name,
                IsElement = true,
                Groups = new[]
                {
                    new PropertyGroupBasic<PropertyTypeBasic>
                    {
                        Alias = "content",
                        Name = "Content",
                        Type = PropertyGroupType.Group,
                        Properties = elementDescriptor.Property.Alias.IsNullOrWhiteSpace()
                            ? Array.Empty<PropertyTypeBasic>()
                            : new[]
                            {
                                new PropertyTypeBasic
                                {
                                    Alias = elementDescriptor.Property.Alias,
                                    Label = elementDescriptor.Property.Label,
                                    Validation = new PropertyTypeValidation { Mandatory = true },
                                    DataTypeId = elementDescriptor.Property.EditorId
                                }
                            }
                    }
                },
                ParentId = container.Id,
                Thumbnail = "folder.png"
            };

            ActionResult<IContentType?> result = createElement(documentTypeSave);

            if (result.Value != null)
            {
                elementUdisByAlias[elementDescriptor.Alias] = result.Value.GetUdi();
                continue;
            }

            if (result.Result is ValidationErrorResult validationErrorResult)
            {
                errorMessage = validationErrorResult.Value switch
                {
                    string error => error,
                    Exception exception => exception.Message,
                    _ => string.Empty
                };
            }

            if(errorMessage.IsNullOrWhiteSpace())
            {
                errorMessage = $"Could not create element type: {elementDescriptor.Name}";
            }

            return null;
        }

        return elementUdisByAlias;
    }

    internal void CreateSamplePartialViews()
    {
        var embeddedBasePath = $"{_partialViewPopulator.CoreEmbeddedPath}.BlockGrid.Components";
        var fileSystemBasePath = "/Views/partials/blockgrid/Components";
        var filesToMove = new[]
        {
            "umbBlockGridDemoHeadlineBlock.cshtml",
            "umbBlockGridDemoImageBlock.cshtml",
            "umbBlockGridDemoRichTextBlock.cshtml",
            "umbBlockGridDemoTwoColumnLayoutBlock.cshtml",
        };

        foreach (var fileName in filesToMove)
        {
            var embeddedPath = $"{embeddedBasePath}.{fileName}";
            var fileSystemPath = $"{fileSystemBasePath}/{fileName}";
            _partialViewPopulator.CopyPartialViewIfNotExists(_partialViewPopulator.GetCoreAssembly(), embeddedPath, fileSystemPath);
        }
    }

    private EntityContainer? GetOrCreateContainer()
    {
        var userId = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1;

        EntityContainer? container = _contentTypeService.GetContainers(ContainerName, 1).FirstOrDefault();
        if (container == null)
        {
            Attempt<OperationResult<OperationResultType, EntityContainer>?> attempt =
                _contentTypeService.CreateContainer(Constants.System.Root, Guid.NewGuid(), ContainerName, userId);
            container = attempt.Result?.Entity;
        }

        return container;
    }

    private void FindDataTypes(out IDataType? textBox, out IDataType? tinyMce, out IDataType? mediaPicker)
    {
        // order by ID to prioritize default installed data types
        IDataType[] dataTypes = _dataTypeService.GetAll().OrderBy(d => d.Id).ToArray();

        textBox = dataTypes.FirstOrDefault(d => d.EditorAlias == Constants.PropertyEditors.Aliases.TextBox);
        tinyMce = dataTypes.FirstOrDefault(d => d.EditorAlias == Constants.PropertyEditors.Aliases.TinyMce);
        mediaPicker = dataTypes.Where(d =>
                d.EditorAlias == Constants.PropertyEditors.Aliases.MediaPicker3
                && d.ConfigurationAs<MediaPicker3Configuration>()?.Multiple == false)
            // prioritize the default "Image Media Picker" if it exists
            .MinBy(d => d.Name == "Image Media Picker" ? 0 : 1);
    }
}
