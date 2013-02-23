<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ExamineManagement.ascx.cs" Inherits="Umbraco.Web.UI.Umbraco.Dashboard.ExamineManagement" %>
<%@ Import Namespace="Umbraco.Core" %>
<%@ Register TagPrefix="cc1" Namespace="Umbraco.Web.UI.Controls" Assembly="umbraco" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>


<umb:JsInclude ID="JsInclude1" runat="server" FilePath="Dashboards/ExamineManagement.js" PathNameAlias="UmbracoClient" />
<umb:CssInclude ID="CssInclude1" runat="server" FilePath="Dashboards/ExamineManagement.css" PathNameAlias="UmbracoClient" />

<script type="text/javascript">

    (function ($) {
        $(document).ready(function () {
            var mgmt = new Umbraco.Dashboards.ExamineManagement({
                container: $("#examineManagement"),
                restServiceLocation: "<%=Url.GetExamineManagementServicePath() %>"
            });
            mgmt.init();
        });
    })(jQuery);

</script>


<div id="examineManagement">

    <div data-bind="visible: loading()">
        <cc1:ProgressBar runat="server" ID="ProgBar1" Text="Loading..." />
    </div>
    <div data-bind="html: summary"></div>

</div>
