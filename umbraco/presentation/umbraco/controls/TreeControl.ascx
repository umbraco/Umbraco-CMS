<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TreeControl.ascx.cs" Inherits="umbraco.presentation.umbraco.controls.TreeControl" %>

<%@ Register TagPrefix="umb" Namespace="umbraco.presentation.ClientDependency" Assembly="umbraco.presentation.ClientDependency" %>

<umb:ClientDependencyInclude runat="server" id="ClientDependencyInclude7" DependencyType="Css" FilePath="Tree/Themes/tree_component.css" PathNameAlias="UmbracoClient" />

<umb:ClientDependencyInclude ID="ClientDependencyInclude1" runat="server" DependencyType="Javascript" FilePath="Application/JQuery/jquery.metadata.min.js" PathNameAlias="UmbracoClient" Priority="10" CompositeGroupName="UmbTree" />
<umb:ClientDependencyInclude ID="ClientDependencyInclude6" runat="server" DependencyType="Javascript" FilePath="Application/JQuery/jquery.cookie.js" PathNameAlias="UmbracoClient" Priority="10" CompositeGroupName="UmbTree" />
<umb:ClientDependencyInclude ID="ClientDependencyInclude2" runat="server" DependencyType="Javascript" FilePath="Tree/css.js" PathNameAlias="UmbracoClient" Priority="10" CompositeGroupName="UmbTree" />
<umb:ClientDependencyInclude ID="ClientDependencyInclude5" runat="server" DependencyType="Javascript" FilePath="Tree/tree_component.min.js" PathNameAlias="UmbracoClient" Priority="11" CompositeGroupName="UmbTree" />
<umb:ClientDependencyInclude ID="ClientDependencyInclude3" runat="server" DependencyType="Javascript" FilePath="Tree/NodeDefinition.js" PathNameAlias="UmbracoClient" Priority="12" CompositeGroupName="UmbTree" />
<umb:ClientDependencyInclude ID="ClientDependencyInclude4" runat="server" DependencyType="Javascript" FilePath="Tree/UmbracoTree.js" PathNameAlias="UmbracoClient" Priority="13" CompositeGroupName="UmbTree" />

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