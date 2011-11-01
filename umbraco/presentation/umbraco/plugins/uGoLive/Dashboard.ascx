<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.ascx.cs" Inherits="Our.Umbraco.uGoLive.Web.Umbraco.Plugins.uGoLive.Dashboard" %>
<%@ Import Namespace="umbraco.IO" %>
<style type="text/css"> 
    .uGoLive .propertyItemContent img { vertical-align: middle; margin-right: 5px; } 
    .uGoLive .propertypane .propertypane { padding: 10px 10px 0px; } 
    .uGoLive .dashboardWrapper { padding: 5px 5px 5px 5px; overflow: hidden; color: #333; }
    .uGoLive .dashboardWrapper h2 { margin-top: 0; padding-bottom: 10px; border-bottom: 1px solid #CCC; padding-left: 37px; line-height: 32px; }
    .uGoLive .dashboardWrapper .dashboardIcon { position: absolute; top: 10px; left: 10px; }
    .uGoLive .dashboardWrapper h3 { margin-bottom: 10px; }
    .uGoLive .dashboardWrapper p { line-height: 1.4em; font-size: 1.1em; margin-top: 0; } 
    .uGoLive #btnRunChecks { color: #fff; background: #f26e20; padding: 10px; border: 0; font-weight: bold; cursor: pointer; }
    .uGoLive #btnRunChecks.disabled { background: #f9b790; }
    .uGoLive a.disabled { cursor: default; opacity:0.3; filter:alpha(opacity=30); }
</style>
<script type="text/javascript" src="/umbraco/plugins/uGoLive/Dashboard.js"></script>
<script type="text/javascript">

    (function ($) {

    	$(function () {

    		Our.Umbraco.uGoLive.Dashboard.init({
    			basePath: '<%= IOHelper.ResolveUrl(IOHelper.returnPath("umbracoBaseDirectory", "~/base")) %>',
    			umbracoPath: '<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>'
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
                    <img src="/umbraco/plugins/uGoLive/icon.png" alt="uGoLive" class="dashboardIcon" />
                    <p>The uGoLive checklist is a checklist of the most widely accredited best practises when deploying an Umbraco website. uGoLive performs a complete system check against these practises, and highlights any areas that need attention.</p>
                    <button id="btnRunChecks">Run All Checks</button>
                    <div class="checks">
                        <asp:PlaceHolder ID="phChecks" runat="server" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>