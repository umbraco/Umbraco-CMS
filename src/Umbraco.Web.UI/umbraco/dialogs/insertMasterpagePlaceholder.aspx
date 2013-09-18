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
  
  <cc1:Pane ID="pane_insert" runat="server">
    <cc1:PropertyPanel runat="server">
            <p>
                <%= umbraco.ui.Text("defaultdialogs", "templateContentAreaHelp")%>
            </p>
    </cc1:PropertyPanel>
    <cc1:PropertyPanel runat="server" id="pp_placeholder" text="Placeholder ID">
            <asp:TextBox ID="tb_alias" Width="350px" CssClass="bigInput input-block-level" runat="server" />
    </cc1:PropertyPanel>
  </cc1:Pane>
   
  <cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
            <a class="btn btn-link" onclick="UmbClientMgr.closeModalWindow(); return false;"><%=umbraco.ui.Text("general", "cancel", this.getUser())%></a>
            <input type="button" onclick="insertCode(); return false;" Class="btn btn-primary" value="<%= umbraco.ui.Text("insert") %>" />
  </cc1:Pane>
</asp:Content>