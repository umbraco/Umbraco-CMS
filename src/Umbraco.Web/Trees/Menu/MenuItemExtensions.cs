namespace Umbraco.Web.Trees.Menu
{
    public static class MenuItemExtensions
    {
        /// <summary>
        /// Used as a key for the AdditionalData to specify a specific dialog title instead of the menu title
        /// </summary>
        internal const string DialogTitleKey = "dialogTitle";
        internal const string ActionUrlKey = "actionUrl";
        internal const string ActionUrlMethodKey = "actionUrlMethod";
        internal const string ActionViewKey = "actionView";

        /// <summary>
        /// Sets the menu item to display a dialog based on a view path
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
        /// Sets the menu item to display a dialog based on a url path
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
    }
}