using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Composing;
using Umbraco.Core.Services;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// Actions and Actionhandlers are a key concept to umbraco and a developer whom wish to apply
    /// businessrules whenever data is changed within umbraco, by implementing the IActionHandler
    /// interface it's possible to invoke methods (foreign to umbraco) - this can be used whenever
    /// there is a specific rule which needs to be applied to content.
    ///
    /// The Action class itself has responsibility for registering actions and actionhandlers,
    /// and contains methods which will be invoked whenever a change is made to ex. a document, media or member
    ///
    /// An action/actionhandler will automatically be registered, using reflection
    /// which is enabling thirdparty developers to extend the core functionality of
    /// umbraco without changing the codebase.
    /// </summary>
    [Obsolete("Actions and ActionHandlers are obsolete and should no longer be used")]
    public class Action
    {
        private static readonly Dictionary<string, string> ActionJs = new Dictionary<string, string>();

        /// <summary>
        /// Jacascript for the contextmenu
        /// Suggestion: this method should be moved to the presentation layer.
        /// </summary>
        /// <param name="language"></param>
        /// <returns>String representation</returns>
        public string ReturnJavascript(string language)
        {
            return findActions(language);
        }

        /// <summary>
        /// Returns a list of JavaScript file paths.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetJavaScriptFileReferences()
        {
            return Current.Actions
                .Where(x => !string.IsNullOrWhiteSpace(x.JsSource))
                .Select(x => x.JsSource).ToList();
            //return ActionJsReference;
        }

        /// <summary>
        /// Javascript menuitems - tree contextmenu
        /// Umbraco console
        ///
        /// Suggestion: this method should be moved to the presentation layer.
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        private static string findActions(string language)
        {
            if (!ActionJs.ContainsKey(language))
            {
                string _actionJsList = "";

                foreach (IAction action in Current.Actions)
                {
                    // Adding try/catch so this rutine doesn't fail if one of the actions fail
                    // Add to language JsList
                    try
                    {
                        // NH: Add support for css sprites
                        string icon = action.Icon;
                        if (!string.IsNullOrEmpty(icon) && icon.StartsWith("."))
                            icon = icon.Substring(1, icon.Length - 1);
                        else
                            icon = "images/" + icon;

                        _actionJsList += string.Format(",\n\tmenuItem(\"{0}\", \"{1}\", \"{2}\", \"{3}\")",
                            action.Letter, icon, Current.Services.TextService.Localize("actions/"+ action.Alias, new[] { language }), action.JsFunctionName);
                    }
                    catch (Exception ex)
                    {
                        Current.Logger.Error<Action>(ex, "Error registrering action to javascript");
                    }
                }

                if (_actionJsList.Length > 0)
                    _actionJsList = _actionJsList.Substring(2, _actionJsList.Length - 2);

                _actionJsList = "\nvar menuMethods = new Array(\n" + _actionJsList + "\n)\n";
                ActionJs.Add(language, _actionJsList);
            }

            return ActionJs[language];

        }

        internal static List<IAction> FromEntityPermission(EntityPermission entityPermission)
        {
            List<IAction> list = new List<IAction>();
            foreach (var c in entityPermission.AssignedPermissions.Where(x => x.Length == 1).Select(x => x.ToCharArray()[0]))
            {
                IAction action = Current.Actions.ToList().Find(
                    delegate (IAction a)
                    {
                        return a.Letter == c;
                    }
                );
                if (action != null)
                    list.Add(action);
            }
            return list;
        }

        /// <summary>
        /// Returns a list of IActions that are permission assignable
        /// </summary>
        /// <returns></returns>
        public static List<IAction> GetPermissionAssignable()
        {
            return Current.Actions.ToList().FindAll(x => x.CanBePermissionAssigned);
        }

        /// <summary>
        /// Check if the current IAction is using legacy javascript methods
        /// </summary>
        /// <param name="action"></param>
        /// <returns>false if the Iaction is incompatible with 4.5</returns>
        public static bool ValidateActionJs(IAction action)
        {
            return !action.JsFunctionName.Contains("+");
        }

        /// <summary>
        /// Method to convert the old modal calls to the new ones
        /// </summary>
        /// <param name="javascript"></param>
        /// <returns></returns>
        public static string ConvertLegacyJs(string javascript)
        {
            MatchCollection tags =
    Regex.Matches(javascript, "openModal[^;]*;", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            foreach (Match tag in tags)
            {
                string[] function = tag.Value.Split(',');
                if (function.Length > 0)
                {
                    string newFunction = "UmbClientMgr.openModalWindow" + function[0].Substring(9).Replace("parent.nodeID", "UmbClientMgr.mainTree().getActionNode().nodeId").Replace("nodeID", "UmbClientMgr.mainTree().getActionNode().nodeId").Replace("parent.returnRandom()", "'" + Guid.NewGuid().ToString() + "'");
                    newFunction += ", " + function[1];
                    newFunction += ", true";
                    newFunction += ", " + function[2];
                    newFunction += ", " + function[3];
                    javascript = javascript.Replace(tag.Value, newFunction);
                }
            }

            return javascript;
        }
    }

    /// <summary>
    /// This class is used to manipulate IActions that are implemented in a wrong way
    /// For instance incompatible trees with 4.0 vs 4.5
    /// </summary>
    public class PlaceboAction : IAction
    {
        public char Letter { get; set; }
        public bool ShowInNotifier { get; set; }
        public bool CanBePermissionAssigned { get; set; }
        public string Icon { get; set; }
        public string Alias { get; set; }
        public string JsFunctionName { get; set; }
        public string JsSource { get; set; }

        public PlaceboAction() { }
        public PlaceboAction(IAction legacyAction)
        {
            Letter = legacyAction.Letter;
            ShowInNotifier = legacyAction.ShowInNotifier;
            CanBePermissionAssigned = legacyAction.CanBePermissionAssigned;
            Icon = legacyAction.Icon;
            Alias = legacyAction.Alias;
            JsFunctionName = legacyAction.JsFunctionName;
            JsSource = legacyAction.JsSource;
        }
    }

}
