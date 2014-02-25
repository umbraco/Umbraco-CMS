<%@ Control Language="c#" AutoEventWireup="True" CodeBehind="TheEnd.ascx.cs" Inherits="Umbraco.Web.UI.Install.Steps.TheEnd"
	TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Import Namespace="Umbraco.Core.IO" %>
<script type="text/javascript">
jQuery(document).ready(function () {

    $.post("InstallerRestService.aspx?feed=sitebuildervids",
      function (data) {
          jQuery("#ajax-sitebuildervids").html(data);
      });

    $.post("InstallerRestService.aspx?feed=developervids",
      function (data) {
          jQuery("#ajax-developervids").html(data);
      });
    
      umbraco.presentation.webservices.CheckForUpgrade.InstallStatus(true, navigator.userAgent, "");

});

</script>


<!-- done box -->
<div class="tab main-tabinfo">
	<div class="container">
		<h1>You’re done...now what?</h1>
		<p>Excellent, you are now ready to start using Umbraco, one of the worlds most popular open source .NET CMS.</p>
		<ul class="btn-web">			
			<li class="btn-set"><a href="<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>/"><span>Launch umbraco</span></a></li>
		</ul>
	</div>
	<div class="threcol">
		<div class="t">
			&nbsp;</div>
		<div class="hold">
			<aside class="col1">
				<h2>Useful links</h2>
				<p>We’ve put together some useful links to help you get started with Umbraco.</p>
				<nav class="links">
					<ul>
                        <li><a href="http://umbraco.codeplex.com/documentation" target="_blank">Getting Started Guide</a></li>
						<li><a href="http://our.umbraco.org?ref=ourFromInstaller">our.umbraco.org</a></li>
					</ul>

					<ul>
						<li><a href="http://our.umbraco.org/wiki?ref=LatestDocsFromInstaller">New documentation</a></li>
						<li><a href="http://our.umbraco.org/projects?ref=LatestProjectsFromInstaller">New Projects</a></li>
						<li><a href="http://our.umbraco.org/forum?ref=LatesTalkFromInstaller">Forum Talk</a></li>
					</ul>
				</nav>
			</aside>
			<aside class="col2">
				<h2>Sitebuilder introduction</h2>
                <div  id="ajax-sitebuildervids"><small>Loading...</small></div>
			</aside>
			<aside class="col3">
			<h2>Developer introduction</h2>
                <div id="ajax-developervids"><small>Loading...</small></div>
			</aside>
		</div>
	</div>
</div>
