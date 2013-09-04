<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FormsDashboard.ascx.cs"
    Inherits="Umbraco.Forms.UI.Dashboard.FormsDashboard" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<script type="text/javascript" src="plugins/umbracocontour/scripts/jquery.jfeed.pack.js"></script>

<link href="/umbraco_client/propertypane/style.css" rel="stylesheet">
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

    jQuery(function() {

        jQuery.ajax({
            type: 'GET',
            url: 'plugins/umbracocontour/feedproxy.aspx?url=http://umbraco.org/documentation/videos/umbraco-pro/contour/feed',
            dataType: 'xml',
            success: function(xml) {


                var html = "<div class='tvList'>";

                jQuery('item', xml).each(function() {

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

<umb:Pane ID="register" runat="server" Text="Register Contour" Visible="false">
    <umb:PropertyPanel ID="PropertyPanel6" runat="server">
        <h3>Thank you for trying out Umbraco Contour!</h3>
        <p> To purchase Umbraco Contour, simply go to the <a target="_blank" href="http://umbraco.org/redir/contourOrderFromTrial">
            Umbraco online shop</a> and you're up and running in minutes!</p>
            
        <p>If you've already purchased Umbraco Contour, you can install your license 
            automatically by using your <strong>umbraco.org profile credentials</strong> below.</p>
            
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
            
            <umb:Feedback ID="licenseFeedback" runat="server" />
            
            <asp:Panel ID="licensingLogin" runat="server">
                    <umb:PropertyPanel runat="server" Text="E-mail">
                        <asp:TextBox ID="login" CssClass="guiInputText guiInputStandardSize" runat="server"></asp:TextBox>
                    </umb:PropertyPanel>
                    
                    <umb:PropertyPanel runat="server" Text="Password">
                        <asp:TextBox ID="password" TextMode="Password" CssClass="guiInputText guiInputStandardSize" runat="server"></asp:TextBox>
                    </umb:PropertyPanel>
                    
                    <umb:PropertyPanel runat="server" Text=" ">
                    <p>
                        <asp:Button ID="getLicensesButton" runat="server" Text="Get My licenses From umbraco.org" OnClick="getLicensesButton_Click" />  
                    </p>
                    </umb:PropertyPanel>
            </asp:Panel>
                
            <asp:Panel ID="listLicenses" runat="server" Visible="false">
                    <umb:PropertyPanel runat="server">
                        <h4>
                            Following licenses was found via your profile on umbraco.org:
                        </h4>
                        <p>    
                        <asp:RadioButtonList ID="licensesList" runat="server" />
                        </p>                        
                        <p>
                        <asp:Button ID="chooseLicense" runat="server" Text="Use or configure this license" OnClick="chooseLicense_Click" />
                        </p>
                    </umb:PropertyPanel>
                    
            </asp:Panel>
                
            <asp:Panel ID="configureLicense" runat="server" Visible="false">
                    
                    <umb:PropertyPanel ID="PropertyPanel7" runat="server">
                    <p><strong>Please choose the domain that should be used for this license (without www - for instance 'mysite.com')</strong></p>
                    <p>Any subdomain will work with this license, ie. 'www.mysite.com', 'dev.mysite.com', 'staging.mysite.com'. In addition 'localhost' will always work.</p>
                    </umb:PropertyPanel>
                    
                    <umb:PropertyPanel runat="server" text="Domain">
                        <asp:TextBox ID="domainOrIp" CssClass="guiInputText guiInputStandardSize" runat="server"></asp:TextBox><br />
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="domainOrIp" runat="server" ErrorMessage="Please enter a domain"></asp:RequiredFieldValidator>
                    </umb:PropertyPanel>
                    
                    <umb:PropertyPanel runat="server" Text=" ">
                        <p>
                    <asp:Button ID="generateLicenseButton" runat="server" Text="Generate and save license" OnClick="Button1_Click" />
                        </p>
                    </umb:PropertyPanel>
                    
            </asp:Panel>
                
               
            </ContentTemplate>
        </asp:UpdatePanel>
        
    </umb:PropertyPanel>
</umb:Pane>

<umb:Pane ID="bugreportpane" runat="server" Text="Bug report" Visible="false">
    <p>
        Thank you for trying out Umbraco Contour
    </p>
    <p>
        This version of Umbraco Contour is <strong>beta software</strong>, that means we
        are not 100% done polishing the edges and some areas might be in need of some extra
        care
    </p>
    <p>
        Should you encounter any bugs or issues with using Umbraco Contour, please submit
        a bug report to us, so we can fix it as fast as possible.
    </p>
    <p>
        <button onclick="window.open('<%= Umbraco.Forms.UI.Config.BugSubmissionURL %>'); return false;">
            Submit a bug</button>
    </p>
</umb:Pane>

<umb:Pane ID="createpane" runat="server" Text="Create">
    <umb:PropertyPanel ID="PropertyPanel3" runat="server" Text="Name">
        <asp:TextBox ID="txtCreate" ValidationGroup="create" runat="server" CssClass="guiInputText guiInputStandardSize" ></asp:TextBox><asp:RequiredFieldValidator
            ControlToValidate="txtCreate" ValidationGroup="create" ErrorMessage="*" runat="server" />
    </umb:PropertyPanel>
    <umb:PropertyPanel ID="PropertyPanel4" runat="server" Text="Choose a template (optional)">
        
        <asp:ListBox ID="formTemplate" runat="server" Rows="1" CssClass="guiInputText guiInputStandardSize" SelectionMode="Single">
            <asp:ListItem Value="">None</asp:ListItem>
        </asp:ListBox>
        
    </umb:PropertyPanel>
    <umb:PropertyPanel ID="PropertyPanel5" Text=" " runat="server">
        <p>
            <asp:Button ValidationGroup="create" ID="btnCreate" runat="server" Text="Create" OnClick="btnCreate_Click" />
        </p>
    </umb:PropertyPanel>
</umb:Pane>

<umb:Pane runat="server" Text="Browse and edit" ID="pane_browse">
    <umb:PropertyPanel ID="PropertyPanel2" runat="server">
        <asp:Repeater ID="Repeater1" runat="server">
            <HeaderTemplate>
                <ul class="formList">
            </HeaderTemplate>
            <ItemTemplate>
                <li><a href="plugins/umbracocontour/editForm.aspx?guid=<%# ((Umbraco.Forms.Core.Form)Container.DataItem).Id %>"
                    class="form">
                    <%# ((Umbraco.Forms.Core.Form)Container.DataItem).Name %></a> <small><a href="plugins/umbracocontour/editForm.aspx?guid=<%# ((Umbraco.Forms.Core.Form)Container.DataItem).Id %>">
                        Open form designer</a> <a href="plugins/umbracocontour/editFormEntries.aspx?guid=<%# ((Umbraco.Forms.Core.Form)Container.DataItem).Id %>">
                            View entries</a> </small></li>
            </ItemTemplate>
            <FooterTemplate>
                </ul>
            </FooterTemplate>
        </asp:Repeater>
    </umb:PropertyPanel>
</umb:Pane>
<umb:Pane runat="server" Text="Learn">
    <umb:PropertyPanel runat="server">
        <p>
            Want to master Umbraco Contour ? Spend a couple of minutes learning som new tricks
            by watching one of the many videos about using this product. And visit <a href="http://umbraco.tv"
                target="_blank">umbraco.tv</a> for even more umbraco videos</p>
    </umb:PropertyPanel>
    <umb:PropertyPanel ID="PropertyPanel1" runat="server">
        <div id="latestformvids">
            Loading...
        </div>
    </umb:PropertyPanel>
</umb:Pane>
<small>Umbraco Contour version <%= Umbraco.Forms.Core.Configuration.Version %></small>
