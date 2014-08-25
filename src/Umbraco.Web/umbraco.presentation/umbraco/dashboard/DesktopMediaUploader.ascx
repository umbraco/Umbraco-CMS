<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DesktopMediaUploader.ascx.cs" Inherits="umbraco.presentation.umbraco.dashboard.DesktopMediaUploader" %>
<%@ Register Assembly="ClientDependency.Core" Namespace="ClientDependency.Core.Controls" TagPrefix="umb" %>

<umb:CssInclude runat="server" FilePath="propertypane/style.css" PathNameAlias="UmbracoClient" />

<div class="dashboardWrapper">
    <h2>Desktop Media Uploader</h2>
    <img src="./dashboard/images/dmu.png" alt="Umbraco" class="dashboardIcon" />
    <p><strong>Desktop Media Uploader</strong> is a small desktop application that you can install on your computer which allows you to easily upload media items directly to the media section.</p>
    <div class="dashboardColWrapper">
        <div class="dashboardCols">
            <div class="dashboardCol">
                <asp:Panel ID="Panel1" runat="server"></asp:Panel>
                <asp:Panel ID="Panel2" runat="server">
                    <div id="dmu-badge">
                        <p>
                            <span id="Span1">This application requires Adobe&#174;&nbsp;AIR&#8482; to be installed for <a href="http://airdownload.adobe.com/air/mac/download/latest/AdobeAIR.dmg">Mac OS</a> or <a href="http://airdownload.adobe.com/air/win/download/latest/AdobeAIRInstaller.exe">Windows</a></span>.<br />
                            After Air is installed you can install the <a href="<%= FullyQualifiedAppPath %>umbraco/Dashboard/air/DesktopMediaUploader.air">Desktop Media Uploader by clicking here</a>.
                        </p>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>
</div>
