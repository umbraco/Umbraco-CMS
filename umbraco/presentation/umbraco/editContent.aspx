<%@ Page Title="Edit content" Language="c#" MasterPageFile="masterpages/umbracoPage.Master" CodeBehind="editContent.aspx.cs" ValidateRequest="false" AutoEventWireup="True" Inherits="umbraco.cms.presentation.editContent" Trace="false" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.presentation.ClientDependency.Controls" Assembly="umbraco.presentation.ClientDependency" %>
<asp:Content ContentPlaceHolderID="head" runat="server">

	<script type="text/javascript">
		// Save handlers for IDataFields		
		var saveHandlers = new Array();
		
		// A hack to make sure that any javascript can access page id and version
		<asp:Literal id="jsIds" runat="server"></asp:Literal>
		
		// For short-cut keys
		var isDialog = true;
		var functionsFrame = this;
		var disableEnterSubmit = true;
	
		
		function addSaveHandler(handler) {
			saveHandlers[saveHandlers.length] = handler;
		}		
		
		function invokeSaveHandlers() {
			for (var i=0;i<saveHandlers.length;i++) {
				eval(saveHandlers[i]);
			}
		}
		
		function doSubmit() {
			invokeSaveHandlers();
			document.getElementById("TabView1_tab01layer_save").click();
		}
	</script>	

</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="body" runat="server">
	<umb:JsInclude ID="JsInclude1" runat="server" FilePath="js/umbracoCheckKeys.js" PathNameAlias="UmbracoRoot" />
	
	<table style="height:38px;width:371px;border:none 0px;" cellspacing="0" cellpadding="0">
		<tr valign="top">
			<td height="20">
			</td>
			<td>
				<asp:PlaceHolder ID="plc" runat="server"></asp:PlaceHolder>
			</td>
		</tr>
	</table>
	<input id="doSave" type="hidden" name="doSave" runat="server" />
	<input id="doPublish" type="hidden" name="doPublish" runat="server" />
	
</asp:Content>
