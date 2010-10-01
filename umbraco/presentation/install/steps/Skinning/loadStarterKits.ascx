<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="loadStarterKits.ascx.cs" Inherits="umbraco.presentation.install.steps.Skinning.loadStarterKits" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>

<asp:Panel id="pl_loadStarterKits" runat="server">

<script type="text/javascript">

jQuery(document).ready(function () {
    jQuery('.selectStarterKit').click(function () {
        jQuery('#starterkits').hide();
        jQuery('#installingStarterKit').show();
    });

});

</script>

<div id="installingStarterKit" style="display:none;">
<p>Installing selected starter kit...</p>
</div>

<asp:Repeater ID="rep_starterKits" runat="server">
    <HeaderTemplate>
        <ul id="starterkits">
    </HeaderTemplate>
    <ItemTemplate>
        <li>
        <asp:LinkButton CssClass="selectStarterKit" ID="bt_selectKit" runat="server" onclick="SelectStarterKit" CommandArgument="<%# ((Package)Container.DataItem).RepoGuid %>">
        
        <img src="<%# ((Package)Container.DataItem).Thumbnail %>" alt="<%# ((Package)Container.DataItem).Text %>" />
        
        <span><%# ((Package)Container.DataItem).Text %></span>

        </asp:LinkButton>

        <div><%# ((Package)Container.DataItem).Description %></div>
        
        </li>
    </ItemTemplate>
    <FooterTemplate>
        </ul>

        <div id="starterKitDesc">Click a starterkit icon above to install it</div>

        <asp:LinkButton runat="server" CssClass="declineStarterKits" ID="declineStarterKits" OnClientClick="return confirm('Are you sure you do not want to install a starter kit?');" OnClick="NextStep">I prefer not to install a starter kit</asp:LinkButton>

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