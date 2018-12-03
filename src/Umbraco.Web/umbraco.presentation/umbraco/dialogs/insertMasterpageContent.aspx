<%@ Page Title="" Language="C#" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="true" CodeBehind="insertMasterpageContent.aspx.cs" Inherits="umbraco.presentation.umbraco.dialogs.insertMasterpageContent" %>
<%@ Register TagPrefix="cc1" Namespace="Umbraco.Web._Legacy.Controls" %>

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

  <cc1:Pane ID="pane_insert" runat="server">
    <cc1:PropertyPanel runat="server">
            <p>
                <%= Services.TextService.Localize("defaultdialogs/templateContentPlaceHolderHelp")%>
            </p>
    </cc1:PropertyPanel>
    <cc1:PropertyPanel runat="server" id="pp_placeholder" text="Placeholder ID">
             <asp:DropDownList ID="dd_detectedAlias" Width="350px" CssClass="bigInput input-block-level" runat="server" />
    </cc1:PropertyPanel>
  </cc1:Pane>

  <cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
            <a class="btn btn-link" onclick="UmbClientMgr.closeModalWindow(); return false;"><%=Services.TextService.Localize("general/cancel")%></a>
            <input type="button" onclick="insertCode(); return false;" Class="btn btn-primary" value="<%= Services.TextService.Localize("insert") %>" />
  </cc1:Pane>
</asp:Content>