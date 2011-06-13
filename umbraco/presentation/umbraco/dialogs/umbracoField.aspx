<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" CodeBehind="umbracoField.aspx.cs"
    AutoEventWireup="True" Inherits="umbraco.dialogs.umbracoField" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<asp:Content ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        html, body
        {
            margin-top: 0px !important;
            padding-top: 0px !important;</style>
    <script type="text/javascript">
		function doSubmit()
		{
		
			var tagString = '<' + document.forms[0].<%= tagName.ClientID %>.value;
			
			// hent feltnavne
			var field = document.forms[0].field.value;
			if (field != '')
				tagString += ' field="' + field + '"';
				
			var useIfEmpty = document.forms[0].useIfEmpty.value;
			if (useIfEmpty != '')
				tagString += ' useIfEmpty="' + useIfEmpty + '"';
			
			var alternativeText = document.forms[0].alternativeText.value;
			if (alternativeText != '')
				tagString += ' textIfEmpty="' + alternativeText + '"';
			
			var insertTextBefore = document.forms[0].insertTextBefore.value;
			if (insertTextBefore != '')
				tagString += ' insertTextBefore="' + insertTextBefore.replace(/\"/gi,"&quot;").replace(/\</gi,"&lt;").replace(/\>/gi,"&gt;") + '"';
			
			var insertTextAfter = document.forms[0].insertTextAfter.value;
			if (insertTextAfter != '')
				tagString += ' insertTextAfter="' + insertTextAfter.replace(/\"/gi,"&quot;").replace(/\</gi,"&lt;").replace(/\>/gi,"&gt;") + '"';
			
			if (document.forms[0].formatAsDate[1].checked)
				tagString += ' formatAsDateWithTime="true" formatAsDateWithTimeSeparator="' + document.forms[0].formatAsDateWithTimeSeparator.value + '"';
			else if(document.forms[0].formatAsDate[0].checked)
				tagString += ' formatAsDate="true"';

			if (document.forms[0].toCase[1].checked)
				tagString += ' case="' + document.forms[0].toCase[1].value + '"';
			else if (document.forms[0].toCase[2].checked)
				tagString += ' case="' + document.forms[0].toCase[2].value + '"';

			if (document.forms[0].recursive.checked)
				tagString += ' recursive="true"';
      
      if (document.forms[0].urlEncode[1].checked)
				tagString += ' urlEncode="true"';
			else if (document.forms[0].urlEncode[2].checked)
				tagString += ' htmlEncode="true"';
      
			if (document.forms[0].stripParagraph.checked)
				tagString += ' stripParagraph="true"';

			if (document.forms[0].convertLineBreaks.checked)
				tagString += ' convertLineBreaks="true"';
			
			  tagString += " runat=\"server\" />";
			
			UmbClientMgr.contentFrame().focus();

		    UmbClientMgr.contentFrame().UmbEditor.Insert(tagString, '', '<%=umbraco.helper.Request("objectId")%>');
		  	
		  UmbClientMgr.closeModalWindow();
		}
		
	var functionsFrame = this;
	var tabFrame = this;
	var isDialog = true;
	var submitOnEnter = true;
    </script>
    <style type="text/css">
        .propertyItemheader
        {
            width: 170px !important;
        }
    </style>
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <umb:JsInclude ID="JsInclude1" runat="server" FilePath="js/umbracoCheckKeys.js" PathNameAlias="UmbracoRoot" />
    <input type="hidden" name="tagName" runat="server" id="tagName" value="?UMBRACO_GETITEM">
    <cc1:Pane ID="pane_form" runat="server">
        <cc1:PropertyPanel ID="pp_insertField" runat="server">
            <asp:ListBox ID="fieldPicker" Width="170px" Rows="1" runat="server"></asp:ListBox>
            <input type="text" size="25" name="field" class="guiInputTextTiny">
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_insertAltField" runat="server">
            <asp:ListBox ID="altFieldPicker" Width="170px" Rows="1" runat="server"></asp:ListBox>
            <input type="text" size="25" name="useIfEmpty" class="guiInputTextTiny"><br />
            <span class="guiDialogTiny">
                <%=umbraco.ui.Text("templateEditor", "usedIfEmpty")%></span>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_insertAltText" runat="server">
            <textarea rows="1" style="width: 310px;" name="alternativeText" class="guiInputTextTiny"></textarea><br />
            <span class="guiDialogTiny">
                <%=umbraco.ui.Text("templateEditor", "usedIfAllEmpty")%></span>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_recursive" runat="server">
            <input type="checkbox" name="recursive" value="true">
            <%=umbraco.ui.Text("yes")%>
            <br />
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_insertBefore" runat="server">
            <input type="text" size="40" name="insertTextBefore" class="guiInputTextTiny"><br />
            <span class="guiDialogTiny">
                <%=umbraco.ui.Text("templateEditor", "insertedBefore")%>
            </span>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_insertAfter" runat="server">
            <input type="text" size="40" name="insertTextAfter" class="guiInputTextTiny"><br />
            <span class="guiDialogTiny">
                <%=umbraco.ui.Text("templateEditor", "insertedAfter")%>
            </span>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_FormatAsDate" runat="server">
            <input type="radio" name="formatAsDate" value="formatAsDate">
            <%=umbraco.ui.Text("templateEditor", "dateOnly")%>
            &nbsp; &nbsp;
            <input type="radio" name="formatAsDate" value="formatAsDateWithTime">
            <%=umbraco.ui.Text("templateEditor", "withTime")%>
            :
            <input type="text" size="6" name="formatAsDateWithTimeSeparator" class="guiInputTextTiny">
            <br />
            <span class="guiDialogTiny">Format the value as a date, or a date with time, accoring
                to the active culture.</span>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_casing" runat="server">
            <input type="radio" name="toCase" value="">
            <%=umbraco.ui.Text("templateEditor", "none")%>
            <input type="radio" name="toCase" value="lower">
            <%=umbraco.ui.Text("templateEditor", "lowercase")%>
            <input type="radio" name="toCase" value="upper">
            <%=umbraco.ui.Text("templateEditor", "uppercase")%>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_encode" runat="server">
            <input type="radio" name="urlEncode" value="">
            <%=umbraco.ui.Text("none")%>
            <input type="radio" name="urlEncode" value="url">
            <%=umbraco.ui.Text("templateEditor","urlEncode")%>
            <input type="radio" name="urlEncode" value="html">
            <%=umbraco.ui.Text("templateEditor", "htmlEncode")%>
            <br />
            <span class="guiDialogTiny">
                <%=umbraco.ui.Text("templateEditor", "urlEncodeHelp")%>
            </span>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_convertLineBreaks" runat="server">
            <input type="checkbox" name="convertLineBreaks" value="true">
            <%=umbraco.ui.Text("yes")%>
            <br />
            <span class="guiDialogTiny">
                <%=umbraco.ui.Text("templateEditor", "convertLineBreaksHelp")%>
            </span>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_removePTags" runat="server">
            <input type="checkbox" name="stripParagraph" value="true">
            <%=umbraco.ui.Text("yes")%>
            <br />
            <span class="guiDialogTiny">
                <%=umbraco.ui.Text("templateEditor", "removeParagraphHelp")%>
            </span>
        </cc1:PropertyPanel>
    </cc1:Pane>
    <br />
    <input type="button" name="gem" value=" <%=umbraco.ui.Text("insert")%> " onclick="doSubmit()">
    &nbsp; <em>or </em>&nbsp; <a href="#" style="color: blue" onclick="UmbClientMgr.closeModalWindow()">
        <%=umbraco.ui.Text("general", "cancel", this.getUser())%></a>
</asp:Content>
