<%@ Page Language="c#" CodeBehind="EditNodeTypeNew.aspx.cs" AutoEventWireup="True" ValidateRequest="false"
    Async="true" AsyncTimeOut="300" Trace="false" Inherits="Umbraco.Web.UI.Umbraco.Settings.EditNodeTypeNew" MasterPageFile="../masterpages/umbracoPage.Master" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="uc1" TagName="ContentTypeControlNew" Src="../controls/ContentTypeControlNew.ascx" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {

            UmbClientMgr.appActions().bindSaveShortCut();

            // Auto selection/de-selection of default template based on allow templates
            $("#<%= templateList.ClientID %> input[type='checkbox']").on("change", function() {
                var checkbox = $(this);
                var ddl = $("#<%= ddlTemplates.ClientID %>");
                // If default template is not set, and an allowed template is selected, auto-select the default template
                if (checkbox.is(":checked")) {
                    if (ddl.val() == "0") {
                        ddl.val(checkbox.val());
                    }
                }
                else {
                    // If allowed template has been de-selected, and it's selected as the default, then de-select the default template
                    if (ddl.val() == checkbox.val()) {
                        ddl.val("0");
                    }
                }
            });

            // Auto selection allowed template based on default template
            $("#<%= ddlTemplates.ClientID %>").on("change", function () {
                var ddl = $(this);
                if (ddl.val() != "0") {
                    $("#<%= templateList.ClientID %> input[type='checkbox'][value='" + ddl.val() + "']").prop("checked", true);
                }
            });
        });
    </script>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
    <uc1:ContentTypeControlNew ID="ContentTypeControlNew1" runat="server"></uc1:ContentTypeControlNew>

    <cc1:Pane ID="tmpPane" runat="server">
        <cc1:PropertyPanel Text="Allowed templates" runat="server">
            <div class="guiInputStandardSize" style="border: #ccc 1px solid; background: #fff;
                overflow: auto; height: 170px;">
                <asp:CheckBoxList ID="templateList" runat="server" />
            </div>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel Text="Default template" runat="server">
            <asp:DropDownList ID="ddlTemplates" CssClass="guiInputText guiInputStandardSize"
                runat="server" />
        </cc1:PropertyPanel>
    </cc1:Pane>
</asp:Content>
