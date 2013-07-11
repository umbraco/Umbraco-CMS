namespace Umbraco.Web.Trees
{
    public static class MenuItemExtensions
    {
        /// <summary>
        /// Used as a key for the AdditionalData to specify a specific dialog title instead of the menu title
        /// </summary>
        internal const string DialogTitleKey = "dialogTitle";
        internal const string ActionUrlKey = "actionUrl";
        internal const string ActionUrlMethodKey = "actionUrlMethod";

        /// <summary>
        /// Puts a dialog title into the meta data to be displayed on the dialog of the menu item (if there is one)
        /// instead of the menu name
        /// </summary>
        /// <param name="menuItem"></param>
        /// <param name="dialogTitle"></param>
        public static void SetDialogTitle(this MenuItem menuItem, string dialogTitle)
        {
            menuItem.AdditionalData[DialogTitleKey] = dialogTitle;
        }

        /// <summary>
        /// Configures the menu item to launch a URL with the specified action (dialog or new window)
        /// </summary>
        /// <param name="menuItem"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        public static void SetActionUrl(this MenuItem menuItem, string url, ActionUrlMethod method = ActionUrlMethod.Dialog)
        {
            menuItem.AdditionalData[ActionUrlKey] = url;
            menuItem.AdditionalData[ActionUrlMethodKey] = url;
        }
    }
}