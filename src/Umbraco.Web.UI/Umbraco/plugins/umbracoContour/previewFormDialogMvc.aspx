<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="previewFormDialogMvc.aspx.cs" Inherits="Umbraco.Forms.UI.Dialogs.previewFormDialogMvc" %>
<%@ Register TagPrefix="umbraco" Namespace="umbraco.presentation.templateControls" Assembly="umbraco" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Form Test</title>
    <link rel="stylesheet" href="css/defaultform.css" type="text/css" media="screen" />
    
    <style>
        
        body, html{margin: 0px 0px 20px 0px; padding: 0px 0px 0px 0px;
           font-size: 12px; background:#fff; font-family: "Lucida Grande", Arial, sans-serif;}
           
        #uformNotice{
          background:#FFF6BF;color:#514721;border-color:#FFD324;
          border-bottom: #FFD324 2px solid; padding: 15px; font-size: 1.2em;  margin-bottom: 20px;
          text-align: center;
        }
          
        #uformNotice em{font-weight: bold; font-style: normal; }
        #uformHolder{padding: 20px;}
        
        
    </style>
    <link type="text/css" href="css/jquery-ui-1.8.18.custom.css" rel="stylesheet" />	
    <script type="text/javascript" src="scripts/jquery-1.5.1.min.js"></script>
    <script type="text/javascript" src="scripts/jquery-ui-1.8.11.min.js"></script>
    <script type="text/javascript" src="scripts/jquery.validate.min.js"></script>
    <script type="text/javascript" src="scripts/jquery.validate.unobtrusive.min.js"></script>

</head>
<body>
    
    <div id="uformNotice" runat="server">
      <em>Notice:</em> This is a fully functional version of your form and will act as any normal form would. Complete with workflows and 
       datastorage.
    </div>
    
    
    
    <umbraco:Macro ID="Macro1" runat="server" Alias="umbracoContour.RazorRenderForm" FormGuid="[@guid]" />

</body>
</html>

