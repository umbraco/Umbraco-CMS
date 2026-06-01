using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Defines a factory for creating presentation models used in editing Umbraco elements.
/// </summary>
public interface IElementEditingPresentationFactory
{
    /// <summary>
    /// Maps the given <see cref="CreateElementRequestModel"/> to an <see cref="ElementCreateModel"/>.
    /// </summary>
    /// <param name="requestModel">The request model containing data to create the element.</param>
    /// <returns>An <see cref="ElementCreateModel"/> representing the element to be created.</returns>
    ElementCreateModel MapCreateModel(CreateElementRequestModel requestModel);

    /// <summary>
    /// Maps the given <see cref="UpdateElementRequestModel"/> to an <see cref="ElementUpdateModel"/>.
    /// </summary>
    /// <param name="requestModel">The update element request model to map from.</param>
    /// <returns>The mapped <see cref="ElementUpdateModel"/> instance.</returns>
    ElementUpdateModel MapUpdateModel(UpdateElementRequestModel requestModel);

    /// <summary>
    /// Maps the given <see cref="ValidateUpdateElementRequestModel"/> to a <see cref="ValidateElementUpdateModel"/> for validation purposes.
    /// </summary>
    /// <param name="requestModel">The request model containing the data to validate.</param>
    /// <returns>A <see cref="ValidateElementUpdateModel"/> representing the validated element update.</returns>
    ValidateElementUpdateModel MapValidateUpdateModel(ValidateUpdateElementRequestModel requestModel);
}
