namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMemberService when the AssignRoles and ReplaceRoles methods are called in the API.
/// </summary>
public class AssignedMemberRolesNotification : MemberRolesNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssignedMemberRolesNotification"/>.
    /// </summary>
    /// <param name="memberIds">
    /// Collection of Ids of the members the roles are being assigned to.
    /// </param>
    /// <param name="roles">
    /// Collection of role names being assigned.
    /// </param>
    public AssignedMemberRolesNotification(int[] memberIds, string[] roles)
        : base(memberIds, roles)
    {
    }
}
