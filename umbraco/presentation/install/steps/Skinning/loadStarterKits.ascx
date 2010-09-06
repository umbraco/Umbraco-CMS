<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="loadStarterKits.ascx.cs" Inherits="umbraco.presentation.install.steps.Skinning.loadStarterKits" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>

<asp:Panel id="pl_loadStarterKits" runat="server">


<asp:Repeater ID="rep_starterKits" runat="server">
    <HeaderTemplate>
        <ul>
    </HeaderTemplate>
    <ItemTemplate>
        <li>

        <asp:LinkButton ID="bt_selectKit" runat="server" onclick="SelectStarterKit" CommandArgument="<%# ((Package)Container.DataItem).RepoGuid %>">
        <%# ((Package)Container.DataItem).Text %>
        </asp:LinkButton>

        </li>
    </ItemTemplate>
    <FooterTemplate>
        </ul>
    </FooterTemplate>
</asp:Repeater>
    


</asp:Panel>