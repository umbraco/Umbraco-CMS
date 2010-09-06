<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="loadStarterKitDesigns.ascx.cs" Inherits="umbraco.presentation.install.steps.Skinning.loadStarterKitDesigns" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>

<asp:Panel id="pl_loadStarterKitDesigns" runat="server">



<asp:Repeater ID="rep_starterKitDesigns" runat="server">
    <HeaderTemplate>
        <ul>
    </HeaderTemplate>
    <ItemTemplate>
        <li>

        <asp:LinkButton ID="bt_selectKit" runat="server" onclick="SelectStarterKitDesign" CommandArgument="<%# ((Skin)Container.DataItem).RepoGuid %>">
         <%# ((Skin)Container.DataItem).Text %>
        </asp:LinkButton>

         <br />

        <img src="<%# ((Skin)Container.DataItem).Thumbnail %>" />
        
        </li>
    </ItemTemplate>
    <FooterTemplate>
        </ul>
    </FooterTemplate>
</asp:Repeater>


</asp:Panel>