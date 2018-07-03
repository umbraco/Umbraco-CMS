<%@ Register TagPrefix="cc1" Namespace="Umbraco.Web._Legacy.Controls" Assembly="Umbraco.Web" %>

<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoPage.Master" ValidateRequest="false"
    CodeBehind="EditDictionaryItem.aspx.cs" AutoEventWireup="True" Inherits="umbraco.settings.EditDictionaryItem" %>

<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc1:UmbracoPanel ID="Panel1" runat="server" Width="408px" Height="264px">
    </cc1:UmbracoPanel>
    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>
</asp:Content>
