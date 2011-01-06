<%@ Control Language="c#" AutoEventWireup="True" CodeBehind="welcome.ascx.cs" Inherits="umbraco.presentation.install.welcome"
	TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<!-- welcome box -->
<div class="tab main-tabinfo">
	<div class="container">
		<h1>
			Welcome to the Umbraco installation</h1>
		<h2>
			Thanks for downloading the Umbraco CMS installer.
		</h2>
		<p>
			You are just a few minutes away from getting up and running. The installer will
			take you through the following process:-</p>
		<ul class="text-list">
			<li><strong>1.</strong><span>Accept the easy to read License</span></li>
			<li><strong>2.</strong><span>Set up a database. There are a number of options available
				such as MS SQL Server, MS SQL Express Edition and MYSQL or you may wish to use the
				Microsoft SQL CE 4 database. You may need to consult your ISP or System Admin.</span></li>
			<li><strong>3.</strong><span>Set an Umbraco Admin password</span></li>
			<li><strong>4.</strong><span>You can then choose to install one of our great starter
				kits and a skin</span></li>
			<li><strong>5.</strong><span>But whatever you do don't forget to become part of the Umbraco community, one of the friendliest developer communities you will find. It’s what makes Umbraco such a great product and great much fun to use.</span></li>
		</ul>
		<span class="enjoy">Enjoy!</span>
	</div>
	<!-- btn box -->
	<footer class="btn-box">
        <div class="t">&nbsp;</div>
        <asp:LinkButton ID="btnNext" CssClass="btn btn-get" runat="server" OnClick="gotoNextStep"><span>Lets get started!</span></asp:LinkButton>
    </footer>

</div>