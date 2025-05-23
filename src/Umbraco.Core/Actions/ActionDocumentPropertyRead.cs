namespace Umbraco.Cms.Core.Actions;

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

    /// <inheritdoc />
    public string Icon => string.Empty;

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.OtherCategory;
}

