namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     Represents the action that allows writing document property values.
/// </summary>
/// <remarks>
///     This action is used for permission control when modifying property values on documents.
/// </remarks>
public class ActionDocumentPropertyWrite : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter" />
    public const string ActionLetter = "Umb.Document.PropertyValue.Write";

    /// <inheritdoc cref="IAction.ActionAlias" />
    public const string ActionAlias = "documentpropertywrite";

    /// <inheritdoc/>
    public string Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public bool ShowInNotifier => false;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}

