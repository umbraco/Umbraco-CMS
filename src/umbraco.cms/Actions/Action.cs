using System;
using System.Collections;
using System.Collections.Generic;
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

        private static readonly object Lock = new object();

        static Action()
        {
            ReRegisterActionsAndHandlers();
        }

        /// <summary>
        /// This is used when an IAction or IActionHandler is installed into the system
        /// and needs to be loaded into memory.
        /// </summary>
        /// <remarks>
        /// TODO: this shouldn't be needed... we should restart the app pool when a package is installed!
        /// </remarks>
        public static void ReRegisterActionsAndHandlers()
        {
            lock (Lock)
            {
                // NOTE use the DirtyBackdoor to change the resolution configuration EXCLUSIVELY
                // ie do NOT do ANYTHING else while holding the backdoor, because while it is open
                // the whole resolution system is locked => nothing can work properly => deadlocks

                var newResolver = new ActionsResolver(
                    new ActivatorServiceProvider(), LoggerResolver.Current.Logger,
                        () => TypeFinder.FindClassesOfType<IAction>(PluginManager.Current.AssembliesToScan));

                using (Umbraco.Core.ObjectResolution.Resolution.DirtyBackdoorToConfiguration)
                {
                    ActionsResolver.Reset(false); // and do NOT reset the whole resolution!
                    ActionsResolver.Current = newResolver;
                }

            }
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
        public static List<IAction> FromString(string actions)
        {
            List<IAction> list = new List<IAction>();
            foreach (char c in actions.ToCharArray())
            {
                IAction action = ActionsResolver.Current.Actions.ToList().Find(
                    delegate(IAction a)
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
        /// Returns the string representation of the actions that make up the actions collection
        /// </summary>
        /// <returns></returns>
        public static string ToString(List<IAction> actions)
        {
            string[] strMenu = Array.ConvertAll<IAction, string>(actions.ToArray(), delegate(IAction a) { return (a.Letter.ToString()); });
            return string.Join("", strMenu);
        }

        /// <summary>
        /// Returns a list of IActions that are permission assignable
        /// </summary>
        /// <returns></returns>
        public static List<IAction> GetPermissionAssignable()
        {
            return ActionsResolver.Current.Actions.ToList().FindAll(
                delegate(IAction a)
                {
                    return (a.CanBePermissionAssigned);
                }
            );
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
