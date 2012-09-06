<%@ Page Title="" Language="C#" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="true" CodeBehind="insertMasterpagePlaceholder.aspx.cs" Inherits="umbraco.presentation.umbraco.dialogs.insertMasterpagePlaceholder" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
  <script type="text/javascript">

  function insertCode() {
    var idtb = document.getElementById("<%= tb_alias.ClientID %>");
    var id = idtb.value;
    
    top.right.insertPlaceHolderElement(id);
    UmbClientMgr.closeModalWindow();
  }

</script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
  <div class="notice">
  <p>
    <%= umbraco.ui.Text("defaultdialogs", "templateContentAreaHelp")%>
  </p>
  </div>
   
  <cc1:Pane ID="Pane1" runat="server">
  <p>
  <%= umbraco.ui.Text("placeHolderID") %><br />
  <asp:TextBox ID="tb_alias" Width="350px" CssClass="bigInput" runat="server" />
  </p>
  </cc1:Pane>
  
  <p>
    <input type="button" onclick="insertCode(); return false;" value="<%= umbraco.ui.Text("insert") %>" /> <em><%= umbraco.ui.Text("or") %></em> <a href="#" onclick="UmbClientMgr.closeModalWindow(); return false;"><%= umbraco.ui.Text("cancel") %></a>
  </p>

</asp:Content>