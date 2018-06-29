<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" CodeBehind="umbracoField.aspx.cs"
    AutoEventWireup="True" Inherits="umbraco.dialogs.umbracoField" %>

<%@ Import Namespace="Umbraco.Web" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="cc1" Namespace="Umbraco.Web._Legacy.Controls" Assembly="Umbraco.Web" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        (function($) {
            $(document).ready(function() {
                var umbracoField = new Umbraco.Dialogs.UmbracoField({
                    cancelButton: $("#cancelButton"),
                    submitButton: $("#submitButton"),
                    form: document.forms[0],
                    tagName: document.forms[0].<%= tagName.ClientID %>.value,
                    objectId: '<%=Request.CleanForXss("objectId")%>'
                });
                umbracoField.init();
            });
        })(jQuery);

        var functionsFrame = this;
        var tabFrame = this;
        var isDialog = true;
        var submitOnEnter = true;
    </script>

</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <umb:JsInclude ID="JsInclude1" runat="server" FilePath="js/umbracoCheckKeys.js" PathNameAlias="UmbracoRoot" />
    <umb:JsInclude ID="JsInclude2" runat="server" FilePath="Dialogs/UmbracoField.js" PathNameAlias="UmbracoClient" />

    <div class="umb-panel umb-modal">
    <div class="umb-panel-body no-header with-footer">
    <input type="hidden" name="tagName" runat="server" id="tagName" value="?UMBRACO_GETITEM" />

    <cc1:Pane ID="pane_form" runat="server" Title="Choose value">
        <cc1:PropertyPanel ID="pp_insertField" runat="server">
            <cc1:FieldDropDownList ID="fieldPicker" Width="170px" Rows="1" runat="server"></cc1:FieldDropDownList>
            <input type="text" size="25" name="field" class="guiInputTextTiny"/>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="pp_insertAltField" runat="server">
            <cc1:FieldDropDownList ID="altFieldPicker" Width="170px" Rows="1" runat="server"></cc1:FieldDropDownList>
            <input type="text" size="25" name="useIfEmpty" class="guiInputTextTiny"/><br />
            <span class="guiDialogTiny">
                <%=Services.TextService.Localize("templateEditor/usedIfEmpty")%></span>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="pp_insertAltText" runat="server">
            <textarea rows="1" style="width: 310px;" name="alternativeText" class="guiInputTextTiny"></textarea><br />
            <span class="guiDialogTiny">
                <%=Services.TextService.Localize("templateEditor/usedIfAllEmpty")%></span>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="pp_recursive" runat="server">
            <input type="checkbox" name="recursive" value="true"/> <%=Services.TextService.Localize("templateEditor/recursive")%>
        </cc1:PropertyPanel>

    </cc1:Pane>

    <cc1:Pane runat="server" Title="Format and encoding">
        <cc1:PropertyPanel ID="pp_FormatAsDate" runat="server">
            <input type="radio" name="formatAsDate" value="formatAsDate"/>
            <%=Services.TextService.Localize("templateEditor/dateOnly")%>
            &nbsp; &nbsp;
            <input type="radio" name="formatAsDate" value="formatAsDateWithTime"/>
            <%=Services.TextService.Localize("templateEditor/withTime")%>
            <input type="text" size="6" name="formatAsDateWithTimeSeparator" style="width: 35px" class="guiInputTextTiny"/>
            <br />
            <span class="guiDialogTiny">Format the value as a date, or a date with time, accoring to the active culture.</span>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="pp_casing" runat="server">
            <input type="radio" name="toCase" value=""/>
            <%=Services.TextService.Localize("templateEditor/none")%>
            <input type="radio" name="toCase" value="lower"/>
            <%=Services.TextService.Localize("templateEditor/lowercase")%>
            <input type="radio" name="toCase" value="upper"/>
            <%=Services.TextService.Localize("templateEditor/uppercase")%>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="pp_encode" runat="server">
            <input type="radio" name="urlEncode" value=""/>
            <%=Services.TextService.Localize("none")%>
            <input type="radio" name="urlEncode" value="url"/>
            <%=Services.TextService.Localize("templateEditor/urlEncode")%>
            <input type="radio" name="urlEncode" value="html"/>
            <%=Services.TextService.Localize("templateEditor/htmlEncode")%>
            <br />
            <span class="guiDialogTiny">
                <%=Services.TextService.Localize("templateEditor/urlEncodeHelp")%>
            </span>
        </cc1:PropertyPanel>
        </cc1:Pane>

        <cc1:Pane runat="server" Title="Modify output">
        <cc1:PropertyPanel ID="pp_insertBefore" runat="server">
            <input type="text" size="40" name="insertTextBefore" class="guiInputTextTiny"/><br />
            <span class="guiDialogTiny">
                <%=Services.TextService.Localize("templateEditor/insertedBefore")%>
            </span>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_insertAfter" runat="server">
            <input type="text" size="40" name="insertTextAfter" class="guiInputTextTiny"/><br />
            <span class="guiDialogTiny">
                <%=Services.TextService.Localize("templateEditor/insertedAfter")%>
            </span>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="pp_convertLineBreaks" runat="server">
            <input type="checkbox" name="convertLineBreaks" value="true"/>  <%=Services.TextService.Localize("templateEditor/convertLineBreaks")%>

            <br />
            <span class="guiDialogTiny">
                <%=Services.TextService.Localize("templateEditor/convertLineBreaksHelp")%>
            </span>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="pp_removePTags" runat="server">
            <input type="checkbox" name="stripParagraph" value="true"/>  <%=Services.TextService.Localize("templateEditor/removeParagraph")%>
            <br />
            <span class="guiDialogTiny"><%=Services.TextService.Localize("templateEditor/removeParagraphHelp")%>
            </span>
        </cc1:PropertyPanel>
    </cc1:Pane>
    </div>

    <div class="umb-panel-footer">
        <div class="btn-toolbar umb-btn-toolbar">
            <a id="cancelButton" href="#" class="btn btn-link">
                <%=Services.TextService.Localize("general/cancel")%></a>

            <input id="submitButton" type="button" name="gem" class="btn btn-primary" value="<%=Services.TextService.Localize("insert")%>" />
        </div>
    </div>

    </div>

</asp:Content>
