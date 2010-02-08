<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../masterpages/umbracoDialog.Master" CodeBehind="RegexWs.aspx.cs" Inherits="umbraco.presentation.dialogs.RegexWs" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ContentPlaceHolderID="body" runat="server">
	<umb:JsInclude ID="JsInclude1" runat="server" FilePath="js/umbracoCheckKeys.js" PathNameAlias="UmbracoRoot"/>
	
	<cc1:Pane id="pane1" runat="server">
	  <cc1:PropertyPanel ID="pp_search" Text="Search" runat="server">
		  <asp:TextBox ID="searchField" style="width: 250px;" runat="server" /> <asp:Button ID="bt_search" runat="server" Text="search" OnClick="findRegex" />
				<p>
				<small><%= umbraco.ui.Text("defaultdialogs", "regexSearchHelp")%> </small>
				</p>
	  </cc1:PropertyPanel>
	  
	  <cc1:PropertyPanel runat="server">
		  <asp:Panel ID="regexPanel" Visible="false" runat="server">
		  <div class="diff">
			<asp:Repeater id="results" runat="server" OnItemDataBound="onRegexBind">
			  <ItemTemplate>
				<div class="match">
				  <h3><asp:Literal ID="header" runat="server" /></h3>
					<p>
					  <asp:Literal ID="desc" runat="server" />
					</p>
					<p>
					  <pre><asp:Literal ID="regex" runat="server" /></pre>
					</p>
				</div>
			  </ItemTemplate>
			</asp:Repeater>
		  </div>      
		  </asp:Panel>
	  </cc1:PropertyPanel>
	</cc1:Pane>
      

</asp:Content>

<asp:Content ContentPlaceHolderID="head" runat="server">
  <script type="text/javascript">
	  function chooseRegex(regex) {
		var target = top.right.document.getElementById('<%= Request.QueryString["target"] %>');
		target.value = regex;
		UmbClientMgr.closeModalWindow(); 
	}
  </script>
    
  <style type="text/css">
 	.diff{height: 357px; width: 98%; overflow: auto; font-family: verdana; font-size: 11px;}
	.diff pre{display: block; width: 80%; background: #EFEFF5; margin: 10px; margin-left: 0px; padding: 10px; overflow: hidden;}
	.match{border-bottom: 1px solid #ccc; padding: 5px; margin-left: 10px; margin-bottom: 10px;}
	.match h3{font-size: 14px; padding-left: 0px; margin-left: 0px;}  
	</style>
</asp:Content>
    