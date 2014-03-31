<%@ Page Language="c#" CodeBehind="editLanguage.aspx.cs" AutoEventWireup="True" MasterPageFile="../masterpages/umbracoPage.Master"
    Inherits="umbraco.settings.editLanguage" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc1:UmbracoPanel ID="Panel1" runat="server" Width="608px" Height="336px" hasMenu="true">
        <cc1:Pane ID="Pane7" runat="server">
            <cc1:PropertyPanel runat="server" ID="pp_language">
                <asp:DropDownList ID="Cultures" runat="server">
                </asp:DropDownList>
            </cc1:PropertyPanel>
        </cc1:Pane>
    </cc1:UmbracoPanel>
    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>
</asp:Content>
