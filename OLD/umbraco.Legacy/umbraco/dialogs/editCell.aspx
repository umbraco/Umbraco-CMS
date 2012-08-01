<%@ Page language="c#" Codebehind="editCell.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.editCell" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>umbraco :: <%=umbraco.ui.Text("defaultdialogs", "insertformfield", this.getUser())%> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<LINK href="../css/umbracoGui.css" type="text/css" rel="stylesheet">
		<style>body {margin: 2px;}</style>
<SCRIPT LANGUAGE="JavaScript">
<!--

function editCell()
{
	window.returnValue = document.forms[0].cellWidth.value + ',\'' + 
						document.forms[0].tableAlign[document.forms[0].tableAlign.selectedIndex].text + '\', \'' +
						document.forms[0].tableVAlign[document.forms[0].tableVAlign.selectedIndex].text + '\', \'\'';
    window.close();
} 

function undoCell() 
{
	window.returnValue = '<%=umbraco.helper.Request("width")%>';
	window.close();
}

//-->
</SCRIPT>
	</HEAD>
	<body MS_POSITIONING="GridLayout">
			
		<h3><%=umbraco.ui.Text("defaultdialogs", "insertlink", this.getUser())%></h3>
		<form id="Form1" method="post" runat="server">
			<TABLE class="propertyPane" id="macroProperties" cellSpacing="0" cellPadding="4" width="98%"
				border="0" runat="server">
				<TBODY>
        <TR>
            <TD class="guiDialogForm">
            	<%=umbraco.ui.Text("width")%>
			</td>
			<td>
				<input type="text" size="4" maxlength="10" class="guiInputText" name="cellWidth" value="<%=umbraco.helper.Request("width")%>">
			</td>
		</tr>
        <TR>
            <TD class="guiDialogForm">
            	<%=umbraco.ui.Text("justify")%>
			</td>
			<td>
				<asp:DropDownList Runat="server" id="tableAlign" CssClass="guiInputText"></asp:DropDownList>
			</td>
		</tr>
        <TR>
            <TD class="guiDialogForm">
            	<%=umbraco.ui.Text("justifyVertical")%>
			</td>
			<td>
				<asp:DropDownList Runat="server" id="tableVAlign" CssClass="guiInputText"></asp:DropDownList>
				</select><br/><br/>
	<input type="button" class="guiInputButton" onClick="undoCell()" value="Fortryd"> &nbsp; 
	<input type="button" class="guiInputButton" onClick="editCell()" value="Opdatér">
			</td>
		</tr>
				</tbody>
			</TABLE>
		</form>
	</body>
</HTML>

