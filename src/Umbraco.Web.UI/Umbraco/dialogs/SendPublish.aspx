<%@ Page language="c#" Codebehind="SendPublish.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.SendPublish" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
	<head>
		<title>umbraco - <%=Services.TextService.Localize("editContentSendToPublish")%></title>
		<link href="../css/umbracoGui.css" type="text/css" rel="stylesheet"/>
	</head>
	<body style="padding: 10px;">
		<h3><img src="../images/publish.gif" alt="Republish" align="absmiddle" style="float:left; margin-top: 3px; margin-right: 5px"/> <%=Services.TextService.Localize("editContentSendToPublishText")%></h3>
		<br/>
		<a href="#" onclick="javascript:UmbClientMgr.closeModalWindow();" style="margin-left: 30px" class="guiDialogNormal"><%=Services.TextService.Localize("closewindow")%></a>
	</body>
</html>
