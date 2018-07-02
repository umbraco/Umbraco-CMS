<%@ Page Language="C#" AutoEventWireup="true" Codebehind="DictionaryItemList.aspx.cs"
  Inherits="umbraco.presentation.settings.DictionaryItemList" MasterPageFile="../masterpages/umbracoPage.Master" %>
<%@ Register TagPrefix="cc1" Namespace="Umbraco.Web._Legacy.Controls" %>


<asp:Content ContentPlaceHolderID="body" runat="server">

    <cc1:UmbracoPanel ID="Panel1" runat="server" Text="Dictionary overview">
    <cc1:Pane ID="pane1" runat="server">
      <table id="dictionaryItems" style="width: 100%;">
        <asp:Literal ID="lt_table" runat="server" />
      </table>
      </cc1:Pane>
    </cc1:UmbracoPanel>

</asp:Content>