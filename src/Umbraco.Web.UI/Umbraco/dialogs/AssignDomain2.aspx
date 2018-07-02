<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" Codebehind="AssignDomain2.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.AssignDomain2" %>
<%@ Import Namespace="Umbraco.Web" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="cc1" Namespace="Umbraco.Web._Legacy.Controls" Assembly="Umbraco.Web" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
<umb:JsInclude runat="server" FilePath="Dialogs/AssignDomain2.js" PathNameAlias="UmbracoClient" />
<umb:JsInclude runat="server" FilePath="PunyCode/punycode.min.js" PathNameAlias="UmbracoClient" />
<umb:JsInclude runat="server" FilePath="Application/JQuery/jquery.validate.min.js" PathNameAlias="UmbracoClient" />
<umb:CssInclude runat="server" FilePath="Dialogs/AssignDomain2.css" PathNameAlias="UmbracoClient" />
<script type="text/javascript">
    (function ($) {
        $(document).ready(function () {
            var dialog = new Umbraco.Dialogs.AssignDomain2({
                nodeId: <%=GetNodeId()%>,
                restServiceLocation: '<%=GetRestServicePath() %>',
                invalidDomain: '<%=Services.TextService.Localize("assignDomain/invalidDomain") %>',
                duplicateDomain: '<%=Services.TextService.Localize("assignDomain/duplicateDomain") %>',
                <asp:Literal runat="server" ID="data" />
            });
        dialog.init();
    });
    })(jQuery);
</script>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">

    <div class="umb-dialog-body">

    <cc1:Feedback ID="feedback" runat="server" />
    <div id="komask"></div>

         <cc1:Pane runat="server" ID="pane_language" cssClass="hide">
            <cc1:PropertyPanel runat="server" ID="prop_language">
                <select class="umb-property-editor umb-dropdown" name="language" data-bind="options: languages, optionsText: 'Code', optionsValue: 'Id', value: language, optionsCaption: '<%=Services.TextService.Localize("assignDomain/inherit") %>    '"></select>
             <!--   <small class="help-inline"><%=Services.TextService.Localize("assignDomain/setLanguageHelp") %></small>-->
            </cc1:PropertyPanel>
        </cc1:Pane>


        <cc1:Pane runat="server" ID="pane_domains">
            <small class="help-inline"><%=Services.TextService.Localize("assignDomain/domainHelp") %></small>
            <cc1:PropertyPanel runat="server">
                <table class="table domains" data-bind="visible: domains().length > 0">
                    <thead>
                        <tr>
                            <th><%=Services.TextService.Localize("assignDomain/domain") %></th>
                            <th><%=Services.TextService.Localize("assignDomain/language") %></th>
                            <th />
                        </tr>
                    </thead>
                    <tbody data-bind="foreach: domains">
                        <tr>
                            <td valign="top"><input class="domain duplicate" data-bind="value: Name, uniqueName: true"  /><input type="hidden" value="" data-bind="uniqueName: true"/></td>
                            <td valign="top"><select class="language" data-bind="options: $parent.languages, optionsText: 'Code', optionsValue: 'Id', value: Lang, uniqueName: true"></select></td>
                            <td valign="top"><a href="#" class="btn btn-danger" data-bind="click: $parent.removeDomain"><i class="icon icon-trash"></i></a></td>
                        </tr>
                    </tbody>
                </table>
            </cc1:PropertyPanel>

            <cc1:PropertyPanel runat="server">
                <button class="btn" data-bind="click: addDomain"><%=Services.TextService.Localize("assignDomain/addNew") %></button>
            </cc1:PropertyPanel>


        </cc1:Pane>

        </div>



        <div runat="server" ID="p_buttons" class="umb-dialog-footer btn-toolbar umb-btn-toolbar">
             <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=Services.TextService.Localize("general/cancel")%></a>
             <button class="btn btn-primary" id="btnSave"><%=Services.TextService.Localize("buttons/save") %></button>
        </div>

</asp:Content>