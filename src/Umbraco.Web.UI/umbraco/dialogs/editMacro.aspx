<%@ Page Language="C#" MasterPageFile="../MasterPages/UmbracoPage.Master" AutoEventWireup="true" CodeBehind="EditMacro.aspx.cs" Inherits="Umbraco.Web.UI.Umbraco.Dialogs.EditMacro" %>

<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<%@ Import Namespace="Umbraco.Web" %>
<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ContentPlaceHolderID="head" runat="server">

    <umb:CssInclude runat="server" FilePath="Dialogs/EditMacro.css" PathNameAlias="UmbracoClient" />
    <umb:JsInclude runat="server" FilePath="Dialogs/EditMacro.js" PathNameAlias="UmbracoClient" />

    <script type="text/javascript">
        (function($) {
            $(document).ready(function () {
                Umbraco.Dialogs.EditMacro.getInstance().init({
                    useAspNetMasterPages: <%=umbraco.UmbracoSettings.UseAspNetMasterPages.ToString().ToLower() %>,
                    codeEditorElementId: "<%=Request.GetItemAsString("objectId")%>",
                    renderingEngine: "<%=Request.GetItemAsString("renderingEngine", "Mvc")%>",
                    macroAlias: '<%= _macroAlias %>'
                });
            });
        })(jQuery);        
    </script>

</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <asp:Panel ID="pl_edit" runat="server" Visible="false">
        <cc2:Pane ID="pane_edit" runat="server">
            <div class="macro-properties">
                <asp:PlaceHolder ID="macroProperties" runat="server" />
            </div>
        </cc2:Pane>
        <p>
            <input data-bind="click: updateMacro" type="button" value="<%=umbraco.ui.Text("general", "ok", this.getUser())%>" />
            <em>or </em>
            <a data-bind="click: cancelModal" ><%=umbraco.ui.Text("general", "cancel", this.getUser())%></a>
        </p>
        
        <script type="text/javascript">
            (function($) {
                //when this panel loads, check if there are any macro properties, if not then load the macro content into the editor and close the modal
                $(document).ready(function() {
                    var countOfProperties = <%=CountOfMacroProperties %>;
                    if (countOfProperties == 0) {
                        Umbraco.Dialogs.EditMacro.getInstance().updateMacro();
                    }
                });
            })(jQuery);
        </script>

    </asp:Panel>
    <asp:Panel ID="pl_insert" runat="server">
        <cc2:Pane ID="pane_insert" runat="server">
            <cc2:PropertyPanel ID="pp_chooseMacro" runat="server" Text="Choose a macro">
                <asp:ListBox Rows="1" ID="umb_macroAlias" runat="server"></asp:ListBox>
            </cc2:PropertyPanel>
        </cc2:Pane>
        <p>
            <asp:Button ID="bt_insert" runat="server" Text="ok" OnClick="renderProperties"></asp:Button>
            <em>or </em>
            <a data-bind="click: cancelModal" ><%=umbraco.ui.Text("general", "cancel", this.getUser())%></a>
        </p>
    </asp:Panel>
    <div id="renderContent" style="display: none">
        <asp:PlaceHolder ID="renderHolder" runat="server"></asp:PlaceHolder>
    </div>
</asp:Content>
