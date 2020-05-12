<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master"Codebehind="ChangeDocType.aspx.cs" AutoEventWireup="True" Inherits="Umbraco.Web.UI.Umbraco.Dialogs.ChangeDocType" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ContentPlaceHolderID="head" runat="server">

  <style type="text/css">
    #propertyMapping thead tr th{border-bottom:1px solid #ccc; padding: 4px; padding-right: 25px;
                                background-image: url(<%= Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.UmbracoClient) %>/tableSorting/img/bg.gif);     
                                cursor: pointer; 
                                font-weight: bold; 
                                background-repeat: no-repeat; 
                                background-position: center right; 
                               }
    
    #propertyMapping tbody tr td{border-bottom:1px solid #efefef}
    #propertyMapping td{padding: 4px; ;}  
    body.umbracoDialog { overflow: auto; }
    .umb-dialog .umb-control-group .umb-el-wrap { overflow: hidden; }
    .umb-dialog .umb-control-group .umb-el-wrap label { float: left; width: 140px; font-weight: bold; }
    .umb-dialog .umb-control-group .umb-el-wrap label:after { content:":"; }
    .umb-dialog .umb-control-group .umb-el-wrap .controls-row { float: left; width: 280px; padding-bottom: 8px; }
    .umb-dialog .umb-control-group .umb-el-wrap .controls-row select { width: auto; }
    </style>

</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">

    <cc1:Pane runat="server" ID="ChangeDocTypePane">
        <p class="help">
            <%= umbraco.ui.Text("changeDocType", "changeDocTypeInstruction") %>            
        </p>

        <cc1:PropertyPanel ID="ContentNamePropertyPanel" runat="server">
            <asp:Label ID="ContentNameLabel" runat="server" />
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="CurrentTypePropertyPanel" runat="server">
            <asp:Label ID="CurrentTypeLabel" runat="server" />
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="NewTypePropertyPanel" runat="server">
            <asp:DropDownList ID="NewDocumentTypeList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="NewDocumentTypeList_SelectedIndexChanged" />
            <asp:RequiredFieldValidator ControlToValidate="NewDocumentTypeList" ErrorMessage="*" ID="NewDocumentTypeValidator" runat="server" Display="Dynamic" />          
            <br /><small><%=umbraco.ui.Text("changeDocType", "validDocTypesNote") %></small>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="NewTemplatePropertyPanel" runat="server">
            <asp:DropDownList ID="NewTemplateList" runat="server" />
        </cc1:PropertyPanel>

        <asp:PlaceHolder ID="NotAvailablePlaceholder" runat="server" Visible="false">        
            <div class="propertyItem notice" style="padding-top: 10px">     
                <p><%=umbraco.ui.Text("changeDocType", "docTypeCannotBeChanged") %></p>
            </div>
        </asp:PlaceHolder>
    </cc1:Pane>

    <cc1:Pane runat="server" ID="ChangeDocTypePropertyMappingPane">

        <p class="help">
            <%= umbraco.ui.Text("changeDocType", "changeDocTypeInstruction2") %>            
        </p>

        <asp:Repeater ID="PropertyMappingRepeater" runat="server">
            <HeaderTemplate>
                <table id="propertyMapping">
                    <thead>
                        <tr>
                            <th><%= umbraco.ui.Text("changeDocType", "currentProperty") %></th>
                            <th><%= umbraco.ui.Text("changeDocType", "mapToProperty") %></th>
                        </tr>
                    </thead>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td>
                        <%# DataBinder.Eval(Container, "DataItem.Name") %>
                        <asp:HiddenField ID="Alias" runat="server" Value='<%#DataBinder.Eval(Container, "DataItem.Alias")%>' />
                        <asp:HiddenField ID="Name" runat="server" Value='<%#DataBinder.Eval(Container, "DataItem.Name")%>' />
                        <asp:HiddenField ID="PropertyEditorAlias" runat="server" Value='<%#DataBinder.Eval(Container, "DataItem.PropertyEditorAlias")%>' />
                    </td>
                    <td><asp:DropDownList id="DestinationProperty" runat="server" /></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </cc1:Pane>

    <asp:PlaceHolder ID="SuccessPlaceholder" runat="server" Visible="false">
        <p><%=umbraco.ui.Text("changeDocType", "docTypeChanged") %></p>
        <p>        
            <asp:Literal ID="SuccessMessage" runat="server" />
            <asp:Literal ID="PropertiesMappedMessage" runat="server" />
            <asp:Literal ID="ContentPublishedMessage" runat="server" />
            <br /><br />
            <a href="#" style="color: blue" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("defaultdialogs", "closeThisWindow") %></a>  
        </p>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="ValidationPlaceholder" runat="server" Visible="false">
        <br />
        <div class="notice" style="padding: 10px">        
            <asp:Literal id="ValidationError" runat="server" />
        </div>
    </asp:PlaceHolder>

    <div class="btn-toolbar umb-btn-toolbar">
        <asp:PlaceHolder ID="SaveAndCancelPlaceholder" runat="server">
            <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
            <asp:PlaceHolder ID="SavePlaceholder" runat="server">        
                <asp:Button ID="ValidateAndSave" runat="server" CssClass="btn btn-primary" Text="Create" OnClick="ValidateAndSave_Click"></asp:Button>
            </asp:PlaceHolder>
        </asp:Placeholder>
    </div>
     
</asp:Content>
