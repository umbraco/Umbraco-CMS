<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" CodeBehind="Publish.aspx.cs" AutoEventWireup="True" Inherits="Umbraco.Web.UI.Umbraco.Dialogs.Publish" %>

<%@ Import Namespace="Umbraco.Core" %>
<%@ Import Namespace="Umbraco.Web" %>
<%@ Import Namespace="Umbraco.Web.Mvc" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <umb:JsInclude ID="JsInclude1" runat="server" FilePath="js/umbracoCheckKeys.js" PathNameAlias="UmbracoRoot" />
    <umb:JsInclude ID="JsInclude2" runat="server" FilePath="Dialogs/PublishDialog.js" PathNameAlias="UmbracoClient" />
    <umb:CssInclude ID="CssInclude1" runat="server" FilePath="Dialogs/PublishDialog.css" PathNameAlias="UmbracoClient" />

    <script type="text/javascript">
        //NOTE: These variables are required for the legacy UmbracoCheckKeys.js
        var functionsFrame = this;
        var tabFrame = this;
        var isDialog = true;
        var submitOnEnter = true;

        (function ($) {
            $(document).ready(function () {
                Umbraco.Dialogs.PublishDialog.getInstance().init({
                    restServiceLocation: "<%= Url.GetBulkPublishServicePath() %>",
                    documentId: <%= DocumentId %>,
                    documentPath: '<%= DocumentPath %>'
                });
            });
        })(jQuery);
    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <div id="container" >

        <div class="propertyDiv" data-bind="visible: processStatus() == 'init'">
            <p>
                <%= Services.TextService.Localize("publish/publishHelp", new[] { PageName}) %>
            </p>

            <div>
                <input type="checkbox" id="publishAllCheckBox" data-bind="checked: publishAll" />
                <label for="publishAllCheckBox">
                    <%=Services.TextService.Localize("publish/publishAll", new[] { PageName}) %>
                </label>
            </div>

            <div id="includeUnpublished">
                <input type="checkbox" id="includeUnpublishedCheckBox" data-bind="checked: includeUnpublished, attr: { disabled: !publishAll() }" />
                <label for="includeUnpublishedCheckBox" data-bind="css: { disabled: !publishAll() }">
                    <%=Services.TextService.Localize("publish/includeUnpublished") %>
                </label>
            </div>
            
            

        </div>

         <div class="umb-dialog-footer btn-toolbar umb-btn-toolbar"  data-bind="visible: processStatus() == 'init'">
            <a href="#" class="btn btn-link" data-bind="click: closeDialog">
                <%=Services.TextService.Localize("general/cancel")%>
            </a>
             <button id="ok" class="btn btn-primary" data-bind="click: startPublish">
                <%=Services.TextService.Localize("content/publish")%>
            </button>
        </div>

        <div id="animDiv" class="propertyDiv" data-bind="visible: processStatus() == 'publishing'">
            <div>
                <p>
                    <%=Services.TextService.Localize("publish/inProgress")%>
                </p>
                <cc1:ProgressBar runat="server" ID="ProgBar1" />
                <br />
            </div>
        </div>


        <div id="feedbackMsg" data-bind="visible: processStatus() == 'complete'">
            <div data-bind="css: { success: isSuccessful(), error: !isSuccessful() }">
                <span data-bind="text: resultMessage, visible: resultMessages().length == 0"></span>
                <ul data-bind="foreach: resultMessages, visible: resultMessages().length > 1">
                    <li data-bind="text: message"></li>
                </ul>
            </div>
             <p>
                 <a href='#' class="btn" data-bind="click: closeDialog"><%=Services.TextService.Localize("closeThisWindow") %></a>
             </p>
        </div> 
       

    </div>

</asp:Content>
