<%@ Control Language="C#" AutoEventWireup="true" Inherits="umbraco.controls.Tree.TreeControl" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="umbClient" Namespace="Umbraco.Web.UI.Bundles" Assembly="umbraco" %>

<umb:CssInclude ID="CssInclude2" runat="server" FilePath="Tree/treeIcons.css" PathNameAlias="UmbracoClient" Priority="10" />
<umb:CssInclude ID="CssInclude3" runat="server" FilePath="Tree/menuIcons.css" PathNameAlias="UmbracoClient" Priority="11" />
<umb:CssInclude ID="CssInclude1" runat="server" FilePath="Tree/Themes/umbraco/style.css" PathNameAlias="UmbracoClient" Priority="12" />

<umbClient:JsApplicationLib ID="JsUmbracoApplicationLib1" runat="server"/>
<umbClient:JsJQueryCore ID="JsJQueryCore1" runat="server"/>
<umbClient:JsJQueryPlugins ID="JsJQueryPlugins1" runat="server"/>
<umbClient:JsUmbracoTree  runat="server"/>

<script type="text/javascript">
jQuery(document).ready(function() {
    var ctxMenu = <%#GetJSONContextMenu() %>;	
    var app = "<%#App%>";
    var showContext = <%#ShowContextMenu.ToString().ToLower()%>;
    var isDialog = <%#IsDialog.ToString().ToLower()%>;
    var dialogMode = "<%#DialogMode.ToString()%>";
    var treeType = "<%#TreeType%>";
    var functionToCall = "<%#FunctionToCall%>";
    var nodeKey = "<%#NodeKey%>";
	
    //create the javascript tree
    jQuery("#<%=ClientID%>").UmbracoTree({
        doNotInit: <%#ManualInitialization.ToString().ToLower()%>,
        jsonFullMenu: ctxMenu,
        appActions: UmbClientMgr.appActions(),
        deletingText: '<%=umbraco.ui.GetText("deleting")%>',
        app: app,
        showContext: showContext,
        isDialog: isDialog,
        dialogMode: dialogMode,
        treeType: treeType,
        functionToCall : functionToCall,
        nodeKey : nodeKey,
        treeMode: "<%#Mode.ToString().ToLower()%>",
        dataUrl: "<%#global::Umbraco.Core.IO.IOHelper.ResolveUrl(global::Umbraco.Core.IO.SystemDirectories.Umbraco)%>/webservices/TreeDataService.ashx",
        serviceUrl: "<%#global::Umbraco.Core.IO.IOHelper.ResolveUrl(global::Umbraco.Core.IO.SystemDirectories.Umbraco)%>/webservices/TreeClientService.asmx/GetInitAppTreeData"});
  
    <%if(string.IsNullOrEmpty(SelectedNodePath) == false) {%>
    setTimeout(function() {
        treeApi = jQuery("#<%=ClientID%>").UmbracoTreeAPI();
		        treeApi.syncTree('<%=SelectedNodePath%>', true, true);
    }, 500);
    <% } %>

    <%#GetLegacyIActionJavascript()%>
	
});	

</script>


<div runat="server" id="TreeContainer">
    <div id="<%=ClientID%>" class="<%#Mode.ToString().ToLower()%>">
    </div>
</div>