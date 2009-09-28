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
      }
    }
    </script>

</asp:Content>
<asp:Content ID="cp1" runat="server" ContentPlaceHolderID="body">
    <cc1:UmbracoPanel ID="UmbracoPanel1" runat="server" Text="Edit xsl" Height="300" Width="600">
        <cc1:Pane ID="Pane1" runat="server" Style="margin-bottom: 10px;">
            <cc1:PropertyPanel ID="pp_filename" Text="Filename" runat="server">
                <asp:TextBox ID="pythonFileName" runat="server" Width="400" CssClass="guiInputText"></asp:TextBox>
            </cc1:PropertyPanel>
            <cc1:PropertyPanel ID="pp_testing" runat="server" Text="Skip testing (ignore errors)">
                <asp:CheckBox ID="SkipTesting" runat="server"></asp:CheckBox>
            </cc1:PropertyPanel>
            <cc1:PropertyPanel ID="pp_error" runat="server">
                <div runat="server" id="errorHolder" visible="false" enableviewstate="false" class="error">
                    <a id="showErrorLink" href="javascript:showError()">
                        <p><b>Hide error</b></p>
                    </a>
                    <br />
                    <asp:Label ID="pythonError" runat="server"></asp:Label>
                </div>
            </cc1:PropertyPanel>
            <cc1:CodeArea ID="pythonSource" CodeBase="Python" AutoResize="true" OffSetX="47" OffSetY="55" runat="server" />
        </cc1:Pane>
    </cc1:UmbracoPanel>
</asp:Content>
