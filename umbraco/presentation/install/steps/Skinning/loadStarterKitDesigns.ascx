<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="loadStarterKitDesigns.ascx.cs" Inherits="umbraco.presentation.install.steps.Skinning.loadStarterKitDesigns" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>

<asp:Panel id="pl_loadStarterKitDesigns" runat="server">

<script type="text/javascript">

    jQuery(document).ready(function () {
        jQuery('.selectSkin').click(function () {
            jQuery('#starterKitDesigns').hide();
            jQuery('#installingSkin').show();
        });

    });

</script>


<div id="installingSkin" style="display:none;">
<p>Installing selected skin...</p>
</div>

<asp:Repeater ID="rep_starterKitDesigns" runat="server">
    <HeaderTemplate>
        <ul id="starterKitDesigns">
    </HeaderTemplate>
    <ItemTemplate>
        <li>

        <asp:LinkButton CssClass="selectSkin" ID="bt_selectKit" runat="server" onclick="SelectStarterKitDesign" CommandArgument="<%# ((Skin)Container.DataItem).RepoGuid %>">

         <img src="<%# ((Skin)Container.DataItem).Thumbnail %>" alt="<%# ((Skin)Container.DataItem).Text %>" />
        
        <span><%# ((Skin)Container.DataItem).Text %></span>

       
        </asp:LinkButton>

         <div><%# ((Skin)Container.DataItem).Description %></div>
        
        </li>
    </ItemTemplate>
    <FooterTemplate>


        </ul>

         <div id="starterKitDesignDesc">Click a skin icon above to install it</div>

          <asp:LinkButton runat="server" CssClass="declineStarterKits" ID="declineStarterKitDesigns" OnClientClick="return confirm('Are you sure you do not want to install a skin?');" OnClick="NextStep">I prefer not to install a skin</asp:LinkButton>
      
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