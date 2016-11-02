<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../masterpages/umbracoPage.Master" CodeBehind="PermissionEditor.aspx.cs" Inherits="umbraco.cms.presentation.user.PermissionEditor" %>

<%@ Register Src="../controls/Tree/TreeControl.ascx" TagName="TreeControl" TagPrefix="umbraco" %>
<%@ Register Src="NodePermissions.ascx" TagName="NodePermissions" TagPrefix="user" %>
<%@ Register TagPrefix="ui" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<asp:Content ContentPlaceHolderID="head" runat="server">
		
	<umb:CssInclude ID="CssInclude2" runat="server" FilePath="css/permissionsEditor.css" PathNameAlias="UmbracoRoot" />
	<umb:CssInclude ID="CssInclude1" runat="server" FilePath="css/umbracoGui.css" PathNameAlias="UmbracoRoot" />
	<umb:JsInclude ID="JsInclude1"  runat="server" FilePath="PermissionsEditor.js" />
	
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">

	<ui:UmbracoPanel ID="pnlUmbraco" runat="server" hasMenu="true" Text="Content Tree Permissions" Width="608px">
		<ui:Pane ID="pnl1" Style="padding: 10px; text-align: left;" runat="server" Text="Select pages to modify their permissions">
			<div id="treeContainer">				
				<umbraco:TreeControl runat="server" ID="JTree" App="content"
				    Mode="Checkbox" CssClass="clearfix"></umbraco:TreeControl>				
			</div>

			<div id="permissionsPanel" style="margin-top: -35px">
				<user:NodePermissions ID="nodePermissions" runat="server" />
			</div>			
			
			<script type="text/javascript" language="javascript">				
				jQuery(document).ready(function() {
					jQuery("#<%=JTree.ClientID%>").PermissionsEditor({
						userId: <%=Request.QueryString["id"] %>,
						pPanelSelector: "#permissionsPanel",
						replacePChkBoxSelector: "#chkChildPermissions"});						
				});
				function SavePermissions() {
					jQuery("#treeContainer .umbTree").PermissionsEditorAPI().beginSavePermissions();
				}
			</script>
    
		</ui:Pane>
	</ui:UmbracoPanel>
	    <script type="text/javascript">
	        jQuery(document).ready(function () {
	            UmbClientMgr.appActions().bindSaveShortCut();
	        });
    </script>

</asp:Content>
