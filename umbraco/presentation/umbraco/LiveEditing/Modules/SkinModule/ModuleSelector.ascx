<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ModuleSelector.ascx.cs" Inherits="umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule.ModuleSelector" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>
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
            <img width="25px" src="http://our.umbraco.org/<%# ((Package)Container.DataItem).Thumbnail %>" alt="<%# ((Package)Container.DataItem).Text %>" />
            <span><%# ((Package)Container.DataItem).Text %></span>
        
        </asp:HyperLink>

        </li>
    </ItemTemplate>
    <FooterTemplate>
        </ul>
        </div>
    </FooterTemplate>

   
</asp:Repeater>

 <p id="installingModule" style="display:none;">
    <span class="selectedModule"></span><br />
    <img src="/umbraco/LiveEditing/Modules/SkinModule/images/loader.gif" /> Installing module...
    
</p>

 <p id="moduleSelect" style="display:none;">
    <span class="selectedModule"></span><br />
    Select where to place the module
 </p>

 <a href="javascript:void(0);" onclick="jQuery('.ModuleSelector').hide();umbRemoveModuleContainerSelectors();">Cancel</a>
 </div>
