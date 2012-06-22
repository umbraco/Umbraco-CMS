<%@ Page language="c#" Codebehind="insertAnchor.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.insertAnchor" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" > 

<html>
  <head>
		<title>umbraco :: <%=umbraco.ui.Text("defaultdialogs", "insertAnchor", this.getUser())%> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</title>
    <meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
    <meta name="CODE_LANGUAGE" Content="C#">
    <meta name=vs_defaultClientScript content="JavaScript">
    <meta name=vs_targetSchema content="http://schemas.microsoft.com/intellisense/ie5">
	<LINK href="../css/umbracoGui.css" type="text/css" rel="stylesheet">
	<style>body {margin: 2px;}</style>
<SCRIPT LANGUAGE="JavaScript">
<!--

function insertAnchor()
{
	window.returnValue = document.forms[0].anchorName.value;
    window.close();
} 

function doSubmit() {insertAnchor();}

	var functionsFrame = this;
	var tabFrame = this;
	var isDialog = true;
	var submitOnEnter = true;
-->
	</script>
<script type="text/javascript" src="../js/umbracoCheckKeys.js"></script>
  </head>
  <body MS_POSITIONING="GridLayout">
	
	<h3><%=umbraco.ui.Text("defaultdialogs", "insertAnchor", this.getUser())%></h3>
    <form id="Form1" method="post" runat="server">
	<TABLE WIDTH="100%" CELLPADDING=4 CELLSPACING=0 class="propertyPane">
        <TR>
            <TD class="propertyHeader">
            	<%=umbraco.ui.Text("defaultdialogs", "anchorInsert", this.getUser())%>
			</td>
			<td class="propertyContent">
				<input type="text" size="30" maxlength="100" class="guiInputText" name="anchorName" value="<%=Request.QueryString["anchorName"]%>"><br/><br/>
	<input type="button" class="guiInputButton" onClick="insertAnchor()" value="<%=umbraco.ui.Text("general", "insert", this.getUser())%>">
	 &nbsp; <input type="button" class="guiInputButton" onClick="if (confirm('<%=umbraco.ui.Text("general", "areyousure", this.getUser())%>')) window.close();" value="<%=umbraco.ui.Text("general", "cancel", this.getUser())%>">
			</td>
		</tr>
	</table><br />

     </form>
	
  </body>
</html>
