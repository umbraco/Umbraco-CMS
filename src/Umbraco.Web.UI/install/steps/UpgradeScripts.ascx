<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UpgradeScripts.ascx.cs" Inherits="Umbraco.Web.UI.Install.Steps.UpgradeScripts" %>
<div class="tab main-tabinfo">
	<div class="container">
		<h1>Upgrade scripts</h1>		
		<p>
			We need to run a few upgrade scripts, press Continue to execute these scripts and continue to the next step.
		</p>
	</div>
	<!-- btn box -->
	<footer class="btn-box">
		<div class="t">&nbsp;</div>
        <asp:LinkButton ID="btnNext" CssClass="btn btn-accept" runat="server" OnClick="RunScripts"><span>Continue</span></asp:LinkButton>
	</footer>
</div>