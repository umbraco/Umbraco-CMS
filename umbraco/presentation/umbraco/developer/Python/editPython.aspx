<%@ Page ValidateRequest="false" Language="c#" MasterPageFile="../../masterpages/umbracoPage.Master"
    CodeBehind="editPython.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.developer.editPython" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ID="cp0" runat="server" ContentPlaceHolderID="head">

    <script type="text/javascript">
    function pythonInsertValue(theValue) {
      insertAtCaret(document.getElementById('pythonSource'), theValue)
    }

    function doSubmit() {
      document.contentForm.submit();
    }

    function showError() {
        var id = "#<%=errorHolder.ClientID%>";
      if (jQuery(id).is(":visible")) {
        jQuery(id).hide();
        jQuery('#showErrorLink').html('<b>Show error</b> <img src="../images/arrowForward.gif" align="absmiddle" border="0"/><br/><br/>');
      }
      else {
        jQuery(id).show();
        jQuery('#showErrorLink').html('<b>Hide error</b> <img src="../images/arrowDown.gif" align="absmiddle" border="0"/><br/>');
      }
    }
    </script>

</asp:Content>
<asp:Content ID="cp1" runat="server" ContentPlaceHolderID="body">
    <cc1:UmbracoPanel ID="UmbracoPanel1" runat="server" Text="Edit xsl" Height="300"
        Width="500">
        <cc1:Pane ID="Pane1" runat="server">
            <cc1:PropertyPanel ID="pp_filename" Text="Filename" runat="server">
                <asp:TextBox ID="pythonFileName" runat="server" Width="400" CssClass="guiInputText"></asp:TextBox>
            </cc1:PropertyPanel>
            <cc1:PropertyPanel ID="pp_testing" runat="server" Text="Skip testing (ignore errors)">
                <asp:CheckBox ID="SkipTesting" runat="server"></asp:CheckBox>
            </cc1:PropertyPanel>
            <br />
            <asp:Literal ID="closeErrorMessage" runat="server" Visible="false" EnableViewState="false">
				  <a id="showErrorLink" href="javascript:showError()"><b>Hide error</b> <img src="../images/arrowDown.gif" align="absmiddle" border="0"/></a>
            </asp:Literal>
            <asp:Panel ID="errorHolder" runat="server" Visible="False" EnableViewState="false">
                <span style="color: red">
                    <asp:Label ID="pythonError" runat="server"></asp:Label>
                </span>
            </asp:Panel>
            <cc1:CodeArea ID="pythonSource" CodeBase="Python" AutoResize="true" OffSetX="47" OffSetY="55" runat="server" />
        </cc1:Pane>
    </cc1:UmbracoPanel>
</asp:Content>
