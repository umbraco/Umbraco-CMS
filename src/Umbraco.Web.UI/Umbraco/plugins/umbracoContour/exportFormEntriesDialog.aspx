<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="exportFormEntriesDialog.aspx.cs" Inherits="Umbraco.Forms.UI.Dialogs.exportFormEntriesDialog" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <link rel="stylesheet" href="css/dialogs.css" type="text/css" media="screen" />
    
    <style>

    #dialogcontainer
    {
        width: 570px;
    	height:380px;
    	overflow:auto;
    }
    </style>

</head>
<body>
    <div id="dialogcontainer">
    <form id="form1" runat="server">
    
    <h1>Export Entries</h1>
    
    <div>
        <div class="propertypane">
    
            <div class="propertyItem" style="">
            <div class="propertyItemheader">Export Type</div> <div class="propertyItemContent"><asp:DropDownList ID="dd_exportTypes" AutoPostBack="true" runat="server" />   </div>
            </div>
            
            <div class="propertyPaneFooter">-</div>
        </div>        
        
        <div class="propertypane" ID="paneAddSettings" runat="server" Visible="false">
        
            <div class="propertyItem" style="">
                <div class="propertyItemheader">Description</div> 
                <div class="propertyItemContent">
                    <em><asp:Literal ID="lt_export_description" runat="server" /></em>
                </div>
            </div>
            
            <asp:PlaceHolder ID="ph_Settings" runat="server" />
             
             <div class="propertyItem" style="">
                <div class="propertyItemheader"></div> 
                <div class="propertyItemContent">
                     <asp:Button ID="bt_export" runat="server" Text="Export data" OnClick="Export" />
                </div>
            </div>
            
            <div class="propertyPaneFooter">-</div>
        </div>
        
         <div class="dialogcontrols">
        
                <a href="javascript:parent.CloseExportFormEntriesDialog();">Close</a>
         </div>
        
    </div>
    </form>
    </div>
</body>
</html>
