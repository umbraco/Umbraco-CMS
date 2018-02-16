using System;
using System.Net.Http.Formatting;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Xslt)]
    [Tree(Constants.Applications.Developer, Constants.Trees.Xslt, null, sortOrder: 5)]
    [LegacyBaseTree(typeof(loadXslt))]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class XsltTreeController : FileSystemTreeController
    {
        protected override void OnRenderFileNode(ref TreeNode treeNode)
        {
            ////TODO: This is all hacky ... don't have time to convert the tree, views and dialogs over properly so we'll keep using the legacy views
            treeNode.AssignLegacyJsCallback("javascript:UmbClientMgr.contentFrame('developer/xslt/editXslt.aspx?file=" + treeNode.Id + "');");
        }

        protected override void OnRenderFolderNode(ref TreeNode treeNode)
        {
            //TODO: This is all hacky ... don't have time to convert the tree, views and dialogs over properly so we'll keep using the legacy views
            treeNode.AssignLegacyJsCallback("javascript:void(0);");
        }

        protected override MenuItemCollection GetMenuForFile(string path, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //TODO: This is all hacky ... don't have time to convert the tree, views and dialogs over properly so we'll keep using the legacy dialogs
            var menuItem = menu.Items.Add(ActionDelete.Instance, Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)));
            var legacyConfirmView = LegacyTreeDataConverter.GetLegacyConfirmView(ActionDelete.Instance);
            if (legacyConfirmView == false)
                throw new InvalidOperationException("Could not resolve the confirmation view for the legacy action " + ActionDelete.Instance.Alias);
            menuItem.LaunchDialogView(
                legacyConfirmView.Result,
                Services.TextService.Localize("general/delete"));

            return menu;
        }

        protected override MenuItemCollection GetMenuForRootNode(FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //set the default to create
            menu.DefaultMenuAlias = ActionNew.Instance.Alias;

            // root actions
            menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)))
                .ConvertLegacyMenuItem(null, Constants.Trees.Xslt, queryStrings.GetValue<string>("application"));

            menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
            return menu;
        }

        protected override IFileSystem2 FileSystem
        {
            get { return FileSystemProviderManager.Current.XsltFileSystem; }
        }

        private static readonly string[] ExtensionsStatic = { "xslt" };

        protected override string[] Extensions
        {
            get { return ExtensionsStatic; }
        }
        protected override string FileIcon
        {
            get { return "icon-code"; }
        }
    }
}