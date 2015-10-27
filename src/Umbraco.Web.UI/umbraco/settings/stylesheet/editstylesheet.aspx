<%@ Page Language="c#" MasterPageFile="../../masterpages/umbracoPage.Master" CodeBehind="editstylesheet.aspx.cs" AutoEventWireup="True"
    Inherits="Umbraco.Web.UI.Umbraco.Settings.Stylesheet.EditStyleSheet" ValidateRequest="False" %>
<%@ Import Namespace="Umbraco.Core" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="cdf" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    <cdf:JsInclude runat="server" FilePath="Editors/EditStyleSheet.js" PathNameAlias="UmbracoClient"></cdf:JsInclude>

    <script type="text/javascript">
        
        (function ($) {
            $(document).ready(function () {
                var editor = new Umbraco.Editors.EditStyleSheet({
                    nameTxtBox: $('#<%= NameTxt.ClientID %>'),
                    originalFileName: '<%= NameTxt.Text %>',
                    saveButton: $("#<%= ((Control)SaveButton).ClientID %>"),
                    editorSourceElement: $('#<%= editorSource.ClientID %>'), 
                    restServiceLocation: "<%= Url.GetSaveFileServicePath() %>",
                    treeSyncPath: "<%= TreeSyncPath %>",
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
    <cc1:TabView ID="Panel1" runat="server" CssClass="panel" hasMenu="true">

        <cc1:Pane ID="Pane7" CssClass="pane" runat="server">
            <cc1:PropertyPanel ID="pp_source" runat="server">
                <cc1:CodeArea ID="editorSource" CodeBase="Css" AutoResize="false" runat="server" />
            </cc1:PropertyPanel>
        </cc1:Pane>

        <cc1:Pane ID="Pane8" CssClass="pane" runat="server">
            <cc1:PropertyPanel ID="pp_name" runat="server">
                <asp:TextBox ID="NameTxt" Width="350px" runat="server"></asp:TextBox>
            </cc1:PropertyPanel>
            <cc1:PropertyPanel ID="pp_path" runat="server">
                <asp:Literal ID="lttPath" runat="server"></asp:Literal>
            </cc1:PropertyPanel>
        </cc1:Pane>
    </cc1:TabView>
</asp:Content>
