<%@ Page Language="c#" CodeBehind="moveOrCopy.aspx.cs" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="True" Inherits="Umbraco.Web.UI.Umbraco.Dialogs.MoveOrCopy" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Import Namespace="Umbraco.Web" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register Src="../controls/Tree/TreeControl.ascx" TagName="TreeControl" TagPrefix="umbraco" %>

<asp:Content ContentPlaceHolderID="head" runat="server">

	<script type="text/javascript">

			function dialogHandler(id) {
				document.getElementById("copyTo").value = id;
				document.getElementById("<%= ok.ClientID %>").disabled = false;
				
				// Get node name by xmlrequest
				if (id > 0)
						umbraco.presentation.webservices.CMSNode.GetNodeName('<%=umbracoUserContextID%>', id, updateName);
				else{
					//document.getElementById("pageNameContent").innerHTML = "'<strong><%=umbraco.ui.Text(Request.CleanForXss("app"))%></strong>' <%= umbraco.ui.Text("moveOrCopy","nodeSelected") %>";
			    
					jQuery("#pageNameContent").html("<strong><%=umbraco.ui.Text(Request.CleanForXss("app"))%></strong> <%= umbraco.ui.Text("moveOrCopy","nodeSelected") %>");
					jQuery("#pageNameHolder").attr("class","success");
			  }
			}
			
            var actionIsValid = true;

			function updateName(result) {
                if(actionIsValid)
                {
				    jQuery("#pageNameContent").html("'<strong>" + result + "</strong>' <%= umbraco.ui.Text("moveOrCopy","nodeSelected") %>");
				    jQuery("#pageNameHolder").attr("class","success");
                }
			}

           
            function notValid()
            {
                jQuery("#pageNameHolder").attr("class", "error");
                jQuery("#pageNameContent").html("<%= umbraco.ui.Text("moveOrCopy","notValid") %>");
                actionIsValid = false;
            }
	
	</script>

	

	<style type="text/css">
		.propertyItemheader
		{
			width: 180px !important;
		}
	</style>
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">
	<umb:JsInclude ID="JsInclude1" runat="server" FilePath="js/umbracoCheckKeys.js" PathNameAlias="UmbracoRoot"/>
	
	<input type="hidden" id="copyTo" name="copyTo" />
	<cc1:Feedback ID="feedback" runat="server" />
	<cc1:Pane ID="pane_form" runat="server" Visible="false">
		<cc1:PropertyPanel runat="server" Style="overflow: auto; height: 220px;position: relative;">
			<umbraco:TreeControl runat="server" ID="JTree" App='<%#Request.CleanForXss("app") %>'
                IsDialog="true" DialogMode="id" ShowContextMenu="false" FunctionToCall="dialogHandler"
                Height="200"></umbraco:TreeControl>
		</cc1:PropertyPanel>
		<cc1:PropertyPanel runat="server" ID="pp_relate" Text="relateToOriginal">
			<asp:CheckBox runat="server" ID="RelateDocuments" Checked="false" />
		</cc1:PropertyPanel>
	</cc1:Pane>
	<asp:PlaceHolder ID="pane_form_notice" runat="server" Visible="false">
		<div class="notice" id="pageNameHolder" style="margin-top: 10px;">
			<p id="pageNameContent">
				<%= umbraco.ui.Text("moveOrCopy","noNodeSelected") %></p>
		</div>
	</asp:PlaceHolder>
	<cc1:Pane ID="pane_settings" runat="server" Visible="false">
		<cc1:PropertyPanel ID="PropertyPanel1" runat="server" Text="Master Document Type">
			<asp:ListBox ID="masterType" runat="server" CssClass="bigInput" Rows="1" SelectionMode="Single"></asp:ListBox>
		</cc1:PropertyPanel>
		<cc1:PropertyPanel runat="server" Text="Name">
			<asp:TextBox ID="rename" runat="server" Style="width: 350px;" CssClass="bigInput"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator>
		</cc1:PropertyPanel>
	</cc1:Pane>
	<asp:Panel ID="panel_buttons" runat="server">
		<p>
			<asp:Button ID="ok" runat="server" CssClass="guiInputButton" OnClick="HandleMoveOrCopy" UseSubmitBehavior="false" OnClientClick="this.disabled = 'disabled';"></asp:Button>
			&nbsp; <em>
				<%=umbraco.ui.Text("general", "or", UmbracoUser)%></em> &nbsp; <a href="#" style="color: blue" onclick="UmbClientMgr.closeModalWindow()">
					<%=umbraco.ui.Text("general", "cancel", UmbracoUser)%></a>
		</p>
	</asp:Panel>
</asp:Content>
