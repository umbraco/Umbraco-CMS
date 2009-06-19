<%@ Page language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" Codebehind="modalHolder.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.modalHolder" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
<style type="text/css">
body{
border: 0px;
height: 100%;
margin: 0px;
padding: 0px;
}
</style>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
  <iframe frameborder="1" src="<%=umbraco.helper.Request("url")%>?<%=umbraco.helper.Request("params").Replace("|", "&")%>" scrolling="auto" width="100%" height="100%"></iframe> 
</asp:Content>