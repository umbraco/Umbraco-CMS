<%@ Page Language="C#" %>
<%@ Import Namespace="Umbraco.Core" %>
<%@ Import Namespace="Umbraco.Core.IO" %>
<%@ Import Namespace="Umbraco.Web" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    
// This page is here purely to deal with legacy logout redirects.    
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        //We need to check the token in the URL to ensure it is correct otherwise malicious GET requests using CSRF attacks
        // can easily just log the user out.
        var token = Request["t"];
        //only perform the logout if the token matches
        if (token.IsNullOrWhiteSpace() == false && token == UmbracoContext.Current.Security.GetSessionId())
        {
            //ensure the person is definitely logged out
            UmbracoContext.Current.Security.ClearCurrentLogin();
        }
        
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Logout</title>
        <script type="text/javascript">
            //if this is not the top window, we'll assume we're in an iframe
            // so we actually won't do anything. Otherwise if this is the top window
            // we'll redirect to the login dialog
            if (window == top) {
                document.location.href = '<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco).EnsureEndsWith('/') + "#/login" %>';
            }
        </script>
    </head>
    <body>        
    </body>
</html>
