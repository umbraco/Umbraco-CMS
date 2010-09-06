<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="license.ascx.cs" Inherits="umbraco.presentation.install.steps.license" %>
<h2>
    Accept the license for umbraco CMS</h2>

<div class="abstract">
<p>
    By clicking the next button (or modifying the umbracoConfigurationStatus in web.config),
    you accept the license for this software as specified in the box below.</p>
</div>

<div id="licenseText">
        <a name="mit"></a>
        <h3>
            The License (MIT):</h3>
        <p>
            Copyright (c)
            2002 - <%=DateTime.Now.Year %>
            Umbraco I/S</p>
        <p>
            Permission is hereby granted, free of charge, to any person obtaining a copy of
            this software and associated documentation files (the "Software"), to deal in the
            Software without restriction, including without limitation the rights to use, copy,
            modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
            and to permit persons to whom the Software is furnished to do so, subject to the
            following conditions:</p>
        <p>
            The above copyright notice and this permission notice shall be included in all copies
            or substantial portions of the Software.</p>
        <p>
            THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
            INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
            PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
            BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
            TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
            USE OR OTHER DEALINGS IN THE SOFTWARE.</p>
       
        <p>That's all. That didn't hurt, did it?)</p>
</div>
