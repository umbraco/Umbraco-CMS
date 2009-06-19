<%@ Control Language="c#" AutoEventWireup="True" Codebehind="welcome.ascx.cs" Inherits="presentation.install.steps.welcome" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<p style="font-size: 80%; position: absolute; top: 3px; left: 15px; display: none; "><a href="#" style="color: #000; font-weight: bold" onClick="javascript:document.getElementById('detailsPane').style.display = 'block';">Where's my site?</a></p>

<div style="text-align: center; padding: 15px;">
<img src="images/logo.png" align="middle" title="Umbraco logo" alt="Umbraco logo" />
<h1>Thank you for choosing umbraco</h1>

<div id="detailsPane" class="notice" style="DISPLAY: none">
<p>
If you have successfully upgraded from a previous version of umbraco, you can skip this wizard by editing the “web.config” file by inserting a line of code (shown below) between the “appSettings” tags (please notice, that by changing this setting you accept the license for running this software as specified in the next step): <br/>
<code>
  &lt;add key="umbracoConfigurationStatus" value="<%=umbraco.GlobalSettings.CurrentVersion %>"/&gt;
</code>
</p>
</div>

<p>This wizard will guide you through the process of configuring <strong>umbraco <%=umbraco.GlobalSettings.CurrentVersion%></strong> for a fresh install or upgrading from version 3.0.</p>
<p>Press <b>"next"</b> to start the wizard.</p>

<h2>Get a great start, watch our introduction videos</h2>
<table id="videos">
<tr>
<td>
<a href="http://umbraco.org/documentation/videos/getting-started/installing-umbraco" target="_blank">
<img src="images/welcomeVid1.png" />
<span>Watch: How to install Umbraco.</span>
</a>
</td>
<td>
<a href="http://umbraco.org/documentation/videos/getting-started/what-is-umbraco" target="_blank">
<img src="images/welcomeVid2.png" />
<span>Watch: What is Umbraco?</span>
</a>
</td>
</tr>
</table>

</div>
