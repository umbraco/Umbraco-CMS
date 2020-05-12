using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.BasePages;
using umbraco.BusinessLogic.Utils;
using umbraco.cms;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.workflow;
using umbraco.interfaces;
using System.Text.RegularExpressions;
using System.Linq;
using Umbraco.Core.IO;
using TypeFinder = Umbraco.Core.TypeFinder;

namespace umbraco.BusinessLogic.Actions
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
        
        [Obsolete("This no longer performs any action there is never a reason to rescan because the app domain will be restarted if new IActions are added because they are included in assemblies")]
        public static void ReRegisterActionsAndHandlers()
        {            
        }

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
            return ActionsResolver.Current.Actions
                .Where(x => !string.IsNullOrWhiteSpace(x.JsSource))
                .Select(x => x.JsSource).ToList();
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

                foreach (IAction action in ActionsResolver.Current.Actions)
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
                            action.Letter, icon, ui.GetText("actions", action.Alias, language), action.JsFunctionName);
                    }
                    catch (Exception ee)
                    {
                        LogHelper.Error<Action>("Error registrering action to javascript", ee);
                    }
                }

                if (_actionJsList.Length > 0)
                    _actionJsList = _actionJsList.Substring(2, _actionJsList.Length - 2);

                _actionJsList = "\nvar menuMethods = new Array(\n" + _actionJsList + "\n)\n";
                ActionJs.Add(language, _actionJsList);
            }

            return ActionJs[language];

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>An arraylist containing all javascript variables for the contextmenu in the tree</returns>
        [Obsolete("Use ActionsResolver.Current.Actions instead")]
        public static ArrayList GetAll()
        {
            return new ArrayList(ActionsResolver.Current.Actions.ToList());
        }

        /// <summary>
        /// This method will return a list of IAction's based on a string list. Each character in the list may represent
        /// an IAction. This will associate any found IActions based on the Letter property of the IAction with the character being referenced.
        /// </summary>
        /// <param name="actions"></param>
        /// <returns>returns a list of actions that have an associated letter found in the action string list</returns>
        [Obsolete("Use ActionsResolver.Current.FromActionSymbols instead")]
        public static List<IAction> FromString(string actions)
        {
            return ActionsResolver.Current.FromActionSymbols(actions.ToCharArray().Select(x => x.ToString())).ToList();            
        }

        /// <summary>
        /// Returns the string representation of the actions that make up the actions collection
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use ActionsResolver.Current.ToActionSymbols instead")]
        public static string ToString(List<IAction> actions)
        {
            string[] strMenu = Array.ConvertAll(actions.ToArray(), a => (a.Letter.ToString(CultureInfo.InvariantCulture)));
            return string.Join("", strMenu);
        }

        /// <summary>
        /// Returns a list of IActions that are permission assignable
        /// </summary>
        /// <returns></returns>
        public static List<IAction> GetPermissionAssignable()
        {
            return ActionsResolver.Current.Actions.ToList().FindAll(a => (a.CanBePermissionAssigned));
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
