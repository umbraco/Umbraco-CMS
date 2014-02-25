<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="license.ascx.cs" Inherits="Umbraco.Web.UI.Install.Steps.License" %>
<!-- licence box -->
<div class="tab main-tabinfo">
	<div class="container">
		<h1>License</h1>
		<div class="accept-hold">
			<h2>Accept the license for Umbraco CMS</h2>
			<p>By clicking the "accept and continue" button (or by modifying the Umbraco Configuration Status in the web.config), you accept the license for this software as specified in the text below.</p>
		</div>
		<h3>The License (MIT):</h3>
		<div class="box-software">
			<p>Copyright (c) 2002 - <%=DateTime.Now.Year %> Umbraco I/S</p>
			<p>Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:</p>
			<p>The above copyright and this permission notice shall be included in all copies or substantial portions of the software.</p>
			<p><span>THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USER OR OTHER DEALINGS IN THE SOFTWARE.</span></p>
		</div>
		<p>That’s all. That didn’t hurt did it?</p>
	</div>
	<!-- btn box -->
	<footer class="btn-box">
		<div class="t">&nbsp;</div>
        <asp:LinkButton ID="btnNext" CssClass="btn btn-accept" runat="server" OnClick="GotoNextStep"><span>Accept and Continue</span></asp:LinkButton>
	</footer>
</div>