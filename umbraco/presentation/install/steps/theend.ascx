<%@ Control Language="c#" AutoEventWireup="True" CodeBehind="theend.ascx.cs" Inherits="umbraco.presentation.install.steps.theend"
	TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<script type="text/javascript">
jQuery(document).ready(function () {

    $.post("utills/p.aspx?feed=twitter",
      function (data) {
          jQuery("#ajax-twitter").html(data);
      });
   
   $.post("utills/p.aspx?feed=blogs",
      function (data) {
          jQuery("#ajax-blogs").html(data);
      });
});

</script>


<!-- done box -->
<div class="tab main-tabinfo">
	<div class="container">
		<h1>
			You’re done...now what?</h1>
		<p>
			Excellent, you are now ready to start using Umbraco, the worlds most popular open
			source .NET CMS.
			<br />
			Now why not get configuring your new installed skin?</p>
		<ul class="btn-web">
			<li class="btn-preview-web"><a href="<%= umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) %>/canvas.aspx?redir=<%= this.ResolveUrl("~/")  %>&umbSkinning=true&umbSkinningConfigurator=true" target="_blank"><span>Set up your new website</span></a></li>
			<li class="btn-set"><a href="<%= umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) %>/umbraco.aspx"><span>Launch umbraco</span></a></li>
		</ul>
	</div>
	<div class="threcol">
		<div class="t">
			&nbsp;</div>
		<div class="hold">
			<aside class="col1">				<h2>Useful links</h2>				<p>We’ve put together some useful links to help you get started with Umbraco.</p>				<nav class="links">					<ul>						<li><a href="http://our.umbraco.org?ref=ourFromInstaller">our.umbraco.org</a></li>					</ul>
					<ul>						<li><a href="http://our.umbraco.org/wiki?ref=LatestDocsFromInstaller">New documentation</a></li>						<li><a href="http://our.umbraco.org/projects?ref=LatestProjectsFromInstaller">New Projects</a></li>						<li><a href="http://our.umbraco.org/forum?ref=LatesTalkFromInstaller">Forum Talk</a></li>					</ul>				</nav>			</aside>
			<aside class="col2">				<h2>Community blogs </h2>                <div  id="ajax-blogs"><small>Loading...</small></div>			</aside>
			<aside class="col3">			<h2>Twitter</h2>                <div id="ajax-twitter"><small>Loading...</small></div>			</aside>
		</div>
	</div>
</div>
