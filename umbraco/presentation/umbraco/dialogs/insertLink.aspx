<%@ Page language="c#" Codebehind="insertLink.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.insertLink" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>umbraco :: <%=umbraco.ui.Text("defaultdialogs", "insertlink", this.getUser())%> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<LINK href="../css/umbracoGui.css" type="text/css" rel="stylesheet">
		<style>body {margin: 2px;}</style>
		<script language="javascript">
			var linkToInsert = "";
			
			function dialogHandler(id) {
				// Find link
				document.forms[0].linkText.value = id;				
//				document.forms[0].linkText.value = linkToInsert.substr(linkToInsert.indexOf(",")+1, linkToInsert.length-2-linkToInsert.indexOf(","));
			}

			function doLink()
			{
				var linkName = document.forms[0].linkText.value;
	
				if (linkName.indexOf('@') > 0 && linkName.indexOf('mailto:') < 0)
					linkName = 'mailto:' + linkName;
				else {
					if (linkName.length - replaceChars(linkName, ".", "").length > 1 && linkName.toLowerCase().indexOf('http') < 0 )
						linkName = 'http://' + linkName;
				}
				
				
				window.returnValue = document.forms[0].linkNewWindow.checked + '|'+linkName.replace(/\+/g,'\%2b').replace(/\?/g,'\%3f');
				window.close();
			}
			
function replaceChars(entry, out, add) {
	temp = "" + entry;

	while (temp.indexOf(out)>-1) {
		pos= temp.indexOf(out);
		temp = "" + (temp.substring(0, pos) + add + 
		temp.substring((pos + out.length), temp.length));
	}

	return temp;
}

function doSubmit() {doLink();}

	var functionsFrame = this;
	var tabFrame = this;
	var isDialog = true;
	var submitOnEnter = true;

		</script>
		<script type="text/javascript" src="../js/umbracoCheckKeys.js"></script>
	</HEAD>
	<body onload="this.focus();">
			
		<h3><%=umbraco.ui.Text("defaultdialogs", "insertlink", this.getUser())%></h3>
		<form id="Form1" method="post" runat="server">
			<TABLE class="propertyPane" id="macroProperties" cellSpacing="0" cellPadding="4" width="98%"
				border="0" runat="server">
				<TBODY>
					<TR>
						<TD class="propertyHeader" width="30%"><%=umbraco.ui.Text("defaultdialogs", "link", this.getUser())%></TD>
						<TD class="propertyContent">
							<asp:TextBox id="linkText" runat="server" Width="252px"></asp:TextBox>&nbsp;&nbsp;
							<input type="hidden" id="actualLink" runat="server" />
							<input type="button" onClick="doLink()" Value="<%=umbraco.ui.Text("general", "ok", this.getUser())%>"/></TD>
					</TR>
					<TR>
						<TD class="propertyHeader" width="30%"><%=umbraco.ui.Text("defaultdialogs", "linknewwindow", this.getUser())%>
						</TD>
						<TD class="propertyContent">
							<asp:CheckBox id="linkNewWindow" runat="server"></asp:CheckBox>
							<%=umbraco.ui.Text("general", "yes", this.getUser())%>
						</TD>
					</TR>
					<TR>
						<TD class="propertyHeader" width="30%"><%=umbraco.ui.Text("defaultdialogs", "linkinternal", this.getUser())%></TD>
						<TD class="propertyContent">
						<asp:PlaceHolder ID="ph" Runat="server"></asp:PlaceHolder>
						</TD>
					</TR>
				</tbody>
			</TABLE>
		</form>
	</body>
</HTML>

