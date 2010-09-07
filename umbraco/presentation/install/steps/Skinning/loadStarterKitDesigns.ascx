<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="loadStarterKitDesigns.ascx.cs" Inherits="umbraco.presentation.install.steps.Skinning.loadStarterKitDesigns" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>

<asp:Panel id="pl_loadStarterKitDesigns" runat="server">



<asp:Repeater ID="rep_starterKitDesigns" runat="server">
    <HeaderTemplate>
        <ul id="starterKitDesigns">
    </HeaderTemplate>
    <ItemTemplate>
        <li>

        <asp:LinkButton ID="bt_selectKit" runat="server" onclick="SelectStarterKitDesign" CommandArgument="<%# ((Skin)Container.DataItem).RepoGuid %>">

         <img src="<%# ((Skin)Container.DataItem).Thumbnail %>" alt="<%# ((Skin)Container.DataItem).Text %>" />
        
        <span><%# ((Skin)Container.DataItem).Text %></span>

       
        </asp:LinkButton>

         <div><%# ((Skin)Container.DataItem).Description %></div>
        
        </li>
    </ItemTemplate>
    <FooterTemplate>


        </ul>

         <div id="starterKitDesignDesc">Click a skin icon above to install it</div>

      
    </FooterTemplate>
</asp:Repeater>

<script type="text/javascript">

    jQuery(document).ready(function () {
        jQuery("#starterKitDesigns li").mouseover(function () {
            jQuery("#starterKitDesignDesc").html(jQuery("div", this).html());
        });
    });

</script>


</asp:Panel>