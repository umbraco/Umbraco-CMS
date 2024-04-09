using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.RelationType;

public class RelationTypeResponseModel : RelationTypeBaseModel
{
    public Guid Id { get; set; }

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
