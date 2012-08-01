<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.ascx.cs" Inherits="Our.Umbraco.uGoLive.Web.Umbraco.Plugins.uGoLive.Dashboard" %>
<%@ Import Namespace="umbraco.IO" %>
<%@ Import Namespace="Our.Umbraco.uGoLive.Web" %>
<link href="../umbraco_client/propertypane/style.css" rel="stylesheet" />
<link href="plugins/uGoLive/Dashboard.css" rel="stylesheet" />
<script type="text/javascript" src="plugins/uGoLive/jquery.tmpl.js"></script>
<script type="text/javascript" src="plugins/uGoLive/knockout-1.2.1.js"></script>
<script type="text/javascript" src="plugins/uGoLive/Dashboard.js"></script>
<script type="text/javascript">

    (function ($) {

    	$(function () {

    	    Our.Umbraco.uGoLive.Dashboard.init({
    	        checkDefs: <%= Checks.ToJsonString() %>,
    			basePath: '<%= IOHelper.ResolveUrl(SystemPaths.Base) %>',
    			umbracoPath: '<%= IOHelper.ResolveUrl(SystemPaths.Umbraco) %>'
    		});

    	});

    })(jQuery)  

</script>

<div class="uGoLive">
    <div class="propertypane">
        <div>
            <div class="propertyItem">
                <div class="dashboardWrapper">
                    <h2>uGoLive Checklist</h2>
                    <img src="plugins/uGoLive/icon.png" alt="uGoLive" class="dashboardIcon" />
                    <p>The uGoLive checklist is a checklist of the most widely accredited best practises when deploying an Umbraco website. uGoLive performs a complete system check against these practises, and highlights any areas that need attention.</p>
                    <button id="btnRunChecks" data-bind="click: checkAll, text: checkAllText, css: { disabled: queuedChecks().length > 0 }"></button>
                    <div class="checks" data-bind="template: { name: 'uGoLiveGroup', foreach: checkGroups }"></div>
                </div>
            </div>
        </div>
    </div>
</div>

<script id="uGoLiveGroup" type="text/html">
    <h3 data-bind="text: name + ':'"></h3>
    <div class="propertypane">
        <div data-bind="template: { name: 'uGoLiveCheck', foreach: checks }"></div>
        <div class="propertyPaneFooter">-</div>
    </div>
</script>

<script id="uGoLiveCheck" type="text/html">
    <div class="propertyItem">
	    <div class="propertyItemheader">
		    <span data-bind="text: name"></span><br />
            <small data-bind="text: description"></small>
	    </div>
        <div class="propertyItemContent">
		    <a href="#" title="Run Check" class="check" data-bind="click: check, css: { disabled: status() == 'Checking' || status() == 'Queued' }"><img src="plugins/uGoLive/run.png" alt="Run Check" /></a>
            <a href="#" title="Rectify Check" class="rectify" data-bind="click: rectify, css: { disabled: !(canRectify && status() == 'Failed') }"><img src="plugins/uGoLive/cog.png" alt="Rectify Check" /></a>
            <span class="status ugl${ status }" data-bind="html: message"></span>
	    </div>
    </div>
</script>