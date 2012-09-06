<%@ Page language="c#" Codebehind="insertTable.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.insertTable" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
  <HEAD>
		<title>Insert Table</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<LINK href="../css/umbracoGui.css" type="text/css" rel="stylesheet">
		<style>BODY { MARGIN: 2px }
	</style>
		<script language="javascript">

function insertTable()
{
	theForm = document.tableForm
	// Indsaml tabel info
	var tableCol = theForm.tableCol[theForm.tableCol.selectedIndex].text;
	var tableRow = theForm.tableRow[theForm.tableRow.selectedIndex].text;
	
	var tableJust = theForm.tableJust[theForm.tableJust.selectedIndex].text;
	var tableWidth = theForm.tableWidth.value;
	var tableHeight = theForm.tableHeight.value;
	var tablePadding = theForm.tablePadding.value;
	var tableSpacing = theForm.tableSpacing.value;

	// hvis der ikke er sat padding eller spacing, skal de sættes til nul
	if (tablePadding == '') tablePadding = '0';
	if (tableSpacing == '') tableSpacing = '0';

	var tableBorder = theForm.tableBorder[theForm.tableBorder.selectedIndex].text;
	var tableClass = "";
	if (theForm.tableClass.length > 0)
		tableClass = theForm.tableClass[theForm.tableClass.selectedIndex].value;
	
	
	// Hvis tabellen blot redigeres, skal vi ikke generere kode
	if (theForm.editMode.value != '') {
		var tableTag = new Array(	tableJust,
									tableWidth,
									tableHeight,
									tablePadding,
									tableSpacing,
									tableBorder,
									tableClass);
	} else {
	
		// vi skal lave kode
		var tableTag = '<TABLE';
		
		if (tableJust != '') tableTag += ' ALIGN="'+ tableJust + '"';
		if (tableWidth != '') tableTag += ' WIDTH="'+ tableWidth + '"';
		if (tableHeight != '') tableTag += ' HEIGHT="'+ tableHeight + '"';
		if (tablePadding != '') tableTag += ' CELLPADDING="'+ tablePadding + '"';
		if (tableSpacing != '') tableTag += ' CELLSPACING="'+ tableSpacing + '"';
		if (tableBorder != '') tableTag += ' BORDER="'+ tableBorder + '"';
		if (tableClass != '') tableTag += ' CLASS="'+ tableClass + '"';
		
		tableTag += '>\n';
		
		// kolonner og rækker
		for (i=1; i<=tableRow;i++) {
			tableTag += '\t<TR>\n';
			for(j=1; j<=tableCol;j++) {
				tableTag += '\t\t<TD></TD>\n';
			}
			tableTag += '\t</TR>\n';
		}
		tableTag += '</TABLE>\n';
	}
	window.returnValue = tableTag;
    window.close();
}
		</script>
</HEAD>
	<body MS_POSITIONING="GridLayout">
		<h3><%=umbraco.ui.Text("defaultdialogs", "inserttable", this.getUser())%></h3>
		<br />
	<span class="guiDialogMedium"><%=umbraco.ui.Text("general", "size", this.getUser())%></span>
	<hr size=1 noshade>
	<TABLE WIDTH="100%" CELLPADDING=4 CELLSPACING=0 class="propertyPane">
	<form id="tableForm" runat="server">
	<input type="hidden" name="editMode" >
        <TR>
            <TD class="propertyHeader" width="200">
            	<%=umbraco.ui.Text("defaultdialogs", "tableColumns", this.getUser())%>
			</TD>
			<td class="propertyContent">
				<select name="tableCol" size="1" class="guiInputText" >
					<option selected>1
<option>2
<option>3
<option>4
<option>5
<option>6
<option>7
<option>8
<option>9
<option>10
<option>11
<option>12
<option>13
<option>14
<option>15
<option>16
<option>17
<option>18
<option>19
<option>20
<option>21
<option>22
<option>23
<option>24
<option>25
<option>26
<option>27
<option>28
<option>29
<option>30
<option>31
<option>32
<option>33
<option>34
<option>35
<option>36
<option>37
<option>38
<option>39
<option>40
<option>41
<option>42
<option>43
<option>44
<option>45
<option>46
<option>47
<option>48
<option>49
<option>50</option>

				</select>
			</td>
		</TR>

        <TR>
            <TD class="propertyHeader" width="200">
            	<%=umbraco.ui.Text("defaultdialogs", "tableRows", this.getUser())%>
			</TD>
			<td class="propertyContent">
				<select name="tableRow" size="1" class="guiInputText" >
					<option selected>1
