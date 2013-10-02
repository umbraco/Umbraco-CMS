using Umbraco.Core;
using Umbraco.Core.Models.EntityBase;
using umbraco;

namespace Umbraco.Web.Trees.Menu
{
    public static class MenuItemExtensions
    {
        /// <summary>
        /// Used as a key for the AdditionalData to specify a specific dialog title instead of the menu title
        /// </summary>
        internal const string DialogTitleKey = "dialogTitle";
        
        /// <summary>
        /// Used to specify the URL that the dialog will launch to in an iframe
        /// </summary>
        internal const string ActionUrlKey = "actionUrl";


        internal const string ActionUrlMethodKey = "actionUrlMethod";

        /// <summary>
        /// Used to specify the angular view that the dialog will launch
        /// </summary>
        internal const string ActionViewKey = "actionView";

        /// <summary>
        /// Sets the menu item to display a dialog based on an angular view path
        /// </summary>
        /// <param name="menuItem"></param>
        /// <param name="view"></param>
        /// <param name="dialogTitle"></param>
        public static void LaunchDialogView(this MenuItem menuItem, string view, string dialogTitle)
        {
            menuItem.SetDialogTitle(dialogTitle);
            menuItem.AdditionalData[ActionViewKey] = view;
        }

        /// <summary>
        /// Sets the menu item to display a dialog based on a url path in an iframe
        /// </summary>
        /// <param name="menuItem"></param>
        /// <param name="url"></param>
        /// <param name="dialogTitle"></param>
        public static void LaunchDialogUrl(this MenuItem menuItem, string url, string dialogTitle)
        {
            menuItem.SetDialogTitle(dialogTitle);
            menuItem.SetActionUrl(url);
        }

        /// <summary>
        /// Puts a dialog title into the meta data to be displayed on the dialog of the menu item (if there is one)
        /// instead of the menu name
        /// </summary>
        /// <param name="menuItem"></param>
        /// <param name="dialogTitle"></param>
        private static void SetDialogTitle(this MenuItem menuItem, string dialogTitle)
        {
            menuItem.AdditionalData[DialogTitleKey] = dialogTitle;
        }

        /// <summary>
        /// Configures the menu item to launch a URL with the specified action (dialog or new window)
        /// </summary>
        /// <param name="menuItem"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        private static void SetActionUrl(this MenuItem menuItem, string url, ActionUrlMethod method = ActionUrlMethod.Dialog)
        {
            menuItem.AdditionalData[ActionUrlKey] = url;
            menuItem.AdditionalData[ActionUrlMethodKey] = url;
        }

        internal static void ConvertLegacyMenuItem(this MenuItem menuItem, IUmbracoEntity item, string nodeType, string currentSection)
        {
            //First try to get a URL/title from the legacy action,
            // if that doesn't work, try to get the legacy confirm view

            //in some edge cases, item can be null so we'll just convert those to "-1" and "" for id and name since these edge cases don't need that.
            Attempt
                .Try(LegacyTreeDataConverter.GetUrlAndTitleFromLegacyAction(menuItem.Action,
                                                                            item == null ? "-1" : item.Id.ToInvariantString(),
                                                                            nodeType,
                                                                            item == null ? "" : item.Name, currentSection),
                     action => menuItem.LaunchDialogUrl(action.Url, action.DialogTitle))
                .OnFailure(() => LegacyTreeDataConverter.GetLegacyConfirmView(menuItem.Action, currentSection),
                           view => menuItem.LaunchDialogView(
                               view,
                               ui.GetText("defaultdialogs", "confirmdelete") + " '" + item.Name + "' ?"));
        }
    }
}