<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="formPickerDialog.aspx.cs" Inherits="Umbraco.Forms.UI.Dialogs.formPickerDialog" %>
<%@ Register TagPrefix="ui" Namespace="umbraco.uicontrols" Assembly="controls" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title></title>
    
    <link rel="stylesheet" type="text/css" href="/umbraco_client/ui/default.css" />
    
    <script type="text/javascript" src="/umbraco_client/ui/jquery.js"></script>
    <script type="text/javascript" src="/umbraco_client/ui/default.js"></script>
    
    <script type="text/javascript" language="javascript">

        var formguid = '';
        function dialogHandler(id) {
            formguid = id;
            jQuery("#submitbutton").attr("disabled", false);
        }

        function UpdatePicker() {
            if (formguid != '') {
                parent.hidePopWin(true, formguid);
            }
        }
    </script>
    
</head>
<body class="umbracoDialog" style="margin: 15px 10px 0px 10px;">
    <form id="form1" runat="server" onsubmit="UpdatePicker();return false;" action="#">
    <div>
    
 
    <h1>Pick or create a new form</h1>

    
    <ui:TabView AutoResize="false" Width="625px" Height="405px" runat="server"  ID="tv_options" />
    </ui:TabView>
 
    <ui:Pane ID="pane_select" runat="server"> 
      <div style="padding: 5px; background: #fff; height: 350px;">
        <iframe id="treeFrame" name="treeFrame" src="../../TreeInit.aspx?app=forms&treeType=forms&isDialog=true&dialogMode=id&contextMenu=false&functionToCall=parent.dialogHandler" style="width: 405px; height: 350px; float: left; border: none;" frameborder="0"></iframe>
       
      </div>
    </ui:Pane>
    
    <ui:Pane ID="pane_new" runat="server"> 
        <div style="padding: 5px; background: #fff; height: 350px;">
             <iframe id="createFrame" name="createFrame" src="formPickerCreateFormDialog.aspx" style="width: 405px; height: 350px; float: left; border: none;" frameborder="0"></iframe>
        </div>
    </ui:Pane>
    <p>
        <input type="submit" value="select" style="width: 60px;" disabled="true" id="submitbutton"/> <em id="orcopy">or</em>
        <a href="#" style="color: blue" onclick="parent.hidePopWin(false,0);" id="cancelbutton">cancel</a>
      </p>   
      
    </div>
    </form>
</body>
</html>
