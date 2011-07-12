<%@ Page Language="c#" CodeBehind="EditNodeTypeNew.aspx.cs" AutoEventWireup="True"
    Trace="false" Inherits="umbraco.settings.EditContentTypeNew" MasterPageFile="../masterpages/umbracoPage.Master" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="uc1" TagName="ContentTypeControlNew" Src="../controls/ContentTypeControlNew.ascx" %>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <uc1:ContentTypeControlNew ID="ContentTypeControlNew1" runat="server"></uc1:ContentTypeControlNew>
    <cc1:Pane ID="tmpPane" runat="server">
        <cc1:PropertyPanel Text="Allowed templates" runat="server">
            <div class="guiInputStandardSize" style="border: #ccc 1px solid; background: #fff;
                overflow: auto; height: 170px;">
                <asp:CheckBoxList ID="templateList" runat="server" />
            </div>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel Text="Default template" runat="server">
            <asp:DropDownList ID="ddlTemplates" CssClass="guiInputText guiInputStandardSize"
                runat="server" />
        </cc1:PropertyPanel>
    </cc1:Pane>
    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>
</asp:Content>
