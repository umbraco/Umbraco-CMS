<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditUserType.aspx.cs" MasterPageFile="../masterpages/umbracoPage.Master"
    Inherits="umbraco.cms.presentation.user.EditUserType" %>

<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc2:UmbracoPanel ID="pnlUmbraco" runat="server" hasMenu="true" Width="608px">
        <cc2:Pane ID="pnl1" Style="padding: 10px; text-align: left;" runat="server">
            <asp:HiddenField runat="server" ID="hidUserTypeID" />
            <cc2:PropertyPanel ID="pp_name" runat="server">
                <asp:TextBox runat="server" ID="txtUserTypeName" MaxLength="30"></asp:TextBox>
            </cc2:PropertyPanel>
            <cc2:PropertyPanel ID="pp_alias" runat="server">
                <asp:Label runat="server" ID="lblUserTypeAlias"></asp:Label>
            </cc2:PropertyPanel>
        </cc2:Pane>
        <cc2:Pane ID="pnl2" Style="padding: 10px; text-align: left;" runat="server">
            <cc2:PropertyPanel ID="pp_rights" runat="server">
                <asp:CheckBoxList ID="cbl_rights" runat="server" />
            </cc2:PropertyPanel>
        </cc2:Pane>
    </cc2:UmbracoPanel>
    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>
</asp:Content>
