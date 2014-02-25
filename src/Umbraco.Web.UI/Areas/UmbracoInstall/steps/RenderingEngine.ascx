<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RenderingEngine.ascx.cs" Inherits="Umbraco.Web.UI.Install.Steps.RenderingEngine" %>

<div class="tab main-tabinfo">
	<div class="container">
		<h1>
			Chose how you like to work with Templates</h1>
		<p>
		    Umbraco works with both ASP.NET WebForms (also known as MasterPages) and ASP.NET MVC (called Views). If you're not sure, we recommend using the MVC templates. You can of course use both but let's select a default one to get started.
		</p>		
	</div>
    <div class="step rendering-engine">
        <div class="container">
            <p>
                <strong>Choose a default template type:</strong>
            </p>
            <asp:RadioButtonList runat="server" ID="EngineSelection" RepeatLayout="Flow">
                <asp:ListItem Selected="True">MVC</asp:ListItem>
                <asp:ListItem>Web forms</asp:ListItem>
            </asp:RadioButtonList>                            
        </div>
    </div>
    <!-- btn box -->
	<footer class="btn-box">
		<div class="t">&nbsp;</div>
        <asp:LinkButton ID="btnNext" CssClass="btn btn-continue" runat="server" OnClick="GotoNextStep"><span>Continue</span></asp:LinkButton>
	</footer>	
</div>
