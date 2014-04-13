<%@ Page Language="c#" ValidateRequest="false" CodeBehind="insertMacro.aspx.cs" AutoEventWireup="True"
    Inherits="umbraco.presentation.tinymce3.insertMacro" Trace="false" %>

<%@ Import Namespace="Umbraco.Web" %>
<%@ Register TagPrefix="ui" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web" %>
<%@ Register TagPrefix="umbClient" Namespace="Umbraco.Web.UI.Bundles" Assembly="umbraco" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>{#advlink_dlg.title}</title>
    <base target="_self" />

    <ui:UmbracoClientDependencyLoader runat="server" ID="ClientLoader" />
    
    <umbClient:JsApplicationLib runat="server" />
    <umbClient:JsJQueryCore runat="server" />
    <umbClient:JsUmbracoApplicationCore runat="server" />

    <umb:JsInclude ID="JsInclude8" runat="server" FilePath="ui/default.js" PathNameAlias="UmbracoClient"
        Priority="4" />
    <umb:JsInclude ID="JsInclude1" runat="server" FilePath="tinymce3/tiny_mce_popup.js"
        PathNameAlias="UmbracoClient" Priority="100" />
    <umb:JsInclude ID="JsInclude3" runat="server" FilePath="tinymce3/utils/mctabs.js"
        PathNameAlias="UmbracoClient" Priority="101" />
    <umb:JsInclude ID="JsInclude4" runat="server" FilePath="tinymce3/utils/form_utils.js"
        PathNameAlias="UmbracoClient" Priority="102" />
    <umb:JsInclude ID="JsInclude5" runat="server" FilePath="tinymce3/utils/validate.js"
        PathNameAlias="UmbracoClient" Priority="103" />

    <style type="text/css">
        .propertyItemheader
        {
            width: 140px !important;
        }
        select, textarea, input.guiInputTextStandard
        {
            width: 200px;
        }
        .macroPane
        {
            height: 360px;
            overflow: auto;
        }
    </style>
    <script type="text/javascript">
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

                tinyMCEPopup.restoreSelection();

                tinyMCEPopup.editor.execCommand("mceInsertContent", false, renderedContent);
            }

            //workaround for the chrome issue, seems to work if there is a small delay before the close function call
            id = window.setTimeout("tinyMCEPopup.close()", 10);

            //tinyMCEPopup.close();
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
        /*
        function init() {
        var inst = tinyMCEPopup.editor;
        var elm = inst.selection.getNode();
        }
        */
        function insertSomething() {
            tinyMCEPopup.close();
        }
    </script>

</head>
<body>
    <form id="Form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>

    <input type="hidden" name="macroMode" value="<%=Request["mode"]%>" />
    <%if (Request["umb_macroID"] != null || Request["umb_macroAlias"] != null)
      {%>
    <input type="hidden" name="umb_macroID" value="<%=Request.CleanForXss("umb_macroID")%>" />
    <input type="hidden" name="umb_macroAlias" value="<%=Request.CleanForXss("umb_macroAlias")%>" />
    <% }%>
    <ui:Pane ID="pane_edit" runat="server" Visible="false">
        <div class="macroPane">
            <asp:PlaceHolder ID="macroProperties" runat="server" />
        </div>
    </ui:Pane>
    <asp:Panel ID="edit_buttons" runat="server" Visible="false">
        <p>
            <asp:Button ID="bt_renderMacro" OnClick="renderMacro_Click" runat="server" Text="ok">
            </asp:Button>
            <em>or </em><a id="cancelbtn" href="#" style="color: blue" onclick="tinyMCEPopup.close();">
                <%=umbraco.ui.Text("general", "cancel", UmbracoUser)%></a>
        </p>
    </asp:Panel>
    <ui:Pane ID="pane_insert" runat="server">
        <ui:PropertyPanel ID="pp_selectMacro" runat="server" Text="Select macro">
            <asp:DropDownList ID="umb_macroAlias" Width="150px" runat="server" />
        </ui:PropertyPanel>
    </ui:Pane>
    <asp:Panel ID="insert_buttons" runat="server">
        <p>
            <input type="submit" value="<%=umbraco.ui.Text("general", "ok", UmbracoUser)%>" />
            <em>or </em><a href="#" style="color: blue" onclick="tinyMCEPopup.close();">
                <%=umbraco.ui.Text("general", "cancel", UmbracoUser)%></a>
        </p>
    </asp:Panel>
    <div id="renderContent" style="display: none">
        <asp:PlaceHolder ID="renderHolder" runat="server"></asp:PlaceHolder>
    </div>
    </form>
    <script type="text/javascript" >
        var inst; // =  tinyMCEPopup.editor;
        var elm; // = inst.selection.getNode();

        jQuery(document).ready(function () {
            //            tinyMCEPopup.onInit.add(init);
            inst = tinyMCEPopup.editor;
            elm = inst.selection.getNode();

    <asp:Literal ID="jQueryReady" runat="server"></asp:Literal>
            
        });

    </script>
</body>
</html>
