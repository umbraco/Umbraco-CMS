using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Trees;

/// <summary>
    /// A context menu item
/// </summary>
[DataContract(Name = "menuItem", Namespace = "")]
public class MenuItem
{
    #region Constructors

    public MenuItem()
    {
        AdditionalData = new Dictionary<string, object>();
        Icon = Constants.Icons.Folder;
    }

    public MenuItem(string alias, string name)
        : this()
    {
        Alias = alias;
        Name = name;
    }

    public MenuItem(string alias, ILocalizedTextService textService)
        : this()
    {
        Alias = alias;
        Name = textService.Localize("actions", Alias);
            TextDescription =  textService.Localize("visuallyHiddenTexts", alias + "_description", Thread.CurrentThread.CurrentUICulture);
    }

    /// <summary>
        /// Create a menu item based on an <see cref="IAction"/> definition
    /// </summary>
    /// <param name="action"></param>
    /// <param name="name"></param>
    public MenuItem(IAction action, string name = "")
        : this()
    {
        Name = name.IsNullOrWhiteSpace() ? action.Alias : name;
        Alias = action.Alias;
        SeparatorBefore = false;
        Icon = action.Icon;
        Action = action;
    }

    #endregion

    #region Properties

    [IgnoreDataMember]
    public IAction? Action { get; set; }

    /// <summary>
    ///     A dictionary to support any additional meta data that should be rendered for the node which is
    ///     useful for custom action commands such as 'create', 'copy', etc...
    /// </summary>
    /// <remarks>
    ///     We will also use the meta data collection for dealing with legacy menu items (i.e. for loading custom URLs or
    ///     executing custom JS).
    /// </remarks>
    [DataMember(Name = "metaData")]
    public Dictionary<string, object> AdditionalData { get; private set; }

    [DataMember(Name = "name", IsRequired = true)]
    [Required]
    public string? Name { get; set; }

    [DataMember(Name = "alias", IsRequired = true)]
    [Required]
    public string? Alias { get; set; }

    [DataMember(Name = "textDescription")]
    public string? TextDescription { get; set; }

    /// <summary>
        /// Ensures a menu separator will exist before this menu item
    /// </summary>
    [DataMember(Name = "separator")]
    public bool SeparatorBefore { get; set; }

    /// <summary>
    /// Icon to use at action menu item.
    /// </summary>
    [DataMember(Name = "icon")]
    public string Icon { get; set; }

    /// <summary>
    /// Used in the UI to indicate whether icons should be prefixed with "icon-".
    /// If not legacy icon full icon name should be specified.
    /// </summary>
    [DataMember(Name = "useLegacyIcon")]
    public bool UseLegacyIcon { get; set; } = true;

        /// <summary>
        /// Used in the UI to inform the user that the menu item will open a dialog/confirmation
    /// </summary>
    [DataMember(Name = "opensDialog")]
    public bool OpensDialog { get; set; }

    #endregion

    #region Constants

    /// <summary>
    ///     Used as a key for the AdditionalData to specify a specific dialog title instead of the menu title
    /// </summary>
    internal const string DialogTitleKey = "dialogTitle";

    /// <summary>
    ///     Used to specify the URL that the dialog will launch to in an iframe
    /// </summary>
    internal const string ActionUrlKey = "actionUrl";

    // TODO: some action's want to launch a new window like live editing, we support this in the menu item's metadata with
    // a key called: "actionUrlMethod" which can be set to either: Dialog, BlankWindow. Normally this is always set to Dialog
    // if a URL is specified in the "actionUrl" metadata. For now I'm not going to implement launching in a blank window,
    // though would be v-easy, just not sure we want to ever support that?
    internal const string ActionUrlMethodKey = "actionUrlMethod";

    /// <summary>
    ///     Used to specify the angular view that the dialog will launch
    /// </summary>
    internal const string ActionViewKey = "actionView";

    /// <summary>
    ///     Used to specify the js method to execute for the menu item
    /// </summary>
    internal const string JsActionKey = "jsAction";

    /// <summary>
    ///     Used to specify an angular route to go to for the menu item
    /// </summary>
    internal const string ActionRouteKey = "actionRoute";

    #endregion

    #region Methods

    /// <summary>
    ///     Sets the menu item to navigate to the specified angular route path
    /// </summary>
    /// <param name="route"></param>
    public void NavigateToRoute(string route) => AdditionalData[ActionRouteKey] = route;

    /// <summary>
    ///     Adds the required meta data to the menu item so that angular knows to attempt to call the Js method.
    /// </summary>
    /// <param name="jsToExecute"></param>
    public void ExecuteJsMethod(string jsToExecute) => SetJsAction(jsToExecute);

    /// <summary>
    ///     Sets the menu item to display a dialog based on an angular view path
    /// </summary>
    /// <param name="view"></param>
    /// <param name="dialogTitle"></param>
    public void LaunchDialogView(string view, string dialogTitle)
    {
        SetDialogTitle(dialogTitle);
        SetActionView(view);
    }

    /// <summary>
    ///     Sets the menu item to display a dialog based on a URL path in an iframe
    /// </summary>
    /// <param name="url"></param>
    /// <param name="dialogTitle"></param>
    public void LaunchDialogUrl(string url, string dialogTitle)
    {
        SetDialogTitle(dialogTitle);
        SetActionUrl(url);
    }

    private void SetJsAction(string jsToExecute) => AdditionalData[JsActionKey] = jsToExecute;

    /// <summary>
    ///     Puts a dialog title into the meta data to be displayed on the dialog of the menu item (if there is one)
    ///     instead of the menu name
    /// </summary>
    /// <param name="dialogTitle"></param>
    private void SetDialogTitle(string dialogTitle) => AdditionalData[DialogTitleKey] = dialogTitle;

    /// <summary>
    ///     Configures the menu item to launch a specific view
    /// </summary>
    /// <param name="view"></param>
    private void SetActionView(string view) => AdditionalData[ActionViewKey] = view;

    /// <summary>
    ///     Configures the menu item to launch a URL with the specified action (dialog or new window)
    /// </summary>
    /// <param name="url"></param>
    /// <param name="method"></param>
    private void SetActionUrl(string url, ActionUrlMethod method = ActionUrlMethod.Dialog)
    {
        AdditionalData[ActionUrlKey] = url;
        AdditionalData[ActionUrlMethodKey] = method;
    }

    #endregion
}
