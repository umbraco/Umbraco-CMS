<%@ Page Language="C#" MasterPageFile="../../masterpages/umbracoPage.Master" AutoEventWireup="true"
    CodeBehind="editView.aspx.cs" Inherits="umbraco.cms.presentation.settings.views.EditView"
    ValidateRequest="False" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<asp:Content ContentPlaceHolderID="head" runat="server">
    
    <script language="javascript" type="text/javascript">
        
        function doSubmit() {
            var codeVal = UmbEditor.GetCode();
            umbraco.presentation.webservices.codeEditorSave.SaveTemplate(jQuery('#<%= NameTxt.ClientID %>').val(), jQuery('#<%= AliasTxt.ClientID %>').val(), codeVal, '<%= Request.QueryString["templateID"] %>', jQuery('#<%= MasterTemplate.ClientID %>').val(), submitSucces, submitFailure);
        }
        
        function submitSucces(t)
        {
            if(t != 'true')
            {
                top.UmbSpeechBubble.ShowMessage('error', '<%= umbraco.ui.Text("speechBubbles", "templateErrorHeader") %>', '<%= umbraco.ui.Text("speechBubbles", "templateErrorText") %>');
            }
            else
            {
                top.UmbSpeechBubble.ShowMessage('save', '<%= umbraco.ui.Text("speechBubbles", "templateSavedHeader") %>', '<%= umbraco.ui.Text("speechBubbles", "templateSavedText") %>')
            }
        }
        
        function submitFailure(t)
        {
            top.UmbSpeechBubble.ShowMessage('error', '<%= umbraco.ui.Text("speechBubbles", "templateErrorHeader") %>', '<%= umbraco.ui.Text("speechBubbles", "templateErrorText") %>')
        }

        function changeMasterPageFile(){
          var editor = document.getElementById("<%= editorSource.ClientID %>");
          var templateDropDown = document.getElementById("<%= MasterTemplate.ClientID %>");
          var templateCode = UmbEditor.GetCode();
          var newValue = templateDropDown.options[templateDropDown.selectedIndex].id;
          
          var layoutDefRegex = new RegExp("(@{[\\s\\S]*?Layout\\s*?=\\s*?\")[^\"]*?(\";[\\s\\S]*?})", "gi");
          if(newValue != undefined && newValue != "") {
            if (layoutDefRegex.test(templateCode)) {
                        // Declaration exists, so just update it
                        templateCode = templateCode.replace(layoutDefRegex, "$1" + newValue + "$2");
                } else {
                    // Declaration doesn't exist, so prepend to start of doc
                    //TODO: Maybe insert at the cursor position, rather than just at the top of the doc?
                    templateCode = "@{\n\tLayout = \"" + newValue + "\";\n}\n" + templateCode;
                }
            } else {
                if (layoutDefRegex.test(templateCode)) {
                    // Declaration exists, so just update it
                    templateCode = templateCode.replace(layoutDefRegex, "$1$2");
                }
            }
            
            UmbEditor.SetCode(templateCode);

            return false;
        }
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

    
    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>

</asp:Content>
