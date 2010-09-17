<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StartupDeveloperDashboard.ascx.cs" Inherits="dashboardUtilities.StartupDeveloperDashboard" %>
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
            url: 'dashboard/feedproxy.aspx?url=http://umbraco.org/documentation/videos/for-developers/foundation/feed',
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

<asp:Panel ID="startPanel" runat="server">
<div class="propertypane">
<div class="guiDialogNormal" style="margin: 10px">
        <h2>Start here</h2>
        <h3>This section contains the tools to add advanced features to your Umbraco site</h3>
        <p>
        From here you can explore and install packages, create macros, add data types, and much more.  Start by exploring the below links or videos.
        </p>
        <ul>
        <li>Find the answers to your Umbraco questions on our <a href="http://our.umbraco.org/wiki" target="_blank">Community Wiki</a></li>
        <li>Ask a question in the <a href="http://our.umbraco.org/" target="_blank">Community Forum</a></li>
        <li>Find an add-on <a href="http://our.umbraco.org/projects" target="_blank">package</a> to help you get going quickly</li>
        <li>Watch our <a href="http://umbraco.tv" target="_blank">tutorial videos</a> (some are free, some require a subscription)</li>
        <li>Find out about our <a href="http://umbraco.org/products" target="_blank">Pro Tools and Support</a></li>
        <li>Find out about real-life <a href="http://umbraco.org/training/training-schedule" target="_blank">training and certification</a> opportunities</li>
        </ul>
</div>
</div>
</asp:Panel>

<asp:Panel ID="learnPanel" runat="server">
<div class="propertypane">
  <div class="guiDialogNormal" style="margin: 10px">
    <h2>Watch and learn</h2>
        <p>
            Want to master Umbraco Macros and more? Spend a couple of minutes learning some best practices
            by watching one of these videos about using Umbraco. And visit <a href="http://umbraco.tv"
                target="_blank">umbraco.tv</a> for even more Umbraco videos</p>
    <div id="latestformvids">
            Loading...
    </div>
  </div>
</div>
</asp:Panel>

<asp:Panel ID="hidePanel" runat="server">
<div class="propertypane">
<div class="guiDialogNormal" style="margin: 10px">
        <p>
                Check here to hide this dashboard in the future <asp:CheckBox ID="hideCheckBox" 
                runat="server" oncheckedchanged="hideCheckBox_CheckedChanged"></asp:CheckBox>
        </p>
</div>
</div>
</asp:Panel>
