<%@ Page Language="C#" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="true" CodeBehind="TemplateSkinning.aspx.cs" Inherits="umbraco.presentation.umbraco.dialogs.TemplateSkinning" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="body" runat="server">
    
    <cc1:Pane ID="p_apply" runat="server" Visible="false">
        
        <cc1:PropertyPanel ID="PropertyPanel1" runat="server" Text="Select a skin">
            <asp:DropDownList ID="dd_skins" runat="server" /> <asp:LinkButton OnClick="openRepo" runat="server" ID="bt_repo">Download more skins</asp:LinkButton>
        </cc1:PropertyPanel>
        
        <cc1:PropertyPanel ID="PropertyPanel2"  runat="server" Text=" "> 
                
        <br />
        
        <asp:Button ID="Button1" runat="server" Text="Apply" OnClick="apply" /> 
        
        <asp:PlaceHolder ID="ph_rollback" runat="server" Visible="false">
         <em>or</em> <asp:LinkButton ID="lb_rollback" OnClick="rollback" runat="server">Rollback current skin</asp:LinkButton>
        </asp:PlaceHolder>
        
        </cc1:PropertyPanel>  
    </cc1:Pane>

    <cc1:Pane ID="p_download" runat="server" Visible="false">
     
     <div id="skins">
        <asp:Repeater ID="rep_starterKitDesigns" runat="server" 
             onitemdatabound="rep_starterKitDesigns_ItemDataBound" >
            <HeaderTemplate>
                <ul id="starterKitDesigns">
            </HeaderTemplate>
            <ItemTemplate>
                <li>
                  
                   <img src="<%# ((Skin)Container.DataItem).Thumbnail %>" alt="<%# ((Skin)Container.DataItem).Text %>" />
        
                   <span><%# ((Skin)Container.DataItem).Text %></span>

                   <br />
                       
                    <asp:Button ID="Button1" runat="server" Text="Download and apply" CommandArgument="<%# ((Skin)Container.DataItem).RepoGuid %>" OnClick="SelectStarterKitDesign" CommandName="<%# ((Skin)Container.DataItem).Text %>"/>
                </li>
            </ItemTemplate>
            
            <FooterTemplate>
                </ul>
            </FooterTemplate>
        </asp:Repeater>

    </div>

    </cc1:Pane>
    
</asp:Content>
