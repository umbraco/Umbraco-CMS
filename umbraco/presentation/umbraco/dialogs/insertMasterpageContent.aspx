<%@ Page Title="" Language="C#" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="true" CodeBehind="insertMasterpageContent.aspx.cs" Inherits="umbraco.presentation.umbraco.dialogs.insertMasterpageContent" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

<script type="text/javascript">

  function insertCode() {
    var idDD = document.getElementById("<%= dd_detectedAlias.ClientID %>");
    var id = idDD.options[idDD.selectedIndex].value;
    top.right.insertContentElement(id);
    UmbClientMgr.closeModalWindow();
  }
</script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
  <div class="notice">
  <p>
    <%= umbraco.ui.Text("defaultdialogs", "templateContentPlaceHolderHelp")%>
  </p>
  </div>
   
  <cc1:Pane runat="server">
  <p>
    <%= umbraco.ui.Text("placeHolderID") %>:<br />
    <asp:DropDownList ID="dd_detectedAlias" Width="350px" CssClass="bigInput" runat="server" />
  </p>
  </cc1:Pane>
  <p>
    <input type="button" onclick="insertCode(); return false;" value="<%= umbraco.ui.Text("insert") %>" /> <em><%= umbraco.ui.Text("or") %></em> <a href="#" onclick="UmbClientMgr.closeModalWindow(); return false;"><%= umbraco.ui.Text("cancel") %></a>
  </p>
  
</asp:Content>