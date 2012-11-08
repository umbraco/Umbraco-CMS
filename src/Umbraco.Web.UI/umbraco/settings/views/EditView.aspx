<%@ Page Language="C#" MasterPageFile="../../masterpages/umbracoPage.Master" AutoEventWireup="True"
    CodeBehind="EditView.aspx.cs" Inherits="Umbraco.Web.UI.Umbraco.Settings.Views.EditView"
    ValidateRequest="False" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<asp:Content ContentPlaceHolderID="head" runat="server">
    
    <umb:JsInclude ID="JsInclude1" runat="server" FilePath="Editors/EditView.js" PathNameAlias="UmbracoClient" />

    <script language="javascript" type="text/javascript">
        
        (function ($) {
            $(document).ready(function () {
                //create a new EditView object
                var editView = new Umbraco.Editors.EditView({
                    masterPageDropDown: $("#<%= MasterTemplate.ClientID %>"),
                    nameTxtBox: $("#<%= NameTxt.ClientID %>"),
                    aliasTxtBox: $("#<%= AliasTxt.ClientID %>"),
                    saveButton: $("#<%= ((Control)SaveButton).ClientID %>"),
                    templateId: '<%= Request.QueryString["templateID"] %>',
                    msgs: {
                        templateErrorHeader: "<%= umbraco.ui.Text("speechBubbles", "templateErrorHeader") %>",
                        templateErrorText: "<%= umbraco.ui.Text("speechBubbles", "templateErrorText") %>",
                        templateSavedHeader: "<%= umbraco.ui.Text("speechBubbles", "templateSavedHeader") %>",
                        templateSavedText: "<%= umbraco.ui.Text("speechBubbles", "templateSavedText") %>"                        
                    }
                });
                //initialize it.
                editView.init();
                
                //bind save shortcut
                UmbClientMgr.appActions().bindSaveShortCut();
            });            
        })(jQuery);
       
    </script>

</asp:Content>


<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc1:UmbracoPanel ID="Panel1" runat="server" Width="608px" Height="336px" hasMenu="true">
        <cc1:Pane ID="Pane7" runat="server" Height="44px" Width="528px">
            
            <cc1:PropertyPanel ID="pp_name" runat="server">
                <asp:TextBox ID="NameTxt" Width="350px" runat="server"></asp:TextBox>
            </cc1:PropertyPanel>
            
            <cc1:PropertyPanel ID="pp_alias" runat="server">
                <asp:TextBox ID="AliasTxt" Width="350px" runat="server"></asp:TextBox>
            </cc1:PropertyPanel>

            <cc1:PropertyPanel ID="pp_masterTemplate" runat="server">
                <asp:DropDownList ID="MasterTemplate" Width="350px" runat="server" />
            </cc1:PropertyPanel>

            <cc1:PropertyPanel ID="pp_source" runat="server">
                <cc1:CodeArea ID="editorSource" runat="server" CodeBase="Razor" ClientSaveMethod="doSubmit" AutoResize="true" OffSetX="37" OffSetY="54"/>
            </cc1:PropertyPanel>

        </cc1:Pane>
    </cc1:UmbracoPanel>

</asp:Content>
