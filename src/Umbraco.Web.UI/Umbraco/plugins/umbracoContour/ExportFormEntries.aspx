<%@ Page MasterPageFile="../../masterpages/umbracoPage.Master" Language="C#" AutoEventWireup="true" CodeBehind="ExportFormEntries.aspx.cs" Inherits="Umbraco.Forms.UI.Pages.ExportFormEntries" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Content ID="Content" ContentPlaceHolderID="body" runat="server">
<umb:UmbracoPanel ID="panel" runat="server" Text="Export data">
    
    <umb:Pane runat="server" ID="export">
    <umb:PropertyPanel ID="p_exportType" runat="server" Text="Export type">
        <asp:DropDownList ID="dd_exportTypes" AutoPostBack="true" runat="server" />
    </umb:PropertyPanel>
    </umb:Pane>

    <umb:Pane ID="paneAddSettings" runat="server" Visible="false">

        <umb:PropertyPanel ID="PropertyPanel2" runat="server" Text="Description">
           <em><asp:Literal ID="lt_export_description" runat="server" /></em>
        </umb:PropertyPanel>
    
        <umb:PropertyPanel ID="pp_maxItems" runat="server" Text="Max number of records">
            <asp:TextBox ID="tb_maxItems" Runat="server" />
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="PropertyPanel1" runat="server" Text="Sort By field">
        
            <asp:DropDownList ID="dd_SortByField" runat="server" />
            
            <asp:DropDownList ID="dd_SortDirection" runat="server">
                <asp:ListItem Text="Ascending" Value="asc" Selected="True" />
                <asp:ListItem Text="Descending" Value="desc" />
            </asp:DropDownList>
        </umb:PropertyPanel>   
        
        <asp:PlaceHolder ID="ph_Settings" runat="server" />
        
        <umb:PropertyPanel id="exportButtonPanel" runat="server">
            <asp:Button ID="bt_export" runat="server" Text="Export data" OnClick="Export" />
        </umb:PropertyPanel>
         
    </umb:Pane>


</umb:UmbracoPanel>
</asp:Content>