<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="license.ascx.cs" Inherits="umbraco.presentation.install.steps.license" %>
<h1>Step 1/5 Accept license</h1>
<p>By clicking the next button (or modifying the umbracoConfigurationStatus in web.config), you accept the license for this software as specified in the box below. Notice that this umbraco distribution consists of two different licenses, the open source MIT license for the framework and the umbraco freeware license that covers the UI.</p>
<div style="border: 1px solid #ccc; padding: 0px; width: 642px; height: 355px; overflow: auto;">

<div style="padding: 10px;">
<p><a href="#mit">License for the framework</a> | <a href="#umbracoLicense">License for the umbraco UI</a></p>
<p></p>
<a name="mit"></a><h3>The umbraco framework (MIT License):</h3>
<p><em>Covers files in the distribution, except the umbraco.dll and files in the /umbraco folder. However, the umbraco.library class in the umbraco.dll is also covered by this (MIT) license</em></p>
<p>Copyright (c) <%=DateTime.Now.Year %> umbraco I/S</p>

<p>Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:</p>

<p>The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.</p>

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
<p>THE SOFTWARE.</p>

<a name="umbracoLicense"></a><h3>Umbraco UI Licence:</h3>
<p><em>Covers files in the /umbraco folder and the umbraco.dll except for the umbraco.library class (which is covered by the license above - MIT).</em></p>

<p>The Umbraco UI Licence below applies to the Umbraco UI software version 3.</p>
<p>Any version of the Umbraco UI software prior to version 3 is not covered by this licence. Please refer to the licence document in such prior versions of the Umbraco UI software to find the relevant licence information.</p>
<p>Please note, that regarding the Umbraco Framework the Umbraco Framework licence applies. </p>
<p>Please also note that for the Umbraco UI the <a href="http://umbraco.org/redir/commercialLicense">Umbraco UI Commercial Licence</a> is also available.</p> 
<p><strong>The Licence</strong></p>
<p>The Umbraco UI software is copyright (c) <%=DateTime.Now.Year %> Umbraco I/S, Vesterbrogade 50, 1620 Copenhagen V, Denmark.</p>
<p>Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software, including the rights to use, copy, modify, merge, publish, distribute, sublicence, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:</p>
<ol>
<li>The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.</li>
<li>To the extent the Software contains the Umbraco name, trademark, brand and/or the Umbraco logo, any copy, modification, merger, publication, distribution or equivalent use of the Software shall retain any such names, trademarks, brand and/or logos intact.</li>

<li>THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGE OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.</li></ol>
<p>Any non-compliance with this licence agreement is to be considered a full and unconditional agreement of the Umbraco UI Commercial Licence.   </p>
<p>Any dispute which may arise between the parties, concerning this licence and/or use of the software, is to be brought before the Danish courts at the venue of Umbraco I/S. This licence shall be governed and construed in accordance with the laws of the Kingdom of Denmark.</p>
</div>
</div>
