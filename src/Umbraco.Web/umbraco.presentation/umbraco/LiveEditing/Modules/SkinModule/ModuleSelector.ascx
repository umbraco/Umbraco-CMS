<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ModuleSelector.ascx.cs" Inherits="umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule.ModuleSelector" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>

<script type="text/javascript">

    var umbCurrentPageId = <%= umbraco.presentation.nodeFactory.Node.GetCurrent().Id %>;
    var umbCurrentUmbracoDir = '<%= this.ResolveUrl(umbraco.GlobalSettings.Path) %>';


</script>
<div id="moduleSelectorContainer">

<asp:Repeater ID="rep_modules" runat="server" 
    onitemdatabound="rep_modules_ItemDataBound">
    <HeaderTemplate>
    <div id="modules">
    <p>Please select the module you wish to insert.</p>
        <ul>
    </HeaderTemplate>
    <ItemTemplate>
        <li>

        <asp:HyperLink ID="ModuleSelectLink" runat="server" NavigateUrl="javascript:void(0);">
            <img width="25px" src="<%# GetThumbNail(((Package)Container.DataItem).Thumbnail) %>" alt="<%# ((Package)Container.DataItem).Text %>" />
            <span><%# ((Package)Container.DataItem).Text %></span>
        
        </asp:HyperLink>

        </li>
    </ItemTemplate>
    <FooterTemplate>
        </ul>
        </div>
    </FooterTemplate>

   
</asp:Repeater>

<p id="noConnectionToRepo" runat="server" visible="false">
Unable to fetch module, please try again later.
</p>

 <p id="installingModule" style="display:none;">
    <span class="selectedModule"></span><br />
    <img src="<%= this.ResolveUrl(umbraco.GlobalSettings.Path) %>/LiveEditing/Modules/SkinModule/images/loader.gif" /> Installing module...
    
</p>

 <p id="moduleSelect" style="display:none;">
    <span class="selectedModule"></span><br />
    Select where to place the module
 </p>

 <a href="javascript:void(0);" onclick="jQuery('.ModuleSelector').hide();umbRemoveModuleContainerSelectors();">Cancel</a>
 </div>
