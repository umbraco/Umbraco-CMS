<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>
<link href="/umbraco_client/propertypane/style.css" rel="stylesheet" />
<div class="dashboardWrapper">
    <h2>
        Start here</h2>
    <img src="/umbraco/dashboard/images/logo32x32.png" alt="Umbraco" class="dashboardIcon" />
    <h3>
        Get started with Media right now</h3>
    <p>
        Use the tool below to upload a ZIP file of your images or documents to a media folder.
    </p>
    <h3>
        Follow these steps:</h3>
    <div class="dashboardColWrapper">
        <div class="dashboardCols">
            <div class="dashboardCol">
                <ul>
                    <li>Create a media folder by right-clicking on the Media root folder, selecting Create,
                        then give your folder a name, select the Media Type Folder, and click create</li>
                    <li>Select the created folder by click the Choose link</li>
                    <li>Use the Browse button below to select a ZIP file containing your images (you can
                        even organize them into folders and the tool will create these for you)</li>
                    <li>Click the Upload zip file button</li>
                    <li>Refresh the Media section by right-clicking the Media root folder and selecting
                        Reload Nodes</li>
                </ul>
            </div>
        </div>
    </div>
</div>
