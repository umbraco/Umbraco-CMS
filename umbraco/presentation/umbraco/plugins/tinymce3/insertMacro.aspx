<%@ Page Language="c#" ValidateRequest="false" CodeBehind="insertMacro.aspx.cs" AutoEventWireup="True" Inherits="umbraco.presentation.tinymce3.insertMacro" Trace="false" %>
<%@ Register TagPrefix="ui" Namespace="umbraco.uicontrols" Assembly="controls" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <title>{#advlink_dlg.title}</title>
    
    <script type="text/javascript" src="../../../umbraco_client/ui/jquery.js"></script>
    <script type="text/javascript" src="../../../umbraco_client/ui/default.js"></script>    
    
    <script type="text/javascript" src="../../../umbraco_client/tinymce3/tiny_mce_popup.js"></script>
    <script type="text/javascript" src="../../../umbraco_client/tinymce3/utils/mctabs.js"></script>
    <script type="text/javascript" src="../../../umbraco_client/tinymce3/utils/form_utils.js"></script>
    <script type="text/javascript" src="../../../umbraco_client/tinymce3/utils/validate.js"></script>
<!--
    <link href="../../../umbraco_client/tinymce3/plugins/advlink/css/advlink.css" rel="stylesheet" type="text/css" />
-->
        
    <base target="_self" />

    <style type="text/css">
      .propertyItemheader{width: 140px !Important;}
      select, textarea, input.guiInputTextStandard{width: 200px;}
    </style>
    
    <script type="text/javascript" language="javascript">
        var inst = tinyMCEPopup.editor;
        var elm = inst.selection.getNode();

        function umbracoEditMacroDo(fieldTag, macroName, renderedContent) {
            
            // is it edit macro?
            if (!tinyMCE.activeEditor.dom.hasClass(elm, 'umbMacroHolder')) {
                while (!tinyMCE.activeEditor.dom.hasClass(elm, 'umbMacroHolder') && elm.parentNode) {
                    elm = elm.parentNode;
                }
            }

            if (elm.nodeName == "DIV" && tinyMCE.activeEditor.dom.getAttrib(elm, 'class').indexOf('umbMacroHolder') >= 0) {
                tinyMCE.activeEditor.dom.setOuterHTML(elm, renderedContent);
            }
            else {
                tinyMCEPopup.execCommand("mceInsertContent", false, renderedContent);
            }
            tinyMCEPopup.close();
        }

        function saveTreepickerValue(appAlias, macroAlias) {
            var treePicker = window.showModalDialog('../../dialogs/treePicker.aspx?app=' + appAlias + '&treeType=' + appAlias, 'treePicker', 'dialogWidth=350px;dialogHeight=300px;scrollbars=no;center=yes;border=thin;help=no;status=no')
            document.forms[0][macroAlias].value = treePicker;
            document.getElementById("label" + macroAlias).innerHTML = "</b><i>updated with id: " + treePicker + "</i><b><br/>";
        }

        var macroAliases = new Array();

        function registerAlias(alias) {
            macroAliases[macroAliases.length] = alias;
        }

        function pseudoHtmlEncode(text) {
            return text.replace(/\"/gi, "&amp;quot;").replace(/\</gi, "&amp;lt;").replace(/\>/gi, "&amp;gt;");
        }

        function init() {
            var inst = tinyMCEPopup.editor;
            var elm = inst.selection.getNode();
        }

        function insertSomething() {
            tinyMCEPopup.close();
        }
    </script>
    
</head>
<body>
    <form id="Form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    
    <input type="hidden" name="macroMode" value="<%=Request["mode"]%>" />
    
    <%if (Request["umb_macroID"] != null || Request["umb_macroAlias"] != null) {%>
        <input type="hidden" name="umb_macroID" value="<%=umbraco.helper.Request("umb_macroID")%>" />
        <input type="hidden" name="umb_macroAlias" value="<%=umbraco.helper.Request("umb_macroAlias")%>" />
    <% }%>
     
    <ui:Pane id="pane_edit" runat="server" Visible="false">
        <div style="height: 380px; overflow: auto;">
          <asp:PlaceHolder ID="macroProperties" runat="server" />
        </div>
    </ui:Pane>
    
    <asp:Panel ID="edit_buttons" runat="server" Visible="false">
    <p>
        <asp:Button ID="bt_renderMacro" OnClick="renderMacro_Click" runat="server" Text="ok"></asp:Button>
        <em> or </em>
        <a href="#" style="color: blue"  onclick="tinyMCEPopup.close();"><%=umbraco.ui.Text("general", "cancel", this.getUser())%></a>
    </p>    
    </asp:Panel>
    
    <ui:Pane ID="pane_insert" runat="server">
      <ui:PropertyPanel ID="pp_selectMacro" runat="server" Text="Select macro">
          <asp:DropDownList ID="umb_macroAlias" Width="150px" runat="server"/>
      </ui:PropertyPanel>
    </ui:Pane>
    
    <asp:Panel ID="insert_buttons" runat="server">
     <p>
          <input type="submit" value="<%=umbraco.ui.Text("general", "ok", this.getUser())%>" />
          <em> or </em>
          <a href="#" style="color: blue"  onclick="tinyMCEPopup.close();"><%=umbraco.ui.Text("general", "cancel", this.getUser())%></a>
     </p>
    </asp:Panel>   
    
    <div id="renderContent" style="display: none">
        <asp:PlaceHolder ID="renderHolder" runat="server"></asp:PlaceHolder>
    </div>
    
    
    </form>
    
    
    <script type="text/javascript">
        tinyMCEPopup.onInit.add(init);
    </script>
    
</body>
</html>