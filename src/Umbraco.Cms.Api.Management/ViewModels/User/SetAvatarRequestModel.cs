namespace Umbraco.Cms.Api.Management.ViewModels.User;

    /// <summary>
    /// A request model used to set the avatar for a user.
    /// </summary>
public class SetAvatarRequestModel
{
    public required ReferenceByIdModel File { get; set; }
}
