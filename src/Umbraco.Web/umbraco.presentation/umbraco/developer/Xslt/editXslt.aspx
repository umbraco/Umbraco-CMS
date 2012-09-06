<%@ Page Title="Edit XSLT File" MasterPageFile="../../masterpages/umbracoPage.Master"
    ValidateRequest="false" Language="c#" CodeBehind="editXslt.aspx.cs" AutoEventWireup="True"
    Inherits="umbraco.cms.presentation.developer.editXslt" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<asp:Content ContentPlaceHolderID="head" runat="server" ID="cp2">
    <style type="text/css">
        #errorDiv
        {
            margin-bottom: 10px;
        }
        #errorDiv a
        {
            float: right;
        }
        .propertyItemheader
        {
            width: 200px !important;
        }
    </style>
    <script type="text/javascript">

        var xsltSnippet = "";

        function closeErrorDiv() {
            jQuery('#errorDiv').hide();
        }

        function doSubmit() {
            closeErrorDiv();

            var codeVal = jQuery('#<%= editorSource.ClientID %>').val();
            //if CodeMirror is not defined, then the code editor is disabled.
            if (typeof (CodeMirror) != "undefined") {
                codeVal = UmbEditor.GetCode();
            }

            umbraco.presentation.webservices.codeEditorSave.SaveXslt(jQuery('#<%= xsltFileName.ClientID %>').val(), '<%= xsltFileName.Text %>', codeVal, document.getElementById('<%= SkipTesting.ClientID %>').checked, submitSucces, submitFailure);
        }

        function submitSucces(t) {
            if (t != 'true') {
                top.UmbSpeechBubble.ShowMessage('error', 'Saving Xslt file failed', '');
                jQuery('#errorDiv').html('<p><a href="#" onclick=\'closeErrorDiv()\'>Hide Errors</a><strong>Error occured</strong></p><p>' + t + '</p>');
                jQuery('#errorDiv').slideDown('fast');
            }
            else {
                top.UmbSpeechBubble.ShowMessage('save', 'Xslt file saved', '')
            }
        }
        function submitFailure(t) {
            top.UmbSpeechBubble.ShowMessage('warning', 'Xslt file could not be saved', '')
        }

        function xsltVisualize() {

            xsltSnippet = UmbEditor.IsSimpleEditor
                ? jQuery("#<%= editorSource.ClientID %>").getSelection().text
                    : UmbEditor._editor.selection();

            if (xsltSnippet == '') {
                xsltSnippet = UmbEditor.IsSimpleEditor
                ? jQuery("#<%= editorSource.ClientID %>").val()
                    : UmbEditor.GetCode();
                //                alert('Please select the xslt to visualize');
            }

            UmbClientMgr.openModalWindow('<%= umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) %>/developer/xslt/xsltVisualize.aspx', 'Visualize XSLT', true, 550, 650);

        }
		  
    </script>
    <umb:JsInclude ID="JsInclude1" runat="server" FilePath="Application/jQuery/jquery-fieldselection.js"
        PathNameAlias="UmbracoClient" />
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server" ID="cp1">
    <cc1:UmbracoPanel ID="UmbracoPanel1" runat="server" Text="Edit xsl" hasMenu="true"
        Height="300" Width="600">
        <cc1:Pane ID="Pane1" runat="server" Style="margin-bottom: 10px;">
            <cc1:PropertyPanel ID="pp_filename" runat="server" Text="Filename">
                <asp:TextBox ID="xsltFileName" runat="server" Width="300" CssClass="guiInputText"></asp:TextBox>
            </cc1:PropertyPanel>
            <cc1:PropertyPanel ID="pp_testing" runat="server" Text="Skip testing (ignore errors)">
                <asp:CheckBox ID="SkipTesting" runat="server"></asp:CheckBox>
            </cc1:PropertyPanel>
            <cc1:PropertyPanel ID="pp_errorMsg" runat="server">
                <div id="errorDiv" style="display: none;" class="error">
                    test</div>
            </cc1:PropertyPanel>
            <cc1:CodeArea ID="editorSource" CodeBase="XML" ClientSaveMethod="doSubmit" runat="server"
                AutoResize="true" OffSetX="47" OffSetY="55" />
        </cc1:Pane>
    </cc1:UmbracoPanel>
    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="footer" runat="server">
    <asp:Literal ID="editorJs" runat="server"></asp:Literal>
</asp:Content>
