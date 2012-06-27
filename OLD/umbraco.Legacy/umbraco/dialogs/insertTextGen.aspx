	<%@ Page language="c#" Codebehind="insertTextGen.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.insertTextGen" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>umbraco :: <%=umbraco.ui.Text("defaultdialogs", "insertgraphicheadline", this.getUser())%> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<LINK href="../css/umbracoGui.css" type="text/css" rel="stylesheet">
		<style>BODY { MARGIN: 2px }
		</style>
		<script language="javascript">

function doTextGen() {
	var theForm = document.Form1;
	
	var bgColor = theForm.pickerBgColor.value;
	var fgColor = theForm.pickerColor.value;
	var isBold = theForm.isBold.checked;
	if (isBold) 
		isBold = '1';
	else
		isBold = '0';
	
	if (fgColor == '')
		fgColor = '000000'
	if (bgColor == '')
		bgColor = 'FFFFFF'
		
	var textGen = '<img src="<%=umbraco.GlobalSettings.Path%>/TextGen.aspx?text=' + theForm.Text.value.replace(/\&/gi,"\&amp;") + '&font=' +
		theForm.fontList[theForm.fontList.selectedIndex].value + '&size='+
		theForm.fontSizeList[theForm.fontSizeList.selectedIndex].text + '&color=' + fgColor + '&bgColor=' + bgColor + '&bold=' + isBold + '" border="0" alt="' + theForm.Text.value.replace(/\&/gi,"\&amp;") + '"/>';

	window.returnValue = textGen;
    window.close();


}


function doSubmit() {doTextGen();}
	var functionsFrame = this;
	var tabFrame = this;
	var isDialog = true;
	var submitOnEnter = true;

		</script>
		<script type="text/javascript" src="../js/umbracoCheckKeys.js"></script>
	</HEAD>
	<body MS_POSITIONING="GridLayout">
		<h3><%=umbraco.ui.Text("defaultdialogs", "insertgraphicheadline", this.getUser())%></h3>
		<form id="Form1" method="post" runat="server">
			<TABLE class="propertyPane" id="macroProperties" cellSpacing="0" cellPadding="4" width="98%"
				border="0" runat="server">
				<TBODY>
					<TR>
						<TD class="propertyHeader" width="30%"><%=umbraco.ui.Text("graphicheadline", "text", this.getUser())%></TD>
						<TD class="propertyContent">
							<asp:TextBox id="Text" runat="server" Width="252px"></asp:TextBox>&nbsp;&nbsp;
						</TD>
					</TR>
					<TR>
						<TD class="propertyHeader" width="30%"><%=umbraco.ui.Text("graphicheadline", "font", this.getUser())%>
						</TD>
						<TD class="propertyContent">
							<asp:DropDownList Runat="server" ID="fontList"></asp:DropDownList>
							:
							<asp:DropDownList ID="fontSizeList" Runat="server"></asp:DropDownList><br />
							<asp:CheckBox ID="isBold" Runat="server" />
							<%=umbraco.ui.Text("graphicheadline", "bold", this.getUser())%>
						</TD>
					</TR>
					<TR>
						<TD class="propertyHeader" width="30%"><%=umbraco.ui.Text("graphicheadline", "color", this.getUser())%></TD>
						<TD class="propertyContent">
							<asp:PlaceHolder id="PlaceholderColor" runat="server"></asp:PlaceHolder>
						</TD>
					</TR>
					<TR>
						<TD class="propertyHeader" width="30%"><%=umbraco.ui.Text("graphicheadline", "backgroundcolor", this.getUser())%></TD>
						<TD class="propertyContent">
							<asp:PlaceHolder id="PlaceHolderBgColor" runat="server"></asp:PlaceHolder>
						</TD>
					</TR>
					<TR>
						<TD class="propertyHeader" width="30%"></TD>
						<TD class="propertyContent">
							<input type="button" onclick="doSubmit()" value=" OK "/>
						</TD>
					</TR>
				</TBODY>
			</TABLE>
		</form>
	</body>
</HTML>
