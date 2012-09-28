<%@ Page Language="C#" MasterPageFile="../../masterpages/umbracoPage.Master" AutoEventWireup="true"
    CodeBehind="editView.aspx.cs" Inherits="umbraco.cms.presentation.settings.views.editView"
    ValidateRequest="False" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<asp:Content ContentPlaceHolderID="head" runat="server">
    
    <script language="javascript" type="text/javascript">
        jQuery(document).ready(function() {
            jQuery(".codeTemplate").click(function(){              
                insertCodeBlockFromTemplate(jQuery(this).attr("rel"));
            });
        });
        
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

        function umbracoTemplateInsertMasterPageContentContainer() {
          var master = document.getElementById('<%= MasterTemplate.ClientID %>')[document.getElementById('<%= MasterTemplate.ClientID %>').selectedIndex].value;
          if (master == "") master = 0;
          umbraco.presentation.webservices.legacyAjaxCalls.TemplateMasterPageContentContainer(<%=Request["templateID"] %>, master, umbracoTemplateInsertMasterPageContentContainerDo);
        }
        
        function umbracoTemplateInsertMasterPageContentContainerDo(result) {
          UmbEditor.Insert(result + '\n', '\n</asp\:Content>\n', '<%= editorSource.ClientID%>');
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
        
       function insertContentElement(id){
       
       //nasty hack to avoid asp.net freaking out because of the markup...
        var cp = 'asp:Content ContentPlaceHolderId="' + id + '"';
        cp += ' runat="server"';
        cp += '>\n\t<!-- Insert "' + id + '" markup here -->';

        UmbEditor.Insert('\n<' + cp, '\n</asp:Content' + '>\n', '<%= editorSource.ClientID %>');
       }
       
       function insertPlaceHolderElement(id){       
        
        var cp = 'asp:ContentPlaceHolder Id="' + id + '"';
        cp += ' runat="server"';
        cp += '>\n\t<!-- Insert default "' + id + '" markup here -->';

        UmbEditor.Insert('\n<' + cp, '\n</asp:ContentPlaceHolder' + '>\n', '<%= editorSource.ClientID %>');
       }
        
       function insertCodeBlock()
       {
            var snip = umbracoInsertSnippet();
            UmbEditor.Insert(snip.BeginTag, snip.EndTag, '<%= editorSource.ClientID %>');
       }

       function umbracoInsertSnippet() {
            var snip = new UmbracoCodeSnippet();
            var cp = 'umbraco:Macro runat="server" language="cshtml"';
            snip.BeginTag = '\n<' + cp + '>\n';
            snip.EndTag = '\n<' + '/umbraco:Macro' + '>\n';
            snip.TargetId = "<%= editorSource.ClientID %>";
            return snip;
       }

       function insertMacro(alias)
       {
       	    var macroElement = "umbraco:Macro";
		    var cp = macroElement + ' Alias="'+ alias +'" runat="server"';
            UmbEditor.Insert('<' + cp +' />','', '<%= editorSource.ClientID %>');
       }
       
       function openMacroModal(alias)
       {
            var t = "";
            if(alias != null && alias != ""){
                t = "&alias="+alias;
            }
            UmbClientMgr.openModalWindow('<%= umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) %>/dialogs/editMacro.aspx?objectId=<%= editorSource.ClientID %>' + t, 'Insert Macro', true, 470, 530, 0, 0, '', '');
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
