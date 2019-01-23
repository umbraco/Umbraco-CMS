using System;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;

using Umbraco.Web.Composing;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Converts the legacy tree data to the new format
    /// </summary>
    internal class LegacyTreeDataConverter
    {
        //todo: remove this whole class when everything is angularized

        /// <summary>
        /// This will look at the legacy IAction's JsFunctionName and convert it to a confirmation dialog view if possible
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static Attempt<string> GetLegacyConfirmView(IAction action)
        {
            switch (action)
            {
                case ActionDelete actionDelete:
                    return Attempt.Succeed(
                        Current.Configs.Global().Path.EnsureEndsWith('/') + "views/common/dialogs/legacydelete.html");
            }

            return Attempt<string>.Fail();
        }

        /// <summary>
        /// This will look at a legacy IAction's JsFunctionName and convert it to a URL if possible.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="nodeName"></param>
        /// <param name="currentSection"></param>
        /// <param name="nodeId"></param>
        /// <param name="nodeType"></param>
        internal static Attempt<LegacyUrlAction> GetUrlAndTitleFromLegacyAction(IAction action, string nodeId, string nodeType, string nodeName, string currentSection)
        {
            switch (action)

            {
                case ActionNew actionNew:
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "create.aspx?nodeId=" + nodeId + "&nodeType=" + nodeType + "&nodeName=" + nodeName + "&rnd=" + DateTime.UtcNow.Ticks,
                            Current.Services.TextService.Localize("actions/create")));
            }
            return Attempt<LegacyUrlAction>.Fail();
        }

        internal class LegacyUrlAction
        {
            public LegacyUrlAction(string url, string dialogTitle)
                : this(url, dialogTitle, ActionUrlMethod.Dialog)
            {

            }

            public LegacyUrlAction(string url, string dialogTitle, ActionUrlMethod actionMethod)
            {
                Url = url;
                ActionMethod = actionMethod;
                DialogTitle = dialogTitle;
            }

            public string Url { get; private set; }
            public ActionUrlMethod ActionMethod { get; private set; }
            public string DialogTitle { get; private set; }
        }
    }
}
