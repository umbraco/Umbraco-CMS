<%@ Page Language="C#" MasterPageFile="../../masterpages/umbracoPage.Master" AutoEventWireup="True"
    CodeBehind="EditView.aspx.cs" Inherits="Umbraco.Web.UI.Umbraco.Settings.Views.EditView"
    ValidateRequest="False" %>

<%@ OutputCache Location="None" %>

<%@ Import Namespace="Umbraco.Core" %>
<%@ Import Namespace="Umbraco.Core.IO" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ID="DocTypeContent" ContentPlaceHolderID="DocType" runat="server">
    <!DOCTYPE html>
</asp:Content>

<asp:Content ContentPlaceHolderID="head" runat="server">
    
    <umb:JsInclude ID="JsInclude1" runat="server" FilePath="Editors/EditView.js" PathNameAlias="UmbracoClient" />

    <script type="text/javascript">

        //we need to have this as a global object since we reference this object with callbacks.
        var editViewEditor;

        (function ($) {
            $(document).ready(function () {
                //create and assign a new EditView object
                editViewEditor = new Umbraco.Editors.EditView({
                    treeSyncPath: "<%=TemplateTreeSyncPath%>",
                    currentTreeType: "<%=CurrentTreeType%>",
                    editorType: "<%= EditorType.ToString() %>",
                    originalFileName: "<%=OriginalFileName %>",
                    restServiceLocation: "<%= Url.GetSaveFileServicePath() %>",                    
                    masterPageDropDown: $("#<%= MasterTemplate.ClientID %>"),
                    nameTxtBox: $("#<%= NameTxt.ClientID %>"),
                    aliasTxtBox: $("#<%= AliasTxt.ClientID %>"),
                    saveButton: $("#<%= ((Control)SaveButton).ClientID %>"),
                    templateId: '<%= Request.QueryString["templateID"] %>',
                    codeEditorElementId: '<%= editorSource.ClientID %>',
                    modalUrl: "<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>/dialogs/editMacro.aspx"
                });

                //initialize it.
                editViewEditor.init();
                
                //bind save shortcut
                UmbClientMgr.appActions().bindSaveShortCut();
            });            
        })(jQuery);
       
    </script>

</asp:Content>


<asp:Content ContentPlaceHolderID="body" runat="server">
    
    <cc1:TabView ID="Panel1" runat="server" hasMenu="true">
        
         <cc1:Pane ID="Pane8" runat="server" >
            <cc1:PropertyPanel ID="pp_source" runat="server">
                <cc1:CodeArea ID="editorSource" runat="server" CodeBase="Razor" ClientSaveMethod="doSubmit" AutoResize="false" />
            </cc1:PropertyPanel>
         </cc1:Pane>
            
        <cc1:Pane ID="Pane7" runat="server" >
            
            <cc1:PropertyPanel ID="pp_name" runat="server">
                <asp:Literal runat="server" ID="PathPrefix"></asp:Literal>
                <asp:TextBox ID="NameTxt" runat="server"></asp:TextBox>
            </cc1:PropertyPanel>
            
            <cc1:PropertyPanel ID="pp_alias" runat="server">
                <asp:TextBox ID="AliasTxt" runat="server"></asp:TextBox>
            </cc1:PropertyPanel>

            <cc1:PropertyPanel ID="pp_masterTemplate" runat="server">
                <asp:DropDownList ID="MasterTemplate" runat="server" />
            </cc1:PropertyPanel>

            
        </cc1:Pane>

        
    </cc1:TabView>

</asp:Content>
