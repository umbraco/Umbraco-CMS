<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Register Assembly="controls" Namespace="umbraco.uicontrols" TagPrefix="umb" %>
<%@ Register Assembly="ClientDependency.Core" Namespace="ClientDependency.Core.Controls" TagPrefix="umb" %>

<umb:CssInclude runat="server" FilePath="propertypane/style.css" PathNameAlias="UmbracoClient" />

<div class="dashboardWrapper">
    <h2>Start here</h2>
    <img src="./dashboard/images/logo32x32.png" alt="Umbraco" class="dashboardIcon" />
    <h3>Get started with Members right now</h3>
    <p>Use the tool below to search for an existing member.</p>
    <h3>More about members</h3>
    <div class="dashboardColWrapper">
        <div class="dashboardCols">
            <div class="dashboardCol">
                <ul>
                    <li>Learn about how to protect pages of your site from <a href="http://our.umbraco.org/wiki/reference/umbraco-client/context-menus/public-access" target="_blank">this Wiki entry</a></li>
                </ul>
            </div>
        </div>
    </div>
</div>
