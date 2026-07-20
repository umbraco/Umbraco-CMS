namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
///     Represents the model for creating a new member type.
/// </summary>
public class MemberTypeCreateModel : MemberTypeModelBase
{
    /// <summary>
    ///     Gets or sets the unique key for the member type being created.
    /// </summary>
    public Guid? Key { get; set; }

    /// <summary>
    ///     Gets or sets the key of the container (folder) to place the member type in.
    /// </summary>
    public Guid? ContainerKey { get; set; }
}
