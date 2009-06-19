<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="upgrade.ascx.cs" Inherits="umbraco.presentation.install.steps.upgrade" %>

<div style="text-align: center; padding: 55px;">
<img src="images/logo.png" align="middle" title="Umbraco logo" alt="Umbraco logo" />
<h1>Upgrading Umbraco</h1>

<p>
Welcome to the umbraco upgrade wizard. This will make sure that you upgrade safely from your old version to <strong>Umbraco version <%=umbraco.GlobalSettings.CurrentVersion %></strong> 
</p>

<p>
As this is an upgrade, <strong>the wizard might skip steps</strong> that are only needed for new umbraco installations. It might also ask you questions you've already answered once. But do not worry, 
everything is in order. Click <strong>next</strong> below to begin your upgrade.
</p>

</div>