<option>2
<option>3
<option>4
<option>5
<option>6
<option>7
<option>8
<option>9
<option>10
<option>11
<option>12
<option>13
<option>14
<option>15
<option>16
<option>17
<option>18
<option>19
<option>20
<option>21
<option>22
<option>23
<option>24
<option>25
<option>26
<option>27
<option>28
<option>29
<option>30
<option>31
<option>32
<option>33
<option>34
<option>35
<option>36
<option>37
<option>38
<option>39
<option>40
<option>41
<option>42
<option>43
<option>44
<option>45
<option>46
<option>47
<option>48
<option>49
<option>50</option>

				</select>
			</td>
		</TR>
	</TABLE>
	<br />
	<span class="guiDialogMedium"><%=umbraco.ui.Text("general", "layout", this.getUser())%></span>
	<hr size=1 noshade>
	<TABLE WIDTH="100%" CELLPADDING=4 CELLSPACING=0 class="propertyPane">
        <TR>
            <TD class="propertyHeader" width="200">
            	<%=umbraco.ui.Text("general", "justify", this.getUser())%>
			</TD>
    <TD class="propertyContent align=" right?>
			<select class="guiInputText" 
      size=1 name=tableJust>
					<option selected>
<option>Left
<option>Right
<option>Center</option>
			</select> </TD>
            <TD class="propertyHeader" width="200">
            	<%=umbraco.ui.Text("general", "width", this.getUser())%>
			</TD>
			<td class="propertyContent">
				<input type="text" name="tableWidth" value="100%" class="guiInputText" size="4" maxlength="4">
			</td>
		</TR>

        <TR>
            <TD class="propertyHeader" width="200">
            	<%=umbraco.ui.Text("general", "innerMargin", this.getUser())%>
			</TD>
    <TD class="propertyContent align=" right?>
			<input class="guiInputText" 
      maxlength="4" type=text size=4 name=tablePadding> </TD>
            <TD class="propertyHeader" width="200">
            	<%=umbraco.ui.Text("general", "height", this.getUser())%>
			</TD>
			<td class="propertyContent">
				<input type="text" name="tableHeight" class="guiInputText" size="4" maxlength="4">
			</td>
		</TR>

        <TR>
            <TD class="propertyHeader" width="200">
            	<%=umbraco.ui.Text("general", "cellMargin", this.getUser())%>
			</TD>
    <TD class="propertyContent align=" right?>
			<input class="guiInputText" 
      maxlength="4" type=text size=4 name=tableSpacing> </TD>
            <TD class="propertyHeader" width="200">
            	&nbsp;
			</TD>
			<td class="propertyContent">
				&nbsp;
			</td>
		</TR>
	</TABLE>
<br />
	<span class="guiDialogMedium"><%=umbraco.ui.Text("general", "design", this.getUser())%></span>
	<hr size=1 noshade>
	<TABLE WIDTH="100%" CELLPADDING=4 CELLSPACING=0 class="propertyPane">
        <TR>
            <TD class="propertyHeader" width="200">
            	<%=umbraco.ui.Text("general", "border", this.getUser())%>
			</TD>
			<td class="propertyContent">
				<select name="tableBorder" size="1" class="guiInputText">
					<option selected>0
<option>1
<option>2
<option>3</option>

				</select>
			</td>
            <TD class="propertyHeader" width="200">
            	<%=umbraco.ui.Text("buttons", "styleChoose", this.getUser())%>
			</TD>
			<td class="propertyContent">
				<asp:DropDownList Runat="server" ID="tableClass"></asp:DropDownList>
			</td>
		</TR></FORM>
	</TABLE>
	&nbsp;
	<input type="button" class="guiInputButton" onClick="if (confirm('<%=umbraco.ui.Text("areyousure").Replace("'", "\\'")%>')) window.close();" value="<%=umbraco.ui.Text("cancel")%>"> &nbsp; 
	<input type="button" class="guiInputButton" onClick="insertTable()" value="<%=umbraco.ui.Text("insert")%>">
	</body>
</HTML>
