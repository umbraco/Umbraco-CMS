<%@ Page Language="C#" AutoEventWireup="true" Inherits="Umbraco.Web.UI.Pages.UmbracoEnsuredPage" %>
<%--
    This page is required because we cannot reload the angular app with a changed Hash since it just detects the hash and doesn't reload.
    So this is used purely for a full reload of an angular app with a changed hash.
--%>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Redirecting...</title>
    <script type="text/javascript">
        
        var parts = window.location.href.split("?redirectUrl=");
        if (parts.length !== 2) {
            window.location.href = "/";
        }
        else {

            //This is a genius way of parsing a uri
            //https://gist.github.com/jlong/2428561

            try {
                var parser = document.createElement('a');
                parser.href = parts[1];

                // This next line may seem redundant but is required to get around a bug in IE 
                // that doesn't set the parser.hostname or parser.protocol correctly for relative URLs.
                // See https://gist.github.com/jlong/2428561#gistcomment-1461205
                parser.href = parser.href;

                // => "http:"
                if (!parser.protocol || (parser.protocol.toLowerCase() !== "http:" && parser.protocol.toLowerCase() !== "https:")) {
                    throw "invalid protocol";
                };

                // => "example.com"
                if (!parser.hostname || parser.hostname === "") {
                    throw "invalid hostname";
                }

                //parser.port;     // => "3000"
                //parser.pathname => "/pathname/"
                //parser.search => "?search=test"

                // => "#hash"
                if (parser.hash && parser.hash.indexOf("#/developer/framed/") !== 0) {
                    throw "invalid hash";
                }

                //parser.host;     // => "example.com:3000"
                
                if (parser.host === window.location.host) {
                    window.location.href = parts[1];
                }

            } catch (e) {
                alert(e);
            }
        }
    </script>
</head>
<body>
    <small>Redirecting...</small>
</body>
</html>
