<%@ Page Language="C#" MasterPageFile="../../masterpages/umbracoPage.Master" AutoEventWireup="true"
    CodeBehind="editScript.aspx.cs" Inherits="umbraco.cms.presentation.settings.scripts.editScript"
    ValidateRequest="False" %>
<%@ Import Namespace="Umbraco.Core" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ID="DocTypeContent" ContentPlaceHolderID="DocType" runat="server">
    <!DOCTYPE html>
</asp:Content>

<asp:Content ContentPlaceHolderID="head" runat="server">
    
    <umb:JsInclude ID="JsInclude1" runat="server" FilePath="Editors/EditScript.js" PathNameAlias="UmbracoClient" />

    <script language="javascript" type="text/javascript">

        (function ($) {
            $(document).ready(function () {
                var editor = new Umbraco.Editors.EditScript({
                    nameTxtBox: $('#<%= NameTxt.ClientID %>'),
                    originalFileName: '<%= NameTxt.Text %>',
                    saveButton: $("#<%= ((Control)SaveButton).ClientID %>"),
                    restServiceLocation: "<%= Url.GetSaveFileServicePath() %>",
                    editorSourceElement: $('#<%= editorSource.ClientID %>'),
                    treeSyncPath: "<%= ScriptTreeSyncPath %>",
                    lttPathElement: $('#<%= lttPath.ClientID %>')
                });
                editor.init();
                
                //bind save shortcut
                UmbClientMgr.appActions().bindSaveShortCut();
            });
        })(jQuery);
        
    </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc1:TabView ID="Panel1" runat="server" hasMenu="true">
         <cc1:Pane ID="Pane7" runat="server" >
            <cc1:PropertyPanel ID="pp_source" runat="server">
                <cc1:CodeArea ID="editorSource" CodeBase="JavaScript" ClientSaveMethod="doSubmit"
                    runat="server" AutoResize="false" />
            </cc1:PropertyPanel>
         </cc1:Pane>

        <cc1:Pane ID="Pane8" runat="server" >
            <cc1:PropertyPanel runat="server" ID="pp_name">
                <asp:TextBox ID="NameTxt" runat="server"></asp:TextBox>
            </cc1:PropertyPanel>
            <cc1:PropertyPanel runat="server" ID="pp_path">
                <asp:Literal ID="lttPath" runat="server" />
            </cc1:PropertyPanel>
            
        </cc1:Pane>
    </cc1:TabView>
    
</asp:Content>
