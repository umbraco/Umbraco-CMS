<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" CodeBehind="umbracoField.aspx.cs"
    AutoEventWireup="True" Inherits="umbraco.dialogs.umbracoField" %>

<%@ Import Namespace="Umbraco.Web" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

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
                <%=umbraco.ui.Text("templateEditor", "usedIfEmpty")%></span>
        </cc1:PropertyPanel>
        
        <cc1:PropertyPanel ID="pp_insertAltText" runat="server">
            <textarea rows="1" style="width: 310px;" name="alternativeText" class="guiInputTextTiny"></textarea><br />
            <span class="guiDialogTiny">
                <%=umbraco.ui.Text("templateEditor", "usedIfAllEmpty")%></span>
        </cc1:PropertyPanel>
        
        <cc1:PropertyPanel ID="pp_recursive" runat="server">
            <input type="checkbox" name="recursive" value="true"/> <%=umbraco.ui.Text("templateEditor", "recursive")%>
        </cc1:PropertyPanel>

    </cc1:Pane>
    
    <cc1:Pane runat="server" Title="Format and encoding">
        <cc1:PropertyPanel ID="pp_FormatAsDate" runat="server">
            <input type="radio" name="formatAsDate" value="formatAsDate"/>
            <%=umbraco.ui.Text("templateEditor", "dateOnly")%>
            &nbsp; &nbsp;
            <input type="radio" name="formatAsDate" value="formatAsDateWithTime"/>
            <%=umbraco.ui.Text("templateEditor", "withTime")%>
            <input type="text" size="6" name="formatAsDateWithTimeSeparator" style="width: 35px" class="guiInputTextTiny"/>
            <br />
            <span class="guiDialogTiny">Format the value as a date, or a date with time, accoring to the active culture.</span>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="pp_casing" runat="server">
            <input type="radio" name="toCase" value=""/>
            <%=umbraco.ui.Text("templateEditor", "none")%>
            <input type="radio" name="toCase" value="lower"/>
            <%=umbraco.ui.Text("templateEditor", "lowercase")%>
            <input type="radio" name="toCase" value="upper"/>
            <%=umbraco.ui.Text("templateEditor", "uppercase")%>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="pp_encode" runat="server">
            <input type="radio" name="urlEncode" value=""/>
            <%=umbraco.ui.Text("none")%>
            <input type="radio" name="urlEncode" value="url"/>
            <%=umbraco.ui.Text("templateEditor","urlEncode")%>
            <input type="radio" name="urlEncode" value="html"/>
            <%=umbraco.ui.Text("templateEditor", "htmlEncode")%>
            <br />
            <span class="guiDialogTiny">
                <%=umbraco.ui.Text("templateEditor", "urlEncodeHelp")%>
            </span>
        </cc1:PropertyPanel>
        </cc1:Pane>

        <cc1:Pane runat="server" Title="Modify output">
        <cc1:PropertyPanel ID="pp_insertBefore" runat="server">
            <input type="text" size="40" name="insertTextBefore" class="guiInputTextTiny"/><br />
            <span class="guiDialogTiny">
                <%=umbraco.ui.Text("templateEditor", "insertedBefore")%>
            </span>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_insertAfter" runat="server">
            <input type="text" size="40" name="insertTextAfter" class="guiInputTextTiny"/><br />
            <span class="guiDialogTiny">
                <%=umbraco.ui.Text("templateEditor", "insertedAfter")%>
            </span>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="pp_convertLineBreaks" runat="server">
            <input type="checkbox" name="convertLineBreaks" value="true"/>  <%=umbraco.ui.Text("templateEditor", "convertLineBreaks")%>
            
            <br />
            <span class="guiDialogTiny">
                <%=umbraco.ui.Text("templateEditor", "convertLineBreaksHelp")%>
            </span>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="pp_removePTags" runat="server">
            <input type="checkbox" name="stripParagraph" value="true"/>  <%=umbraco.ui.Text("templateEditor", "removeParagraph")%>
            <br />
            <span class="guiDialogTiny"><%=umbraco.ui.Text("templateEditor", "removeParagraphHelp")%>
            </span>
        </cc1:PropertyPanel>
    </cc1:Pane>
    </div>
    
    <div class="umb-panel-footer">
        <div class="btn-toolbar umb-btn-toolbar">
            <a id="cancelButton" href="#" class="btn btn-link">
                <%=umbraco.ui.Text("general", "cancel", UmbracoUser)%></a> 
    
            <input id="submitButton" type="button" name="gem" class="btn btn-primary" value="<%=umbraco.ui.Text("insert")%>" />
        </div>
    </div>

    </div>
    
</asp:Content>
