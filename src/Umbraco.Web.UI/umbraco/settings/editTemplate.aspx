<%@ Page MasterPageFile="../masterpages/umbracoPage.Master" Language="c#" CodeBehind="editTemplate.aspx.cs"
    ValidateRequest="false" AutoEventWireup="True" Inherits="umbraco.cms.presentation.settings.editTemplate" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<asp:Content ContentPlaceHolderID="head" runat="server">
    <umb:CssInclude ID="CssInclude1" runat="server" FilePath="splitbutton/splitbutton.css"
        PathNameAlias="UmbracoClient" />
    <umb:JsInclude ID="JsInclude" runat="server" FilePath="splitbutton/jquery.splitbutton.js"
        PathNameAlias="UmbracoClient" Priority="1" />
    <script language="javascript" type="text/javascript">
        jQuery(document).ready(function() {
            //macro split button
            jQuery('#sbMacro').splitbutton({menu:'#macroMenu'});
            jQuery("#splitButtonMacro").appendTo("#splitButtonMacroPlaceHolder");
            jQuery(".macro").click(function(){
                var alias = jQuery(this).attr("rel");
               if(jQuery(this).attr("params") == "1")
                {
                    openMacroModal(alias);
                }
                else
                {
                    insertMacro(alias);
                }
            });
            applySplitButtonOverflow('mcontainer','innerc','macroMenu','.macro', 'showMoreMacros');
            
            //razor macro split button
            jQuery('#sb').splitbutton({menu:'#codeTemplateMenu'});
            jQuery("#splitButton").appendTo("#splitButtonPlaceHolder");

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

       function insertCodeBlockFromTemplate(templateId)
       {
            
        jQuery.ajax({
            type: "POST",
            url: "../webservices/templates.asmx/GetCodeSnippet",
            data: "{templateId: '" + templateId + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function(msg) {

            var cp = 'umbraco:Macro  runat="server" language="cshtml"';
            UmbEditor.Insert('\n<' + cp +'>\n'  + msg.d,'\n</umbraco:Macro' + '>\n', '<%= editorSource.ClientID %>');

                    }
           });

       }

       function insertMacro(alias)
       {
            <%if (umbraco.UmbracoSettings.UseAspNetMasterPages) { %>
			var macroElement = "umbraco:Macro";
			<%}else{ %>
			var macroElement = "?UMBRACO_MACRO";
			<%}%>

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
                <cc1:CodeArea ID="editorSource" runat="server" CodeBase="HtmlMixed" EditorMimeType="text/html" ClientSaveMethod="doSubmit"
                    AutoResize="true" OffSetX="37" OffSetY="54"/>
            </cc1:PropertyPanel>
        </cc1:Pane>
    </cc1:UmbracoPanel>
    <div id="splitButton" style="display: inline; height: 23px; vertical-align: top;">
        <a href="javascript:insertCodeBlock();" id="sb" class="sbLink">
            <img alt="Insert Inline Razor Macro" src="../images/editor/insRazorMacro.png" title="Insert Inline Razor Macro"
                style="vertical-align: top;">
        </a>
    </div>
    <div id="codeTemplateMenu" style="width: 285px;">
        <asp:Repeater ID="rpt_codeTemplates" runat="server">
            <ItemTemplate>
                <div class="codeTemplate" rel="<%# DataBinder.Eval(Container, "DataItem.Key") %>">
                    <%# DataBinder.Eval(Container, "DataItem.Value") %>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
    <div id="splitButtonMacro" style="display: inline; height: 23px; vertical-align: top;">
        <a href="javascript:openMacroModal();" id="sbMacro" class="sbLink">
            <img alt="Insert Macro" src="../images/editor/insMacroSB.png" title="Insert Macro"
                style="vertical-align: top;">
        </a>
    </div>
    <div id="macroMenu" style="width: 285px">
        <asp:Repeater ID="rpt_macros" runat="server">
            <ItemTemplate>
                <div class="macro" rel="<%# DataBinder.Eval(Container, "DataItem.macroAlias")%>"
                    params="<%#  DoesMacroHaveSettings(DataBinder.Eval(Container, "DataItem.id").ToString()) %>">
                    <%# DataBinder.Eval(Container, "DataItem.macroName")%>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>
</asp:Content>
