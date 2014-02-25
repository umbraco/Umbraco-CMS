<%@ Control Language="c#" AutoEventWireup="True" CodeBehind="Welcome.ascx.cs" Inherits="Umbraco.Web.UI.Install.Steps.Welcome"
	TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Import Namespace="Umbraco.Core.Configuration" %>
<!-- welcome box -->
<div class="tab main-tabinfo">
	<div class="container">
        

        <asp:PlaceHolder ID="ph_install" runat="server">
                <h1>
			    Welcome to the Umbraco installation</h1>
		    <h2>
			    Thanks for downloading the Umbraco CMS installer.
		    </h2>
		    <p>
			    You are just a few minutes away from getting up and running. The installer will
			    take you through the following process:-</p>
		    <ul class="text-list">
			    <li><strong>1.</strong><span>Accept the easy to read License.</span></li>
			    <li><strong>2.</strong><span>Set up a database. There are a number of options available
				    such as MS SQL Server, MS SQL Express Edition and MYSQL or you may wish to use the
				    Microsoft SQL CE 4 database. You may need to consult your web host or system administrator.</span></li>
			    <li><strong>3.</strong><span>Set an Umbraco Admin password.</span></li>
			    <li><strong>4.</strong><span>You can then choose to install one of our great starter
				    kits and a skin.</span></li>
			    <li><strong>5.</strong><span>But whatever you do don't forget to become part of the Umbraco community, one of the friendliest developer communities you will find. It’s what makes Umbraco such a great product and so much fun to use.</span></li>
		    </ul>
		    <span class="enjoy">Enjoy!</span>
        </asp:PlaceHolder>

		 <asp:PlaceHolder ID="ph_upgrade" runat="server" Visible="false">
            <h1>Upgrading Umbraco</h1>
            <p>
                Welcome to the umbraco upgrade wizard. This will make sure that you upgrade safely from your old version to <strong>Umbraco version <%=UmbracoVersion.Current.ToString(3) %> <%=UmbracoVersion.CurrentComment %></strong> 
            </p>
            <p>
                As this is an upgrade, <strong>the wizard might skip steps</strong> that are only needed for new umbraco installations. It might also ask you questions you've already answered once. But do not worry, 
                everything is in order. Click <strong>Let's get started</strong> below to begin your upgrade.
            </p>
		<span class="enjoy">Enjoy!</span>
         </asp:PlaceHolder>

	</div>
	<!-- btn box -->
	<footer class="btn-box">
        <div class="t">&nbsp;</div>
        <asp:LinkButton ID="btnNext" CssClass="btn btn-get" runat="server" OnClick="GotoNextStep"><span>Let's get started!</span></asp:LinkButton>
    </footer>

</div>
<script type="text/javascript">
    jQuery(document).ready(function () {
        umbraco.presentation.webservices.CheckForUpgrade.InstallStatus(false, navigator.userAgent, "");
    });
</script>