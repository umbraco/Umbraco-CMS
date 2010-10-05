<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="StartupMemberDashboard.ascx.cs" Inherits="dashboardUtilities.StartupMemberDashboard" %>
<%@ Register src="/umbraco/members/membersearch.ascx" tagname="membersearch" tagprefix="uc1" %>
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
            url: 'dashboard/feedproxy.aspx?url=http://umbraco.org/documentation/videos/for-site-builders/members/feed',
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
        <h3>Get started with Members right now</h3>
        <p>
        Use the tool below to search for an existing member.
        </p>
        <ul>
        <li><strong>More about members:</strong></li>
        <li>Learn about how to protect pages of your site from <a href="http://our.umbraco.org/wiki/reference/umbraco-client/context-menus/public-access" target="_blank">this Wiki entry</a></li>
        </ul>  
</div>
</div>
</asp:Panel>

<uc1:membersearch ID="memberSearch1" runat="server" />

<asp:Panel ID="learnPanel" runat="server">
<div class="propertypane">
  <div class="guiDialogNormal" style="margin: 10px">
    <h2>Watch and learn</h2>
        <p>
            Want to master Umbraco Members? Spend a couple of minutes learning some best practices
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
                runat="server" oncheckedchanged="hideCheckBox_CheckedChanged" AutoPostBack="true"></asp:CheckBox>
        </p>
</div>
</div>
</asp:Panel>


