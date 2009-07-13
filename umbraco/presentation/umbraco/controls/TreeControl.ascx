<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TreeControl.ascx.cs" Inherits="umbraco.presentation.umbraco.controls.TreeControl" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.presentation.ClientDependency" Assembly="umbraco.presentation.ClientDependency" %>

<umb:ClientDependencyInclude runat="server" id="ClientDependencyInclude7" DependencyType="Css" FilePath="Tree/Themes/tree_component.css" PathNameAlias="UmbracoClient" />

<umb:ClientDependencyInclude runat="server" id="ClientDependencyInclude8" DependencyType="Javascript" FilePath="Application/NamespaceManager.js" PathNameAlias="UmbracoClient" Priority="0" />
<umb:ClientDependencyInclude runat="server" id="ClientDependencyInclude13" DependencyType="Javascript" FilePath="Application/UmbracoClientManager.js" PathNameAlias="UmbracoClient" />
<umb:ClientDependencyInclude runat="server" id="ClientDependencyInclude11" DependencyType="Javascript" FilePath="Application/UmbracoApplicationActions.js" PathNameAlias="UmbracoClient" />
<umb:ClientDependencyInclude runat="server" id="ClientDependencyInclude12" DependencyType="Javascript" FilePath="Application/UmbracoUtils.js" PathNameAlias="UmbracoClient" />
<umb:ClientDependencyInclude runat="server" id="ClientDependencyInclude6" DependencyType="Javascript" FilePath="ui/jquery.js" PathNameAlias="UmbracoClient" Priority="0" />
<umb:ClientDependencyInclude runat="server" ID="ClientDependencyInclude1" DependencyType="Javascript" FilePath="Application/JQuery/jquery.metadata.min.js" PathNameAlias="UmbracoClient" Priority="10" />
<umb:ClientDependencyInclude runat="server" ID="ClientDependencyInclude2" DependencyType="Javascript" FilePath="Tree/css.js" PathNameAlias="UmbracoClient" Priority="10" />
<umb:ClientDependencyInclude runat="server" ID="ClientDependencyInclude5" DependencyType="Javascript" FilePath="Tree/tree_component.min.js" PathNameAlias="UmbracoClient" Priority="11"  />
<umb:ClientDependencyInclude runat="server" ID="ClientDependencyInclude3" DependencyType="Javascript" FilePath="Tree/NodeDefinition.js" PathNameAlias="UmbracoClient" Priority="12"  />
<umb:ClientDependencyInclude runat="server" ID="ClientDependencyInclude4" DependencyType="Javascript" FilePath="Tree/UmbracoTree.min.js" PathNameAlias="UmbracoClient" Priority="13" />

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