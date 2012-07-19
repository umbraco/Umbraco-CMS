<%@ Page language="c#" Codebehind="logout.aspx.cs" AutoEventWireup="True" Inherits="umbraco.logout" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" > 

<html>
  <head>
    <title>logout</title>
    <meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
    <meta name="CODE_LANGUAGE" Content="C#">
    <meta name=vs_defaultClientScript content="JavaScript">
    <meta name=vs_targetSchema content="http://schemas.microsoft.com/intellisense/ie5">
  </head>
  <body MS_POSITIONING="GridLayout">
	
    <form id="Form1" method="post" runat="server">
		<script>
			window.top.location.href='login.aspx?redir=<%=Server.UrlEncode(Request["redir"]) %>';
		</script>
     </form>
	
  </body>
</html>
