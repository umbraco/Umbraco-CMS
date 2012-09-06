<%@ Page Language="c#" MasterPageFile="../../masterpages/umbracoPage.Master" Codebehind="editstylesheet.aspx.cs" AutoEventWireup="True"
  Inherits="umbraco.cms.presentation.settings.stylesheet.editstylesheet" ValidateRequest="False" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
  <script type="text/javascript">
	    
        function doSubmit() {
            var codeVal = jQuery('#<%= editorSource.ClientID %>').val();
            //if CodeMirror is not defined, then the code editor is disabled.
            if (typeof(CodeMirror) != "undefined") {
                codeVal = UmbEditor.GetCode();
            }
          umbraco.presentation.webservices.codeEditorSave.SaveCss(jQuery('#<%= NameTxt.ClientID %>').val(), '<%= NameTxt.Text %>', codeVal, '<%= Request.QueryString["id"] %>', submitSucces, submitFailure);
        }

        function submitSucces(t) {
          if (t != 'true') {
            top.UmbSpeechBubble.ShowMessage('error', '<%= umbraco.ui.Text("speechBubbles", "CssErrorHeader") %>', 'Please make sure that you have permissions set correctly');
          }
          else {
            top.UmbSpeechBubble.ShowMessage('save', '<%= umbraco.ui.Text("speechBubbles", "cssSavedHeader") %>', '<%= umbraco.ui.Text("speechBubbles", "cssSavedText") %>')
          }
        }
        function submitFailure(t) {
          top.UmbSpeechBubble.ShowMessage('error', '<%= umbraco.ui.Text("speechBubbles", "CssErrorHeader") %>', '<%= umbraco.ui.Text("speechBubbles", "CssErrorText") %>')
        }
        
  </script>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc1:UmbracoPanel ID="Panel1" runat="server" CssClass="panel" hasMenu="true">
      <cc1:Pane ID="Pane7" CssClass="pane" runat="server">
      <cc1:PropertyPanel ID="pp_name" runat="server">
        <asp:TextBox ID="NameTxt" Width="350px" runat="server"></asp:TextBox>
      </cc1:PropertyPanel>
      <cc1:PropertyPanel ID="pp_path" runat="server">
        <asp:Literal ID="lttPath" runat="server"></asp:Literal>
      </cc1:PropertyPanel>
      <cc1:PropertyPanel id="pp_source" runat="server">
         <cc1:CodeArea id="editorSource" CodeBase="Css" ClientSaveMethod="doSubmit" OffSetX="37" OffSetY="54" AutoResize="true" runat="server" />
      </cc1:PropertyPanel>
     </cc1:Pane>
    </cc1:UmbracoPanel>
        <script type="text/javascript">
            jQuery(document).ready(function () {
                UmbClientMgr.appActions().bindSaveShortCut();
            });
    </script>

</asp:Content>