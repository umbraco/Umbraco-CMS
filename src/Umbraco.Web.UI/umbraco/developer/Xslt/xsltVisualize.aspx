<%@ Page Language="C#" MasterPageFile="../../masterpages/umbracoDialog.Master" AutoEventWireup="true"
    CodeBehind="xsltVisualize.aspx.cs" ValidateRequest="false" Inherits="umbraco.presentation.umbraco.developer.Xslt.xsltVisualize" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="cc2" Namespace="umbraco.controls" Assembly="umbraco" %>

<asp:Content runat="server" ContentPlaceHolderID="head">

    <script type="text/javascript">

        jQuery(document).ready(function () {
            var xsltSelection = jQuery("#<%=xsltSelection.ClientID %>");
            if (xsltSelection.val() == '') {
                xsltSelection.val(UmbClientMgr.contentFrame().xsltSnippet);

                // automatically submit if page is chosen
                var picker = $(".umb-tree-picker input");
                if (picker.val() != '') {
                    jQuery("#<%=visualizeDo.ClientID %>").click();
                }
            }
        });

        function encodeDecodeResult(isChecked) {
            var html = jQuery("#result").html();
            if (isChecked) {
                jQuery("#result").html(
                    html.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/\n/g, "<br/>").replace(/\r/g, ""));
            } else {
                jQuery("#result").html(
                        html.replace(/<BR>/g, "\n").replace(/&amp;/g, '&').replace(/&lt;/g, '<').replace(/&gt;/g, '>'));
            }
        }
    </script>

</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">

    <div class="umb-dialog-body">

        <cc1:Pane ID="Pane1" runat="server" Text="Visualize XSLT">

            <cc1:PropertyPanel ID="PropertyPanel1" runat="server">
                <input type="hidden" runat="server" id="xsltSelection" />
                <cc2:ContentPicker ID="contentPicker" runat="server" />
                <br />
                <br />
            </cc1:PropertyPanel>

            <cc1:PropertyPanel runat="server">
                <input type="checkbox" id="encodeDecode" onclick="encodeDecodeResult(this.checked)" />
                <label for="encodeDecode">Encode/Decode result</label>
            </cc1:PropertyPanel>


        </cc1:Pane>

        <cc1:Pane ID="visualizeContainer" runat="server" Text="Generated Result" Visible="false">
            <cc1:PropertyPanel ID="visualizePanel" runat="server">
                <asp:Literal ID="visualizeArea" runat="server"></asp:Literal>
            </cc1:PropertyPanel>
        </cc1:Pane>


        <div class="umb-dialog-footer btn-toolbar umb-btn-toolbar">
            <asp:Button ID="visualizeDo" runat="server" CssClass="btn btn-primary" Text="Visualize XSLT" OnClick="visualizeDo_Click" />
        </div>

    </div>
</asp:Content>
