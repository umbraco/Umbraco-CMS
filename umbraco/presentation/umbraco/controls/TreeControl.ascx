<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TreeControl.ascx.cs" Inherits="umbraco.presentation.umbraco.controls.TreeControl" %>
<asp:ScriptManagerProxy runat="server">
	<Scripts>
		<%--<asp:ScriptReference Path="~/umbraco_client/Application/NamespaceManager.js" />--%>
		<asp:ScriptReference Path="~/umbraco_client/Tree/css.js" />
		<%--<asp:ScriptReference Path="~/umbraco_client/Application/JQuery/jquery.metadata.min.js" />--%>
		<asp:ScriptReference Path="~/umbraco_client/Tree/tree_component.min.js" />				
		<asp:ScriptReference Path="~/umbraco_client/Tree/NodeDefinition.js" />
		<asp:ScriptReference Path="~/umbraco_client/Tree/UmbracoTree.js" />
		<%--<asp:ScriptReference Path="~/umbraco_client/Application/UmbracoClientManager.js" />--%>
		<%--<asp:ScriptReference Path="~/umbraco_client/Application/UmbracoUtils.js" />--%>
	</Scripts>
</asp:ScriptManagerProxy>

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