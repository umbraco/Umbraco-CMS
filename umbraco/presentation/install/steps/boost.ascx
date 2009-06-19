<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="boost.ascx.cs" Inherits="umbraco.presentation.install.steps.boost" %>
<h1>Step 5/5: Install a foundation?</h1>

<asp:Panel ID="pl_boost" Runat="server" Visible="True">

<asp:RadioButton GroupName="mode" ID="rb_install" Checked="true" runat="server" style="float: left; margin: 10px;" />
<div class="rbChoice" style="float: left; padding: 0px 10px 10px 10px; width: 80%;">

<h3 style="margin-top: 10px;">Add 'Runway' - a great foundation for building websites<br />
<span style="font-size: 80%; color: #999;">Recommended</span></h3>

<div style="width: 150px; float: right; margin: 0px 0px 15px 15px; text-align: center;">
  <a target="_blank" style="color: #666; text-decoration: none;" href="http://umbraco.org/documentation/videos/getting-started/what-is-runway-and-modules">
  <img src="images/packagesVid1.png" style="border: none;" />
  <br /><span>Watch: What is Runway</span>
  </a>
</div>

<p>Runway makes it easy to build sharp websites using the latest modules from the umbraco community. You'll still have full control of design, markup and document models but with Runway you'll be ready for take off right from the beginning. </p>
<p>When you click next, you'll get a basic website and then you can choose from the latest and greatest functionality 
that's offered by our friendly community with the click of your mouse - from Galleries to FAQs, from RSS fetchers to News, from the necessities to the cream on the top.</p>

</div>
<br style="clear: both;"/>


<asp:RadioButton GroupName="mode" ID="rb_noInstall" runat="server" style="float: left; margin: 10px;" />
<div class="rbChoice" style="float: left; padding: 0px 10px 10px 10px;  width: 80%;">
<h3 style="margin-top: 10px;">No foundation, please<br />
<span style="font-size: 80%; color: #999;">Only recommended for experienced users</span></h3>
If you're an experienced umbraco user and want to setup everything manually this is the right choice. Be aware that 
you'll be unable to create content until you've setup your Document Types and Templates (<a href="http://umbraco.org/redir/runwayOnYourOwn" target="_blank">Learn how</a>).<br />
</div>

<br style="clear: both;"/>

</asp:Panel>

<asp:Panel ID="pl_nitros" Runat="server" Visible="false">
<h2>Runway is installed</h2>
<p>
Below is our list of recommended modules to get you started. If you fancy some more, toggle the <a href="#" onclick="toggleModules(); return false;" id="toggleModuleList">full list of community modules by clicking here</a>. <br /> <br />
To add modules, simply mark the ones you would like to install and click "Install selected modules" button.
</p>

<div id="nitros">
<asp:PlaceHolder ID="ph_nitros" runat="server" />
</div>

</asp:Panel>