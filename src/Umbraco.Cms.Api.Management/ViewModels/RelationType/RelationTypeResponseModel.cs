using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.RelationType;

/// <summary>
/// Represents a response model containing information about a relation type in the Umbraco CMS Management API.
/// </summary>
public class RelationTypeResponseModel : RelationTypeBaseModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the relation type.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the alias of the relation type.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    ///     Gets or sets the Parent's object type.
    /// </summary>
    public ObjectTypeResponseModel? ParentObject { get; set; }

    /// <summary>
    ///     Gets or sets the Child's object type.
    /// </summary>
    public ObjectTypeResponseModel? ChildObject { get; set; }
}
