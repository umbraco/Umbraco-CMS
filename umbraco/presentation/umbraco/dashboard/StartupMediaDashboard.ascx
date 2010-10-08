<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="StartupMediaDashboard.ascx.cs" Inherits="dashboardUtilities.StartupMediaDashboard" %>
<%@ Register src="zipupload.ascx" tagname="zipupload" tagprefix="uc1" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>
<script type="text/javascript" src="/umbraco/dashboard/scripts/jquery.jfeed.pack.js"></script>
<link href="/umbraco_client/propertypane/style.css" rel="stylesheet" />
<style type="text/css">
    .formList, .tvList
    {
        list-style: none;
        display: block;
        margin: 10px;
        padding: 0px;
    }
    .formList li
    {
        padding: 0px 0px 15px 0px;
        list-style: none;
        margin: 0px;
    }
    .formList li a.form
    {
        font-size: 1em;
        font-weight: bold;
        color: #000;
        display: block;
        height: 16px;
        padding: 2px 0px 0px 30px;
        background: url(images/umbraco/icon_form.gif) no-repeat 2px 2px;
    }
    .formList li small
    {
        display: block;
        padding-left: 30px;
        height: 10px;
    }
    .formList li small a
    {
        display: block;
        float: left;
        padding-right: 10px;
    }
    .tvList .tvitem
    {
        font-size: 11px;
        text-align: center;
        display: block;
        width: 130px;
        height: 158px;
        margin: 0px 20px 20px 0px;
        float: left;
        overflow: hidden;
    }
    .tvList a
    {
        overflow: hidden;
        display: block;
    }
    .tvList .tvimage
    {
        display: block;
        height: 120px;
        width: 120px;
        overflow: hidden;
        border: 1px solid #999;
        margin: auto;
        margin-bottom: 10px;
    }
    .contourLabel
    {
        clear: left;
        float: left;
        font-weight: bold;
        padding-bottom: 10px;
        padding-right: 10px;
        width: 130px;
    }
    .contourInput
    {
        color: #333333;
        font-family: Trebuchet MS,Lucida Grande,verdana,arial;
        font-size: 12px;
        padding: 2px;
        width: 250px;
    }
</style>
<script type="text/javascript">

    jQuery(function () {

        jQuery.ajax({
            type: 'GET',
            url: 'dashboard/feedproxy.aspx?url=http://umbraco.org/documentation/videos/getting-started/feed',
            dataType: 'xml',
            success: function (xml) {


                var html = "<div class='tvList'>";

                jQuery('item', xml).each(function () {

                    html += '<div class="tvitem">'
                    + '<a target="_blank" href="'
                    + jQuery(this).find('link').eq(0).text()
                    + '">'
                    + '<div class="tvimage" style="background: url(' + jQuery(this).find('thumbnail').attr('url') + ') no-repeat center center;">'
                    + '</div>'
                    + jQuery(this).find('title').eq(0).text()
                    + '</a>'
                    + '</div>';
                });

                html += "</div>";

                jQuery('#latestformvids').html(html);
            }

        });



    });


</script>

<umb:Pane runat="server" ID="startPanel">
<umb:PropertyPanel runat="server" ID="startPP">
        <h2>Start here</h2>
        <h3>Get started with Media right now</h3>
        <p>
        Use the tool below to upload a ZIP file of your images or documents to a media folder.
        </p>
        <h4>Follow these steps:</h4>
        <div style="float:left; width:5%;">
            <img src="/umbraco/dashboard/images/logo.gif" alt="Umbraco Start Up!" />
        </div>
        <div style="float:right; width:95%;">
            <ul>
            <li>Create a media folder by right-clicking on the Media root folder, selecting Create, then give your folder a name, select the Media Type Folder, and click create</li>
            <li>Select the created folder by click the Choose link</li>
            <li>Use the Browse button below to select a ZIP file containing your images (you can even organize them into folders and the tool will create these for you)</li>
            <li>Click the Upload zip file button</li>
            <li>Refresh the Media section by right-clicking the Media root folder and selecting Reload Nodes</li>
            </ul>  
        </div>
</umb:PropertyPanel>
</umb:Pane>

<umb:Pane runat="server" ID="zipUploadPanel">
<umb:PropertyPanel runat="server" ID="zipUploadPP">
    <h2>Upload Files</h2>
        <uc1:zipupload ID="zipupload1" runat="server" />
</umb:PropertyPanel>
</umb:Pane>

<umb:Pane runat="server" ID="learn">
<umb:PropertyPanel runat="server" ID="learnPP">
    <h2>Watch and learn</h2>
        <p>
            Want to master Umbraco Media? Spend a couple of minutes learning some best practices
            by watching one of these videos about using Umbraco. And visit <a href="http://umbraco.tv"
                target="_blank">umbraco.tv</a> for even more Umbraco videos</p>
    <div id="latestformvids">
            Loading...
    </div>
</umb:PropertyPanel>
</umb:Pane>

<umb:Pane ID="hidePanel" runat="server">
<umb:PropertyPanel runat="server" ID="hidePP">
        <p>
                Check here to hide this dashboard in the future <asp:CheckBox ID="hideCheckBox" 
                runat="server" oncheckedchanged="hideCheckBox_CheckedChanged" AutoPostBack="true"></asp:CheckBox>
        </p>
</umb:PropertyPanel>
</umb:Pane>


