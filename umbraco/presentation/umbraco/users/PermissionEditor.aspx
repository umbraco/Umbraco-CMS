<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../masterpages/umbracoPage.Master" CodeBehind="PermissionEditor.aspx.cs" Inherits="umbraco.cms.presentation.user.PermissionEditor" %>

<%@ Register Src="../controls/TreeControl.ascx" TagName="TreeControl" TagPrefix="umbraco" %>
<%@ Register Src="NodePermissions.ascx" TagName="NodePermissions" TagPrefix="user" %>
<%@ Register Namespace="umbraco.presentation.controls" Assembly="umbraco" TagPrefix="tree" %>
<%@ Register TagPrefix="ui" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ContentPlaceHolderID="head" runat="server">
	<script type="text/javascript" src="/umbraco_client/Application/JQuery/jquery.metadata.min.js"></script>
	<link rel="stylesheet" type="text/css" href="/umbraco_client/Tree/Themes/tree_component.css" />
	<link rel="stylesheet" type="text/css" href="/umbraco/css/umbracoGui.css" />
	<link rel="stylesheet" type="text/css" href="../css/permissionsEditor.css" />

	<script type="text/javascript" src="PermissionsEditor.js"></script>
	
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">

	<ui:UmbracoPanel ID="pnlUmbraco" runat="server" hasMenu="true" Text="Content Tree Permissions" Width="608px">
		<ui:Pane ID="pnl1" Style="padding: 10px; text-align: left;" runat="server" Text="Select pages to modify their permissions">
			<div id="treeContainer">				
				<umbraco:TreeControl runat="server" ID="JTree" TreeType="Checkbox" CustomContainerId="permissionsTreeContainer"></umbraco:TreeControl>
			</div>
			<div id="permissionsPanel">
				<user:NodePermissions ID="nodePermissions" runat="server" />
			</div>			
			
			<script type="text/javascript" language="javascript">				
				jQuery(document).ready(function() {
					jQuery("#treeContainer .umbTree").PermissionsEditor({
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
	
</asp:Content>
