<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TreeControl.ascx.cs" Inherits="umbraco.presentation.umbraco.controls.TreeControl" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<umb:CssInclude ID="CssInclude2" runat="server" FilePath="Tree/treeIcons.css" PathNameAlias="UmbracoClient" Priority="10" />
<umb:CssInclude ID="CssInclude3" runat="server" FilePath="Tree/menuIcons.css" PathNameAlias="UmbracoClient" Priority="11" />

<umb:JsInclude ID="JsInclude1" runat="server" FilePath="Application/NamespaceManager.js" PathNameAlias="UmbracoClient" Priority="0" />
<umb:JsInclude ID="JsInclude2" runat="server" FilePath="Application/UmbracoClientManager.js" PathNameAlias="UmbracoClient" />
<umb:JsInclude ID="JsInclude3" runat="server" FilePath="Application/UmbracoApplicationActions.js" PathNameAlias="UmbracoClient" />
<umb:JsInclude ID="JsInclude4" runat="server" FilePath="Application/UmbracoUtils.js" PathNameAlias="UmbracoClient" />
<umb:JsInclude ID="JsInclude5" runat="server" FilePath="ui/jquery.js" PathNameAlias="UmbracoClient" Priority="0" />
<umb:JsInclude ID="JsInclude6" runat="server" FilePath="Application/JQuery/jquery.metadata.min.js" PathNameAlias="UmbracoClient" Priority="10" />
<umb:JsInclude ID="JsInclude8" runat="server" FilePath="Tree/jquery.tree.js" PathNameAlias="UmbracoClient" Priority="11"  />
<umb:JsInclude ID="JsInclude11" runat="server" FilePath="Tree/UmbracoContext.js" PathNameAlias="UmbracoClient" Priority="12"  />
<umb:JsInclude ID="JsInclude7" runat="server" FilePath="Tree/jquery.tree.contextmenu.js" PathNameAlias="UmbracoClient" Priority="12"  />
<umb:JsInclude ID="JsInclude12" runat="server" FilePath="Tree/jquery.tree.checkbox.js" PathNameAlias="UmbracoClient" Priority="12"  />
<umb:JsInclude ID="JsInclude9" runat="server" FilePath="Tree/NodeDefinition.js" PathNameAlias="UmbracoClient" Priority="12"  />
<umb:JsInclude ID="JsInclude10" runat="server" FilePath="Tree/UmbracoTree.js" PathNameAlias="UmbracoClient" Priority="13" />

<script type="text/javascript">

jQuery(document).ready(function() {
	var ctxMenu = <%#GetJSONContextMenu() %>;
	var initNode = <%#GetJSONInitNode() %>;
	var app = "<%#TreeSvc.App%>";
	var showContext = <%#TreeSvc.ShowContextMenu.ToString().ToLower()%>;
	var isDialog = <%#TreeSvc.IsDialog.ToString().ToLower()%>;

	jQuery("#<%#string.IsNullOrEmpty(CustomContainerId) ? "treeContainer" : CustomContainerId %>").UmbracoTree({
        jsonFullMenu: ctxMenu,
        jsonInitNode: initNode,
        appActions: UmbClientMgr.appActions(),
        uiKeys: UmbClientMgr.uiKeys(),
        app: app,
        showContext: showContext,
        isDialog: isDialog,
        umb_clientFolderRoot: "<%#umbraco.GlobalSettings.ClientPath%>",
        treeType: "<%#TreeType.ToString().ToLower()%>",
        dataUrl: "<%#umbraco.GlobalSettings.Path%>/webservices/TreeDataService.ashx",
        serviceUrl: "<%#umbraco.GlobalSettings.Path%>/webservices/TreeClientService.asmx/GetInitAppTreeData"});
        
	//add event handler for ajax errors, this will refresh the whole application
	UmbClientMgr.mainTree().addEventHandler("ajaxError", function(e) {
		if (e.msg == "rebuildTree") {
			UmbClientMgr.mainWindow("umbraco.aspx");
		}
	});
	
});	

</script>

<div class="<%#TreeType.ToString().ToLower()%>" id="<%#string.IsNullOrEmpty(CustomContainerId) ? "treeContainer" : CustomContainerId %>">
</div>