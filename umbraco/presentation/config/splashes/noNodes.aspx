<%@ Page Language="C#" AutoEventWireup="true" Codebehind="noNodes.aspx.cs" Inherits="umbraco.presentation.config.splashes.noNodes" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head runat="server">
  <title>Umbraco <%=umbraco.GlobalSettings.CurrentVersion%> - no pages found</title>
  	
  	<style type="text/css">
  			
          body{font-size: 11px; width: 100%; font-family: Trebuchet MS, verdana, arial, Lucida Grande;
          text-align: center; padding-top: 25px; margin: 0px;}
          #Panel1_content{margin-left: 1px; background: url(images/background.png) no-repeat bottom center; width: 100%;}
          #Panel1_content p{font-size: 11px; line-height: 15px;}	
          #buttons{position: absolute; bottom: 5px; width: 648px; }
          #buttons #next{float: right}
          #buttons #back{float: left}
          
          #loadingBar{visibility: hidden; padding-left: 220px;}
          #loadingBar img{width: 220px;}
           
          #contentScroll{overflow: auto; width: 100%; height: 545px; padding-right: 7px;}
          
          h1{font-size: 2em; margin-top: 5px; margin-bottom: 5px;}
          h2{font-size: 1.4em; margin-top: 5px; margin-bottom: 5px;}
          
          .error, .notice, .success {padding:.8em; padding-top: 0em; padding-bottom: 0em; margin-bottom:.5em;border:2px solid #ddd;}
          .error {background:#FBE3E4;color:#8a1f11;border-color:#FBC2C4;}
          .notice {background:#FFF6BF;color:#514721;border-color:#FFD324;}
          .success {background:#E6EFC2;color:#264409;border-color:#C6D880;}
          .error a {color:#8a1f11;}
          .notice a {color:#514721;}
          .success a {color:#264409;}
          
          #videos{padding: 0px; width: 400px; text-align: center; margin: auto; font-size: 11px;}
          #videos td{text-align: center;}
          #videos a{color: #666; text-decoration: none;}
          #videos img{padding: 1px; border: none; display: block; margin: auto;}
          #videos.single{margin: 0px;width: 150px;}
          
          #nitros p{display: none;}
          #nitros h2{font-size: 1.2em;}
          #nitros h3{font-size: 1em; margin-bottom: 0px; margin-top: 0px;}
          
          #nitros .umbNitroList input{float: left;}
          #nitros .umbNitroList div.nitro{float: left; padding: 3px 0px 5px 10px; clear: right;}
          
			  </style>
</head>
<body>

	<cc1:UmbracoClientDependencyLoader runat="server" id="ClientLoader" />
	<umb:JsInclude ID="JsInclude4" runat="server" FilePath="ui/jquery.js" PathNameAlias="UmbracoClient" Priority="0" />

<form id="Form1" method="post" runat="server">

<asp:ScriptManager runat="server" ID="umbracoScriptManager"></asp:ScriptManager>

<cc1:UmbracoPanel Style="text-align: left;" ID="Panel1" runat="server" Height="600px" AutoResize="false" Width="680px" Text="No pages found">
<div id="contentScroll">

<div style="text-align: center; padding: 15px;">
<img src="worker.png" align="middle" title="Umbraco logo" alt="Umbraco logo" />
<h1>Looks like there's still work to do</h1>

<p>
You're seeing the wonderful image above because your website doesn't contain any
<strong>published</strong> content yet.</p>
<p>
So get rid of that historic piece of web art by starting umbraco and publishing
some content. You can do this by clicking the "launch umbraco" button below.
</p>
<p>
<input type="button" value="Launch umbraco" id="bt_launch" runat="server" />
</p>

<br />
          
<h2 style="border-top: 1px solid #ccc; padding-top: 10px;">Off to a great start</h2>
<p>You can watch our intro videos on how to get off to a fast and easy start</p>

<table id="videos">
<tr>
<td>
<a target="_blank" href="http://umbraco.org/documentation/videos/getting-started/building-a-simple-site">
<img runat="server" id="vid1" src="~/install/images/packagesVid1.png" />
<span>Watch: Building a simple site.</span>
</a>
</td>
<td>
<a target="_blank" href="http://umbraco.org/documentation/videos/getting-started/using-packages">
<img runat="server" id="vid2" src="~/install/images/packagesVid2.png" />
<span>Watch: using packages</span>
</a>
</td>
</tr>
</table>
          
          
        <p>
          If you need more information on how to use umbraco, visit <a href="http://umbraco.org/documentation">
            the books section on umbraco.org</a>.
        </p>
</div>


</div>
          <div id="buttons">
          
           

          </div>
</cc1:UmbracoPanel>

</form>
</body>
</html>
