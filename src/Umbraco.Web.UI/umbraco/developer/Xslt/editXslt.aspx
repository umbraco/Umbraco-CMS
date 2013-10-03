<%@ Page Title="Edit XSLT File" MasterPageFile="../../masterpages/umbracoPage.Master"
    ValidateRequest="false" Language="c#" CodeBehind="editXslt.aspx.cs" AutoEventWireup="True"
    Inherits="umbraco.cms.presentation.developer.editXslt" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ID="DocTypeContent" ContentPlaceHolderID="DocType" runat="server">
    <!DOCTYPE html>
</asp:Content>

<asp:Content ContentPlaceHolderID="head" runat="server" ID="cp2">
    
    <umb:JsInclude ID="JsInclude2" runat="server" FilePath="Editors/EditXslt.js" PathNameAlias="UmbracoClient" />
    <umb:CssInclude ID="CssInclude1" runat="server" FilePath="Editors/EditXslt.css" PathNameAlias="UmbracoClient" />

    <script type="text/javascript">

        (function ($) {
            $(document).ready(function () {
                var editor = new Umbraco.Editors.EditXslt({
                    nameTxtBox: $('#<%= xsltFileName.ClientID %>'),
                    originalFileName: '<%= xsltFileName.Text %>',
                    saveButton: $("#<%= ((Control)SaveButton).ClientID %>"),
                    editorSourceElement: $('#<%= editorSource.ClientID %>'),
                    skipTestingCheckBox: $("#<%= SkipTesting.ClientID %>"),
                });
                editor.init();

                //bind save shortcut
                UmbClientMgr.appActions().bindSaveShortCut();
            });
        })(jQuery);

        //TODO: Move these to EditXslt.js one day
        var xsltSnippet = "";        
        function xsltVisualize() {

            xsltSnippet = UmbEditor.IsSimpleEditor
                ? jQuery("#<%= editorSource.ClientID %>").getSelection().text
                    : UmbEditor._editor.getSelection();

            if (xsltSnippet == '') {
                xsltSnippet = UmbEditor.IsSimpleEditor
                ? jQuery("#<%= editorSource.ClientID %>").val()
                    : UmbEditor.GetCode();
            }

            UmbClientMgr.openModalWindow('<%= Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco) %>/developer/xslt/xsltVisualize.aspx', 'Visualize XSLT', true, 550, 650);
        } 
    </script>
    <umb:JsInclude ID="JsInclude1" runat="server" FilePath="Application/jQuery/jquery-fieldselection.js"
        PathNameAlias="UmbracoClient" />
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server" ID="cp1">
    <cc1:TabView ID="UmbracoPanel1" runat="server" Text="Edit xsl" hasMenu="true">
            <cc1:Pane runat="server" ID="pane1">
                <cc1:CodeArea ID="editorSource" CodeBase="XML" runat="server" AutoResize="true" OffSetX="47" OffSetY="55" />
            </cc1:Pane>
            
            <cc1:Pane runat="server" ID="pane2">
                 <cc1:PropertyPanel ID="pp_filename" runat="server" Text="Filename">
                    <asp:TextBox ID="xsltFileName" runat="server" Width="300" CssClass="guiInputText"></asp:TextBox>
                </cc1:PropertyPanel>
                <cc1:PropertyPanel ID="pp_testing" runat="server" Text="Skip testing (ignore errors)">
                    <asp:CheckBox ID="SkipTesting" runat="server"></asp:CheckBox>
                </cc1:PropertyPanel>
            </cc1:Pane>
    </cc1:TabView>
</asp:Content>
<asp:Content ContentPlaceHolderID="footer" runat="server">
    <asp:Literal ID="editorJs" runat="server"></asp:Literal>
</asp:Content>
