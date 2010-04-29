<%@ Page Language="C#" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="true"
    CodeBehind="search.aspx.cs" Inherits="umbraco.presentation.dialogs.search" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ID="header1" ContentPlaceHolderID="head" runat="server">
<script type="text/javascript">
    function openItem(id) {
        var url = "editContent.aspx?id=" + id;
        if (UmbClientMgr.mainWindow().UmbClientMgr.appActions().getCurrApp() == "media") {
            url = "editMedia.aspx?id=" + id;
        }
        UmbClientMgr.contentFrame(url);
        UmbClientMgr.closeModalWindow();
    }
</script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="body" runat="server">
    <cc1:Pane ID="Wizard" runat="server">
        <h3>Search</h3>
        <p>
        <asp:TextBox ID="keyword" runat="server" Width="500" CssClass="bigInput"></asp:TextBox> 
            <asp:Button ID="searchButton" runat="server" Text="Search" onclick="search_Click" /><br />
        </p>
        <asp:Xml ID="searchResult" runat="server" TransformSource="../xslt/searchResult.xslt"></asp:Xml>
    </cc1:Pane>
</asp:Content>
<asp:Content ID="footer1" ContentPlaceHolderID="footer" runat="server">
<script type="text/javascript">
    document.getElementById("ctl00_body_keyword").focus();
</script>
</asp:Content>
