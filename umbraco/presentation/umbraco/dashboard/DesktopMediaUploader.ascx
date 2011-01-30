<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DesktopMediaUploader.ascx.cs" Inherits="umbraco.presentation.umbraco.dashboard.DesktopMediaUploader" %>
<script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/swfobject/2.1/swfobject.js"></script>
<div class="propertyDiv" style="text-align: center;margin-top: 10px;">
    <p><strong>Desktop Media Uploader</strong> is a small desktop application that you can install on your computer which allows you to easily upload media items directly to the media section.</p>
    <p>The badge below will auto configure itself based upon whether you already have <strong>Desktop Media Uploader</strong> installed or not.</p>
    <p>Just click the <strong>Install Now / Upgrade Now / Launch Now</strong> link to perform that action.</p>
    <p>
        <div id="flashcontent" style="display:block;margin-bottom: 10px;">
            Download <a href="<%= FullyQualifiedAppPath %>umbraco/dashboard/air/desktopmediauploader.air">Desktop Media Uploader</a> now.<br /><br /><span id="AIRDownloadMessageRuntime">This application requires Adobe&#174;&nbsp;AIR&#8482; to be installed for <a href="http://airdownload.adobe.com/air/mac/download/latest/AdobeAIR.dmg">Mac OS</a> or <a href="http://airdownload.adobe.com/air/win/download/latest/AdobeAIRInstaller.exe">Windows</a>.
        </div>
    </p>
    <script type="text/javascript">
    // <![CDATA[

        var flashvars = {
            appid: "org.umbraco.DesktopMediaUploader",
            appname: "Desktop Media Uploader",
            appversion: "v2.0.4",
            appurl: "<%= FullyQualifiedAppPath %>umbraco/dashboard/air/desktopmediauploader.air",
            applauncharg: "<%= AppLaunchArg %>",
            image: "/umbraco/dashboard/images/dmu-badge.jpg?v=2.0.4",
            airversion: "2.0"
        };
        var params = {
            menu: "false"
        };
        var attributes = {
            style: "margin-bottom:10px;"
        };

        swfobject.embedSWF("/umbraco/dashboard/swfs/airinstallbadge.swf", "flashcontent", "215", "180", "9.0.115", "/umbraco/dashboard/swfs/expressinstall.swf", flashvars, params, attributes);

    // ]]>
    </script>
    <p>For a quick guide on how to use the <strong>Desktop Media Uploader</strong>, <a href="http://screenr.com/vXr" target="_blank">checkout this video</a>.</p>
</div>