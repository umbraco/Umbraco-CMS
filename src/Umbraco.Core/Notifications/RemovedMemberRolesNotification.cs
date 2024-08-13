namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMemberService when the DissociateRoles method is called in the API.
/// </summary>
public class RemovedMemberRolesNotification : MemberRolesNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemovedMemberRolesNotification"/>.
    /// </summary>
    /// <param name="memberIds">
    /// Collection of Ids of the members the roles are being removed from.
    /// </param>
    /// <param name="roles">
    /// Collection of role names being removed.
    /// </param>
    public RemovedMemberRolesNotification(int[] memberIds, string[] roles)
        : base(memberIds, roles)
    {
    }
}
