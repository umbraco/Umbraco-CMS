<%@ Page ValidateRequest="false" Language="c#" MasterPageFile="../../masterpages/umbracoPage.Master"
    CodeBehind="editPython.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.developer.editPython" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ID="cp0" runat="server" ContentPlaceHolderID="head">
    <script type="text/javascript">
        function closeErrorDiv() {
            jQuery('#errorDiv').hide();
        }

        function doSubmit() {
            closeErrorDiv();

            var codeVal = jQuery('#<%= pythonSource.ClientID %>').val();
            
            //if CodeMirror is not defined, then the code editor is disabled.
            if (typeof (CodeMirror) != "undefined") {
                codeVal = UmbEditor.GetCode();
            }

            
            umbraco.presentation.webservices.codeEditorSave.SaveDLRScript(jQuery('#<%= pythonFileName.ClientID %>').val(), '<%= pythonFileName.Text %>', codeVal, document.getElementById('<%= SkipTesting.ClientID %>').checked, submitSucces, submitFailure);
        }

        function submitSucces(t) {
            if (t != 'true') {
                top.UmbSpeechBubble.ShowMessage('error', 'Saving scripting file failed', '');
                jQuery('#errorDiv').html('<p><a href="#" style="position: absolute; right: 10px; top: 10px;" onclick=\'closeErrorDiv()\'>Hide Errors</a><strong>Error occured</strong></p><p>' + t + '</p>');
                jQuery('#errorDiv').slideDown('fast');
            }
            else {
                top.UmbSpeechBubble.ShowMessage('save', 'Scripting file saved', '')
            }
        }

        function submitFailure(t) {
            top.UmbSpeechBubble.ShowMessage('warning', 'Scripting file could not be saved', '')
        }


        function showError() {
            var id = "#errorDiv";
            if (jQuery(id).is(":visible")) {
                jQuery(id).hide();
            }
        }
    </script>
</asp:Content>
<asp:Content ID="cp1" runat="server" ContentPlaceHolderID="body">
    <cc1:UmbracoPanel ID="UmbracoPanel1" runat="server" Text="Edit scripting file" Height="300"
        Width="600">
        <cc1:Pane ID="Pane1" runat="server" Style="margin-bottom: 10px;">
            <cc1:PropertyPanel ID="pp_filename" Text="Filename" runat="server">
                <asp:TextBox ID="pythonFileName" runat="server" Width="400" CssClass="guiInputText"></asp:TextBox>
            </cc1:PropertyPanel>
            <cc1:PropertyPanel ID="pp_testing" runat="server" Text="Skip testing (ignore errors)">
                <asp:CheckBox ID="SkipTesting" runat="server"></asp:CheckBox>
            </cc1:PropertyPanel>
            <cc1:PropertyPanel ID="pp_errorMsg" runat="server">
                <div id="errorDiv" style="position: relative; display: none;" class="error">
                    -</div>
            </cc1:PropertyPanel>
            <cc1:CodeArea ID="pythonSource" ClientSaveMethod="doSubmit" AutoSuggest="true"  CodeBase="Razor" AutoResize="true" OffSetX="47"
                OffSetY="55" runat="server" />
        </cc1:Pane>
    </cc1:UmbracoPanel>
    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>
</asp:Content>
