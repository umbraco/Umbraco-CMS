<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" Codebehind="AssignDomain2.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.AssignDomain2" %>
<%@ Import Namespace="Umbraco.Web" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
<umb:JsInclude runat="server" FilePath="Dialogs/AssignDomain2.js" PathNameAlias="UmbracoClient" />
<umb:JsInclude runat="server" FilePath="Application/JQuery/jquery.validate.min.js" PathNameAlias="UmbracoClient" />
<umb:CssInclude runat="server" FilePath="Dialogs/AssignDomain2.css" PathNameAlias="UmbracoClient" />
<script type="text/javascript">

    (function ($) {
        $(document).ready(function () {
            var dialog = new Umbraco.Dialogs.AssignDomain2({
                nodeId: <%=GetNodeId()%>,
                restServiceLocation: '<%=GetRestServicePath() %>',
                invalidDomain: '<%=umbraco.ui.Text("assignDomain", "invalidDomain") %>',
                duplicateDomain: '<%=umbraco.ui.Text("assignDomain", "duplicateDomain") %>',
                <asp:Literal runat="server" ID="data" />
            });
            dialog.init();
        });
    })(jQuery);

</script>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
    
    <cc1:Feedback ID="feedback" runat="server" />
    
    <div id="komask"></div>
    <div>        
        <cc1:Pane runat="server" ID="pane_language">
            <cc1:PropertyPanel runat="server" ID="prop_language">
                <select class="language" name="language" data-bind="options: languages, optionsText: 'Code', optionsValue: 'Id', value: language, optionsCaption: '<%=umbraco.ui.Text("assignDomain", "inherit") %>'"></select>
                <br /><small><%=umbraco.ui.Text("assignDomain", "setLanguageHelp") %></small>
            </cc1:PropertyPanel>
        </cc1:Pane>
    
        <cc1:Pane runat="server" ID="pane_domains">
            <cc1:PropertyPanel runat="server">
                <table class="domains" data-bind="visible: domains().length > 0">
                    <thead>
                        <tr>
                            <th><%=umbraco.ui.Text("assignDomain", "domain") %></th>
                            <th><%=umbraco.ui.Text("assignDomain", "language") %></th>
                            <th />
                        </tr>
                    </thead>
                    <tbody data-bind="foreach: domains">
                        <tr>
                            <td valign="top"><input class="domain duplicate" data-bind="value: Name, uniqueName: true" /><input type="hidden" value="0" /></td>
                            <td valign="top"><select class="language" data-bind="options: $parent.languages, optionsText: 'Code', optionsValue: 'Id', value: Lang, uniqueName: true"></select></td>
                            <td valign="top"><a href="#" class="remove" data-bind="click: $parent.removeDomain"><%=umbraco.ui.Text("assignDomain", "remove") %></a></td>
                        </tr>
                    </tbody>
                </table>
                <table class="addDomain">
                    <tr>
                        <td valign="top"><button data-bind="click: addDomain"><%=umbraco.ui.Text("assignDomain", "addNew") %></button></td>
                        <td class="help"><small><%=umbraco.ui.Text("assignDomain", "domainHelp") %></small></td>
                    </tr>
                </table>
            </cc1:PropertyPanel> 
        </cc1:Pane>

        <p>
            <asp:PlaceHolder runat="server" ID="phSave">
                <button id="btnSave"><%=umbraco.ui.Text("buttons", "save") %></button>
                <em><%=umbraco.ui.Text("general", "or")%></em>
            </asp:PlaceHolder>
            <a href="#" style="color: #0000ff;" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("general", "cancel")%></a>  
        </p>

    </div>
</asp:Content>