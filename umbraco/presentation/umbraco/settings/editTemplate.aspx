<%@ Page MasterPageFile="../masterpages/umbracoPage.Master" Language="c#" Codebehind="editTemplate.aspx.cs" ValidateRequest="false"
  AutoEventWireup="True" Inherits="umbraco.cms.presentation.settings.editTemplate" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

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
          var selectedTemplate = templateDropDown.options[templateDropDown.selectedIndex].id;
          var masterTemplate = "<%= umbraco.IO.SystemDirectories.Masterpages%>/" + selectedTemplate + ".master";
          
          if(selectedTemplate == "")
            masterTemplate = "<%= umbraco.IO.SystemDirectories.Umbraco%>/masterpages/default.master";
                    
          var regex = /MasterPageFile=[~a-z0-9/._"-]+/im;
          
           if (templateCode.match(regex)) {
             templateCode = templateCode.replace(regex, 'MasterPageFile="' + masterTemplate + '"');
             
             UmbEditor.SetCode(templateCode);
           
           } else {
             //todo, spot if a directive is there, and if not suggest that the user inserts it.. 
             alert("Master directive not found...");
             return false;
           } 
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
        
  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">

<cc1:UmbracoPanel ID="Panel1" runat="server" Width="608px" Height="336px" hasMenu="true">
      <cc1:Pane ID="Pane7" runat="server" Height="44px" Width="528px">
        <cc1:PropertyPanel ID="pp_name" runat="server">
            <asp:TextBox ID="NameTxt" Width="350px" runat="server"></asp:TextBox>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel id="pp_alias" runat="server">
            <asp:TextBox ID="AliasTxt" Width="350px" runat="server"></asp:TextBox>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_masterTemplate" runat="server">
            <asp:DropDownList ID="MasterTemplate" Width="350px" runat="server" />
        </cc1:PropertyPanel>
        <cc1:PropertyPanel id="pp_source" runat="server">
            <cc1:CodeArea ID="editorSource" runat="server" CodeBase="HTML" ClientSaveMethod="doSubmit" AutoResize="true" OffSetX="37" OffSetY="54" CssClass="codepress html" />
        </cc1:PropertyPanel>
      </cc1:Pane>
    </cc1:UmbracoPanel>
</asp:Content>

    
