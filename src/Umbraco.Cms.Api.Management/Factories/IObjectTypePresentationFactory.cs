using Umbraco.Cms.Api.Management.ViewModels.RelationType;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentation models for object types.
/// </summary>
public interface IObjectTypePresentationFactory
{
    /// <summary>
    /// Creates a collection of object type response models.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{ObjectTypeResponseModel}"/> containing the object type response models.</returns>
    IEnumerable<ObjectTypeResponseModel> Create();
}
