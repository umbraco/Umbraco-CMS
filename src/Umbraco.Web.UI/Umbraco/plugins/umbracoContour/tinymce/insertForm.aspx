<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="insertForm.aspx.cs" Inherits="Umbraco.Forms.UI.Pages.tinymce3.insertForm" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Insert form</title>
    
    <script type="text/javascript" src="../../../../umbraco_client/ui/jquery.js"></script>
    <script type="text/javascript" src="../../../../umbraco_client/ui/default.js"></script>    
    
    <script type="text/javascript" src="../../../../umbraco_client/tinymce3/tiny_mce_popup.js"></script>
    <script type="text/javascript" src="../../../../umbraco_client/tinymce3/utils/mctabs.js"></script>
    <script type="text/javascript" src="../../../../umbraco_client/tinymce3/utils/form_utils.js"></script>
    <script type="text/javascript" src="../../../../umbraco_client/tinymce3/utils/validate.js"></script>
    
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
    </script>
    
</head>
<body>
    <form id="form1" runat="server">
    <div>
        This will insert and edit umbraco forms
        
        <div>
        <a href="../formPickerChooseFormDialog.aspx">Choose an existing form</a>
        
        or
        
         <a href="../formPickerCreateFormDialog.aspx">Create a new form</a>
         
         </div>
    </div>
    
    <asp:TextBox ID="tb_guid" TextMode="MultiLine" runat="server">
    
    
    </asp:TextBox>
    
    <asp:Button OnClick="InsertFormMacro" runat="server" Text="Insert form" />
    </form>
</body>
</html>
