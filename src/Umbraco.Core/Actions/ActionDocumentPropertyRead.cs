namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     Represents the action that allows reading document property values.
/// </summary>
/// <remarks>
///     This action is used for permission control when accessing property values on documents.
/// </remarks>
public class ActionDocumentPropertyRead : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter" />
    public const string ActionLetter = "Umb.Document.PropertyValue.Read";

    /// <inheritdoc cref="IAction.ActionAlias" />
    public const string ActionAlias = "documentpropertyread";

    /// <inheritdoc/>
    public string Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public bool ShowInNotifier => false;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}

