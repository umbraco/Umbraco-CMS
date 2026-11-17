namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     Represents the action that allows access to sensitive member data.
/// </summary>
/// <remarks>
///     This action is used for permission control when viewing and editing sensitive data on members.
///     Members of the built-in sensitive data user group have the same access.
/// </remarks>
public class ActionMemberSensitiveData : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter" />
    public const string ActionLetter = "Umb.Member.SensitiveData";

    /// <inheritdoc cref="IAction.ActionAlias" />
    public const string ActionAlias = "membersensitivedata";

    /// <inheritdoc/>
    public string Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public bool ShowInNotifier => false;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
