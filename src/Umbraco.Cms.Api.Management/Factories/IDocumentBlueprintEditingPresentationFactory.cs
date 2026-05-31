using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory for creating editing presentations of document blueprints.
/// </summary>
public interface IDocumentBlueprintEditingPresentationFactory
{
    /// <summary>
    /// Maps the create document blueprint request model to a content blueprint create model.
    /// </summary>
    /// <param name="requestModel">The create document blueprint request model.</param>
    /// <returns>The content blueprint create model.</returns>
    ContentBlueprintCreateModel MapCreateModel(CreateDocumentBlueprintRequestModel requestModel);

    /// <summary>
    /// Maps the update request model to a content blueprint update model.
    /// </summary>
    /// <param name="requestModel">The update request model containing the data to map.</param>
    /// <returns>The mapped content blueprint update model.</returns>
    ContentBlueprintUpdateModel MapUpdateModel(UpdateDocumentBlueprintRequestModel requestModel);
}
