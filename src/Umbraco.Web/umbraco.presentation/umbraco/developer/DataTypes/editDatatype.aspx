<%@ Page Language="c#" MasterPageFile="../../masterpages/umbracoPage.Master" Title="Edit data type"
    CodeBehind="editDatatype.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.developer.editDatatype" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc1:UmbracoPanel ID="Panel1" runat="server" Width="496px" Height="584px">
        <cc1:Pane ID="pane_control" runat="server">
            <cc1:PropertyPanel ID="pp_name" runat="server">
                <asp:TextBox ID="txtName" runat="server"></asp:TextBox>
            </cc1:PropertyPanel>
            <cc1:PropertyPanel ID="pp_renderControl" runat="server">
                <asp:DropDownList ID="ddlRenderControl" runat="server" />
            </cc1:PropertyPanel>
            <cc1:PropertyPanel ID="pp_guid" runat="server">
                <asp:Literal ID="litGuid" runat="server" />
            </cc1:PropertyPanel>
        </cc1:Pane>
        <cc1:Pane ID="pane_settings" runat="server">
            <asp:PlaceHolder ID="plcEditorPrevalueControl" runat="server"></asp:PlaceHolder>
        </cc1:Pane>
    </cc1:UmbracoPanel>
    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>
</asp:Content>
