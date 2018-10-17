using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Services;
using Umbraco.Web._Legacy.Actions;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Converts the legacy tree data to the new format
    /// </summary>
    internal class LegacyTreeDataConverter
    {
        /// <summary>
        /// This will look at the legacy IAction's JsFunctionName and convert it to a confirmation dialog view if possible
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static Attempt<string> GetLegacyConfirmView(IAction action)
        {
            if (action.JsFunctionName.IsNullOrWhiteSpace())
            {
                return Attempt<string>.Fail();
            }

            switch (action.JsFunctionName)
            {
                case "UmbClientMgr.appActions().actionDelete()":
                    return Attempt.Succeed(
                        UmbracoConfig.For.GlobalSettings().Path.EnsureEndsWith('/') + "views/common/dialogs/legacydelete.html");
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
            if (action.JsFunctionName.IsNullOrWhiteSpace())
            {
                return Attempt<LegacyUrlAction>.Fail();
            }

            switch (action.JsFunctionName)
            {
                case "UmbClientMgr.appActions().actionNew()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "create.aspx?nodeId=" + nodeId + "&nodeType=" + nodeType + "&nodeName=" + nodeName + "&rnd=" + DateTime.UtcNow.Ticks,
                            Current.Services.TextService.Localize("actions/create")));
                case "UmbClientMgr.appActions().actionNewFolder()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "createFolder.aspx?nodeId=" + nodeId + "&nodeType=" + nodeType + "&nodeName=" + nodeName + "&rnd=" + DateTime.UtcNow.Ticks,
                            Current.Services.TextService.Localize("actions/create")));
                case "UmbClientMgr.appActions().actionProtect()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/protectPage.aspx?mode=cut&nodeId=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            Current.Services.TextService.Localize("actions/protect")));
                case "UmbClientMgr.appActions().actionRollback()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/rollback.aspx?nodeId=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            Current.Services.TextService.Localize("actions/rollback")));
                case "UmbClientMgr.appActions().actionNotify()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/notifications.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            Current.Services.TextService.Localize("actions/notify")));
                case "UmbClientMgr.appActions().actionPublish()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/publish.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            Current.Services.TextService.Localize("actions/publish")));
                case "UmbClientMgr.appActions().actionChangeDocType()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/ChangeDocType.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            Current.Services.TextService.Localize("actions/changeDocType")));
                case "UmbClientMgr.appActions().actionToPublish()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/SendPublish.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            Current.Services.TextService.Localize("actions/sendtopublish")));
                case "UmbClientMgr.appActions().actionRePublish()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/republish.aspx?rnd=" + DateTime.UtcNow.Ticks,
                            Current.Services.TextService.Localize("actions/republish")));
                case "UmbClientMgr.appActions().actionSendToTranslate()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/sendToTranslation.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            Current.Services.TextService.Localize("actions/sendToTranslate")));

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
