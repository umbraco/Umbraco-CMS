<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="loadStarterKits.ascx.cs" Inherits="umbraco.presentation.install.steps.Skinning.loadStarterKits" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>

<asp:Panel id="pl_loadStarterKits" runat="server">


<asp:Repeater ID="rep_starterKits" runat="server">
    <HeaderTemplate>
        <ul id="starterkits">
    </HeaderTemplate>
    <ItemTemplate>
        <li>
        <asp:LinkButton ID="bt_selectKit" runat="server" onclick="SelectStarterKit" CommandArgument="<%# ((Package)Container.DataItem).RepoGuid %>">
        
        <img src="<%# ((Package)Container.DataItem).Thumbnail %>" alt="<%# ((Package)Container.DataItem).Text %>" />
        
        <span><%# ((Package)Container.DataItem).Text %></span>

        </asp:LinkButton>

        <div><%# ((Package)Container.DataItem).Description %></div>
        
        </li>
    </ItemTemplate>
    <FooterTemplate>
        </ul>

        <div id="starterKitDesc">Click a starterkit icon above to install it</div>

        <a href="#" id="declineStarterKits">...I prefer <strong>not</strong> to install a starter kit</a>

    </FooterTemplate>
</asp:Repeater>
    

<script type="text/javascript">

    jQuery(document).ready(function () {
        jQuery("#starterKits li").mouseover(function () {
                jQuery("#starterKitDesc").html(jQuery("div", this).html());
        });
    });

</script>


</asp:Panel>