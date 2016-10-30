<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditUserGroup.aspx.cs" MasterPageFile="../masterpages/umbracoPage.Master"
    Inherits="umbraco.cms.presentation.user.EditUserGroup" %>

<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc2:UmbracoPanel ID="pnlUmbraco" runat="server" hasMenu="true" Width="608px">
        <cc2:Pane ID="pnl1" Style="padding: 10px; text-align: left;" runat="server">
            <asp:HiddenField runat="server" ID="hidUserGroupID" />
            <cc2:PropertyPanel ID="pp_name" runat="server">
                <asp:TextBox runat="server" ID="txtUserGroupName" MaxLength="30"></asp:TextBox>
            </cc2:PropertyPanel>
            <cc2:PropertyPanel ID="pp_alias" runat="server">
                <asp:Label runat="server" ID="lblUserGroupAlias"></asp:Label>
            </cc2:PropertyPanel>
        </cc2:Pane>
        <cc2:Pane ID="pnl2" Style="padding: 10px; text-align: left;" runat="server">
            <cc2:PropertyPanel ID="pp_rights" runat="server">
                <asp:CheckBoxList ID="cbl_rights" runat="server" />
            </cc2:PropertyPanel>
        </cc2:Pane>
        <cc2:Pane ID="pnl3" Style="padding: 10px; text-align: left;" runat="server">
            <cc2:PropertyPanel ID="pp_sections" runat="server">
                <asp:CheckBoxList ID="cbl_sections" runat="server" />
            </cc2:PropertyPanel>
        </cc2:Pane>
        <cc2:Pane ID="pnl4" Style="padding: 10px; text-align: left;" runat="server">
            <cc2:PropertyPanel ID="pp_users" runat="server">
                <asp:UpdatePanel ID="pnlUsers" class="group-selector" runat="server">
                    <ContentTemplate>
                        <div class="group-selector-list">
                            <div>Users not in group:</div>
                            <asp:ListBox id="lstUsersNotInGroup" runat="server" SelectionMode="Multiple" />
                        </div>
                        <div class="group-selector-buttons">
                            <asp:Button id="btnAddToGroup" runat="server" Text="Add" OnClick="btnAddToGroup_Click" />
                            <asp:Button id="btnRemoveFromGroup" runat="server" Text="Remove" OnClick="btnRemoveFromGroup_Click" />
                        </div>
                        <div class="group-selector-list">
                            <div>Users in group:</div>
                            <asp:ListBox id="lstUsersInGroup" runat="server" SelectionMode="Multiple" />
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </cc2:PropertyPanel>
        </cc2:Pane>
    </cc2:UmbracoPanel>
    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>
</asp:Content>