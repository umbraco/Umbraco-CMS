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
    
    <div class="umb-dialog-body">

    <cc1:Feedback ID="feedback" runat="server" />    
    <div id="komask"></div>
    
         <cc1:Pane runat="server" ID="pane_language">
            <cc1:PropertyPanel runat="server" ID="prop_language">
                <select class="umb-editor umb-dropdown" name="language" data-bind="options: languages, optionsText: 'Code', optionsValue: 'Id', value: language, optionsCaption: '<%=umbraco.ui.Text("assignDomain", "inherit") %>    '"></select>
             <!--   <small class="help-inline"><%=umbraco.ui.Text("assignDomain", "setLanguageHelp") %></small>-->
            </cc1:PropertyPanel>
        </cc1:Pane>
    

        <cc1:Pane runat="server" ID="pane_domains">
            <cc1:PropertyPanel runat="server">
                <table class="table domains" data-bind="visible: domains().length > 0">
                    <thead>
                        <tr>
                            <th><%=umbraco.ui.Text("assignDomain", "domain") %></th>
                            <th><%=umbraco.ui.Text("assignDomain", "language") %></th>
                            <th />
                        </tr>
                    </thead>
                    <tbody data-bind="foreach: domains">
                        <tr>
                            <td valign="top"><input class="domain" data-bind="value: Name, uniqueName: true"  /><input type="hidden" value="0" /></td>
                            <td valign="top"><select class="language" data-bind="options: $parent.languages, optionsText: 'Code', optionsValue: 'Id', value: Lang, uniqueName: true"></select></td>
                            <td valign="top"><a href="#" class="btn btn-danger" data-bind="click: $parent.removeDomain"><i class="icon icon-trash"></i></a></td>
                        </tr>
                    </tbody>
                </table>
            </cc1:PropertyPanel>
            
            <cc1:PropertyPanel runat="server">
                <small data-bind="visible: domains().length == 0" class="help-inline"><%=umbraco.ui.Text("assignDomain", "domainHelp") %></small>    
                <button class="btn" data-bind="click: addDomain"><%=umbraco.ui.Text("assignDomain", "addNew") %></button>        
            </cc1:PropertyPanel>

               
        </cc1:Pane>

        </div>



        <div runat="server" ID="p_buttons" class="umb-dialog-footer btn-toolbar umb-btn-toolbar">
             <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("general", "cancel")%></a>  
             <button class="btn btn-primary" id="btnSave"><%=umbraco.ui.Text("buttons", "save") %></button>
        </div>

</asp:Content>