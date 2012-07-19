<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<%@ Page Language="c#" CodeBehind="Default.aspx.cs" AutoEventWireup="True" Inherits="umbraco._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
  <title>Start Umbraco</title>
  <script type="text/javascript">
    function startUmbraco() {
      window.open('umbraco.aspx', 'u<%=Request.ServerVariables["SERVER_NAME"].Replace(".","").Replace("-","")%>', 'height=600,width:850,scrollbars=yes,resizable=yes,top=0,left=0,status=yes');
    }
  </script>
  
  <link href="<%= umbraco.IO.IOHelper.ResolveUrl( umbraco.IO.SystemDirectories.Umbraco_client ) %>/ui/default.css" type="text/css" rel="stylesheet"/>
  <style type="text/css">
    body {
      text-align: center;
      height: 100%;
      padding: 40px;
      background-color: white;
    }
    #container {
      height: 200px;
      border: 1px solid #ccc;
      width: 400px;
    }
  </style>
</head>
<body onload="startUmbraco();">
  <form id="Form1" method="post" runat="server">
  <img src="images/umbracoSplash.png" alt="umbraco" style="width: 354px; height: 61px;" /><br />
  <br />
  <h3>umbraco <%=umbraco.ui.Text("dashboard", "openinnew")%></h3>
  
  <span class="guiDialogNormal">
    <br />
    <br />
    <a href="#" onclick="startUmbraco();">
      <%=umbraco.ui.Text("dashboard", "restart")%>
      umbraco</a> &nbsp; <a href="../">
      <%=umbraco.ui.Text("dashboard", "browser")%></a></span>
  
  <br />
  <br />
  
  <span class="guiDialogTiny">(<%=umbraco.ui.Text("dashboard", "nothinghappens")%>)</span>
  <br />
  <br />
  <span class="guiDialogTiny"><a href="http://umbraco.org">
    <%=umbraco.ui.Text("dashboard", "visit")%>
    umbraco.org</a></span>
  </form>
</body>
</html>
